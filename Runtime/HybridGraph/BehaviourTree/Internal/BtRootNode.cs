// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Edanoue.HybridGraph
{
    internal sealed class BtRootNode : IRootNode
    {
        private readonly List<BtExecutableNode>   _children   = new(1);
        private          object                   _blackboard = null!;
        private          CancellationTokenSource? _runningCts;

        public int ChildCount => _children.Count;

        ICompositePort IRootNode.Add => new RootNodePort(_blackboard, _children);

        public void Dispose()
        {
            _runningCts?.Cancel();
            _runningCts?.Dispose();
        }

        internal void SetBlackboard(object blackboard)
        {
            _blackboard = blackboard;
        }

        internal void OnEnter()
        {
            if (_children.Count != 1)
            {
                throw new InvalidOperationException("Root node must have one child");
            }

            _runningCts = new CancellationTokenSource();

            UniTask.Void(async token =>
            {
                var result = await _children[0].WrappedExecuteAsync(token);
            }, _runningCts.Token);
        }

        internal void OnExit()
        {
            _runningCts?.Cancel();
            _runningCts?.Dispose();
            _runningCts = null;
        }

        internal async UniTask<BtNodeResult> ExecuteAsync(CancellationToken token)
        {
            return await _children[0].WrappedExecuteAsync(token);
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