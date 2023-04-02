// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Edanoue.HybridGraph
{
    public abstract class BehaviourTreeBase : BtActionNode, ICompositePort, IRootNode
    {
        private protected BtExecutableNode? _entryNode;

        internal BehaviourTreeBase() : base(null, "")
        {
        }

        void ICompositePort.AddNode(BtExecutableNode node)
        {
            if (_entryNode is not null)
            {
                throw new InvalidOperationException("Root node can only have one child");
            }

            node.Blackboard = Blackboard;
            _entryNode = node;
        }

        ICompositePort IRootNode.Add => this;

        protected sealed override async UniTask<BtNodeResult> ExecuteAsync(CancellationToken token)
        {
            if (_entryNode is null)
            {
                throw new InvalidOperationException("BehaviourTree has not entry node.");
            }

            return await _entryNode.WrappedExecuteAsync(token);
        }

        internal void SetupBehaviours()
        {
            OnSetupBehaviours(this);
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

        private IGraphBox?               _parent;
        private CancellationTokenSource? _runningCts;

        // HybridGraph.Run から直接起動された時のエントリーポイント
        IGraphNode IGraphEntryNode.Run(object blackboard)
        {
            if (_entryNode is not null)
            {
                throw new InvalidOperationException("Behaviour tree is already started.");
            }

            Blackboard = blackboard;
            SetupBehaviours();

            // Setup validation check
            if (_entryNode is null)
            {
                throw new InvalidOperationException("Root node must have one child.");
            }

            return this;
        }

        public void Dispose()
        {
            _runningCts?.Cancel();
            _runningCts?.Dispose();
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
            if (_entryNode is not null)
            {
                throw new InvalidOperationException("Behaviour tree is already started.");
            }

            _parent = parent;
            Blackboard = blackboard;
            SetupBehaviours();

            // Setup validation check
            if (_entryNode is null)
            {
                throw new InvalidOperationException("Root node must have one child.");
            }
        }

        void IGraphItem.OnEnterInternal()
        {
            if (_entryNode is null)
            {
                throw new InvalidOperationException("Root node must have one child");
            }

            _parent?.OnEnterInternal();
            OnEnter(); // call inherited callback

            _runningCts = new CancellationTokenSource();
            UniTask.Void(async token =>
            {
                var result = await _entryNode.WrappedExecuteAsync(token);
            }, _runningCts.Token);
        }

        void IGraphItem.OnUpdateInternal()
        {
            // No OP
        }

        void IGraphItem.OnExitInternal(IGraphItem nextNode)
        {
            _runningCts?.Cancel();
            _runningCts?.Dispose();
            _runningCts = null;

            OnExit(); // call inherited callback
            _parent?.OnExitInternal(nextNode);
        }

        IGraphNode IGraphItem.GetEntryNode()
        {
            return this;
        }

        bool IGraphNode.TryGetNextNode(int trigger, out IGraphNode nextNode)
        {
            return _transitionTable.TryGetValue(trigger, out nextNode);
        }

        protected virtual void OnEnter()
        {
        }

        protected virtual void OnExit()
        {
        }
    }
}