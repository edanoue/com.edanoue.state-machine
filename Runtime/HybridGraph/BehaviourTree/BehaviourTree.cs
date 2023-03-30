// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Edanoue.HybridGraph
{
    public abstract class BehaviourTreeBase : BtActionNode
    {
        private protected readonly BtRootNode RootNode = new();

        internal sealed override async UniTask<BtNodeResult> ExecuteAsync(CancellationToken token)
        {
            return await RootNode.ExecuteAsync(token);
        }

        internal void SetupBehaviours()
        {
            ((IGraphItem)RootNode).Initialize(Blackboard, null!);
            OnSetupBehaviours(RootNode);
        }

        /// <summary>
        /// </summary>
        /// <param name="root"></param>
        protected abstract void OnSetupBehaviours(IRootNode root);
    }

    public abstract class BehaviourTree<TBlackboard> : BehaviourTreeBase, IGraphBox
    {
        /// <summary>
        /// 内部用のTransition Table
        /// 遷移先を辞書形式で保存している
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private readonly Dictionary<int, IGraphNode> _transitionTable = new();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        IGraphNode IGraphItem.RootNode => RootNode;

        void IGraphItem.Initialize(object blackboard, IGraphBox? parent)
        {
            if (RootNode.ChildCount != 0)
            {
                throw new InvalidOperationException("Behaviour tree is already started.");
            }

            ((IGraphItem)RootNode).Initialize(blackboard, this);
            OnSetupBehaviours(RootNode);

            // Setup validation check
            if (RootNode.ChildCount != 1)
            {
                throw new InvalidOperationException("Root node must have one child.");
            }
        }

        void IGraphItem.Connect(int trigger, IGraphItem nextNode)
        {
            if (_transitionTable.ContainsKey(trigger))
            {
                throw new ArgumentException($"Already registered trigger: {trigger}");
            }

            _transitionTable.Add(trigger, nextNode.RootNode);
        }

        void IGraphItem.OnEnterInternal()
        {
        }

        void IGraphItem.OnUpdateInternal()
        {
        }

        void IGraphItem.OnExitInternal(IGraphItem nextNode)
        {
        }

        bool IGraphBox.IsDescendantNode(IGraphItem node)
        {
            return false;
        }
    }
}