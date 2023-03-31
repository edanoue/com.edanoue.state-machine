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
            RootNode.SetBlackboard(Blackboard);
            OnSetupBehaviours(RootNode);
        }

        /// <summary>
        /// </summary>
        /// <param name="root"></param>
        protected abstract void OnSetupBehaviours(IRootNode root);
    }

    public abstract class BehaviourTree<TBlackboard> : BehaviourTreeBase, IGraphNode, IGraphEntryNode
    {
        /// <summary>
        /// 内部用のTransition Table
        /// 遷移先を辞書形式で保存している
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private readonly Dictionary<int, IGraphNode> _transitionTable = new();

        IGraphNode IGraphEntryNode.Run(object blackboard)
        {
            if (RootNode.ChildCount != 0)
            {
                throw new InvalidOperationException("Behaviour tree is already started.");
            }

            Blackboard = blackboard;
            SetupBehaviours();

            // Setup validation check
            if (RootNode.ChildCount != 1)
            {
                throw new InvalidOperationException("Root node must have one child.");
            }

            return this;
        }

        public void Dispose()
        {
            RootNode.Dispose();
        }

        void IGraphItem.Connect(int trigger, IGraphItem nextNode)
        {
            if (_transitionTable.ContainsKey(trigger))
            {
                throw new ArgumentException($"Already registered trigger: {trigger}");
            }

            _transitionTable.Add(trigger, nextNode.GetEntryNode());
        }

        void IGraphItem.OnInitializedInternal(object blackboard, IGraphBox parent)
        {
            if (RootNode.ChildCount != 0)
            {
                throw new InvalidOperationException("Behaviour tree is already started.");
            }

            Blackboard = blackboard;
            SetupBehaviours();

            // Setup validation check
            if (RootNode.ChildCount != 1)
            {
                throw new InvalidOperationException("Root node must have one child.");
            }
        }

        void IGraphItem.OnEnterInternal()
        {
            RootNode.OnEnter();
        }

        void IGraphItem.OnUpdateInternal()
        {
        }

        void IGraphItem.OnExitInternal(IGraphItem nextNode)
        {
            RootNode.OnExit();
        }

        IGraphNode IGraphItem.GetEntryNode()
        {
            return this;
        }

        bool IGraphNode.TryGetNextNode(int trigger, out IGraphNode nextNode)
        {
            return _transitionTable.TryGetValue(trigger, out nextNode);
        }
    }
}