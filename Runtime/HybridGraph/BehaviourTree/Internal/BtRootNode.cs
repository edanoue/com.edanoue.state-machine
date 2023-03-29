// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Edanoue.HybridGraph
{
    internal sealed class BtRootNode : IGraphNode, IRootNode
    {
        private readonly List<BtExecutableNode> _children   = new(1);
        private          object                 _blackboard = null!;

        public int ChildCount => _children.Count;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        IGraphNode IGraphItem.RootNode => throw new NotImplementedException();

        void IGraphItem.Initialize(object blackboard, IGraphBox? parent)
        {
            // TODO:
            _blackboard = blackboard;
        }

        void IGraphItem.Connect(int trigger, IGraphItem nextNode)
        {
            throw new NotImplementedException();
        }

        void IGraphItem.OnEnterInternal()
        {
            if (_children.Count != 1)
            {
                throw new InvalidOperationException("Root node must have one child");
            }

            UniTask.Void(async token =>
            {
                var childNode = _children[0];
                var result = await childNode.ExecuteAsync(token);
            }, default); // TODO: token
        }

        void IGraphItem.OnUpdateInternal()
        {
        }

        void IGraphItem.OnExitInternal(IGraphItem nextNode)
        {
        }

        bool IGraphNode.TryGetNextNode(int trigger, out IGraphNode nextNode)
        {
            throw new NotImplementedException();
        }

        ICompositePort IRootNode.Add => new RootNodePort(_blackboard, _children);

        private sealed class RootNodePort : ICompositePort
        {
            private readonly object                 _blackboard;
            private readonly List<BtExecutableNode> _children;

            public RootNodePort(object blackboard, List<BtExecutableNode> children)
            {
                _blackboard = blackboard;
                _children = children;
            }

            void ICompositePort.AddNode(BtExecutableNode node)
            {
                if (_children.Count > 0)
                {
                    throw new InvalidOperationException("Root node can only have one child");
                }

                node.SetBlackboard(_blackboard);
                _children.Add(node);
            }
        }
    }
}