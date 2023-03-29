// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.HybridGraph
{
    public abstract class BehaviourTree<TBlackboard> : IGraphBox
    {
        private readonly BtRootNode  _rootNode = new();
        private          IGraphBox?  _parent;
        protected        TBlackboard Blackboard = default!;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        IGraphNode IGraphItem.RootNode => _rootNode;

        void IGraphItem.Initialize(object blackboard, IGraphBox? parent)
        {
            if (_rootNode.ChildCount != 0)
            {
                throw new InvalidOperationException("Behaviour tree is already started.");
            }

            Blackboard = (TBlackboard)blackboard ?? throw new ArgumentNullException(nameof(blackboard));
            _parent = parent;
            ((IGraphItem)_rootNode).Initialize(blackboard, this);
            OnSetupBehaviours(_rootNode);

            // Setup validation check
            if (_rootNode.ChildCount != 1)
            {
                throw new InvalidOperationException("Root node must have one child.");
            }
        }

        void IGraphItem.Connect(int trigger, IGraphItem nextNode)
        {
            throw new NotImplementedException();
        }

        void IGraphItem.OnEnterInternal()
        {
            throw new NotImplementedException();
        }

        void IGraphItem.OnUpdateInternal()
        {
            throw new NotImplementedException();
        }

        void IGraphItem.OnExitInternal(IGraphItem nextNode)
        {
            throw new NotImplementedException();
        }

        bool IGraphBox.IsDescendantNode(IGraphItem node)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// </summary>
        /// <param name="root"></param>
        protected abstract void OnSetupBehaviours(IRootNode root);
    }
}