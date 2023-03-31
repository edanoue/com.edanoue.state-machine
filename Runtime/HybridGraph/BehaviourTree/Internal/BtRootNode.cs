// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Edanoue.HybridGraph
{
    internal sealed class BtRootNode : IRootNode
    {
        private readonly List<BtExecutableNode> _children   = new(1);
        private          object                 _blackboard = null!;

        public int ChildCount => _children.Count;


        ICompositePort IRootNode.Add => new RootNodePort(_blackboard, _children);

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        internal void SetBlackboard(object blackboard)
        {
            _blackboard = blackboard;
        }

        internal void OnEnterInternal()
        {
            if (_children.Count != 1)
            {
                throw new InvalidOperationException("Root node must have one child");
            }

            UniTask.Void(async token =>
            {
                var childNode = _children[0];
                var stopWatch = new Stopwatch();
                while (true)
                {
                    stopWatch.Restart();
                    var result = await childNode.ExecuteAsync(token);
                    stopWatch.Stop();
                    if (DoDecoratorsAllowExit())
                    {
                        break;
                    }

                    // 無限ループ(によるハング)防止用の Await 処理
                    // TODO: ここでの最低の待機感覚, Global に設定できるようにするか, Decorator ごとに設定できるようにするべき
                    var elapsedMilliseconds = stopWatch.ElapsedMilliseconds;
                    if (elapsedMilliseconds < 100)
                    {
                        await UniTask.Delay(TimeSpan.FromMilliseconds(100 - elapsedMilliseconds),
                            cancellationToken: token);
                    }
                }
            }, default); // TODO: token
        }

        private bool DoDecoratorsAllowExit()
        {
            var node = _children[0];

            // No decorators, allow enter
            if (node.Decorators.Count == 0)
            {
                return true;
            }

            for (var decoratorIndex = 0; decoratorIndex < node.Decorators.Count; decoratorIndex++)
            {
                var decorator = node.Decorators[decoratorIndex];
                if (decorator.CanExit())
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        internal async UniTask<BtNodeResult> ExecuteAsync(CancellationToken token)
        {
            return await _children[0].ExecuteAsync(token);
        }

        private sealed class RootNodePort : ICompositePort
        {
            private readonly object                 _blackboard;
            private readonly List<BtExecutableNode> _children;

            public RootNodePort(object blackboard, List<BtExecutableNode> children)
            {
                _blackboard = blackboard;
                _children = children;
            }

            void ICompositePort.AddNode(BtExecutableNode node, string nodeName)
            {
                if (_children.Count > 0)
                {
                    throw new InvalidOperationException("Root node can only have one child");
                }

                node.Blackboard = _blackboard;
                node.NodeName = nodeName;
                _children.Add(node);
            }
        }
    }
}