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
        private protected BtExecutableNode? EntryNode;

        internal BehaviourTreeBase() : base(null, "")
        {
            // No OP
        }

        void ICompositePort.AddNodeAndSetBlackboard(BtExecutableNode node)
        {
            if (EntryNode is not null)
            {
                throw new InvalidOperationException("Root node can only have one child");
            }

            node.SetBlackboardRaw(BlackboardRaw);
            EntryNode = node;
        }


        ICompositePort IRootNode.Add => this;

        protected sealed override async UniTask<BtNodeResult> ExecuteAsync(CancellationToken token)
        {
            // Entry Node が設定されていない場合は Failed を返す
            if (EntryNode is null)
            {
                return BtNodeResult.Failed;
            }

            OnEnter();

            return await EntryNode.WrappedExecuteAsync(token);
        }

        /// <summary>
        /// </summary>
        /// <param name="root"></param>
        protected internal abstract void OnSetupBehaviours(IRootNode root);

        /// <summary>
        /// BehaviourTree に Enter した際に呼ばれるコールバック
        /// </summary>
        /// <remarks>
        /// 以下のタイミングで呼ばれます
        /// <para>- BehaviourTree を直接 <see cref="EdaGraph.Run" /> した際</para>
        /// <para>- StateMachine 内部で LeafState として Enter した際</para>
        /// <para>- BehaviourTree(ActionNode) として Enter した際</para>
        /// </remarks>
        protected virtual void OnEnter()
        {
        }
    }

    /// <summary>
    /// HybridGraph で動作する BehaviourTree の基底クラス
    /// そのまま Run したり, StateMachine の LeafState として取り扱うことができる
    /// </summary>
    /// <typeparam name="TBlackboard"></typeparam>
    public abstract class BehaviourTree<TBlackboard> : BehaviourTreeBase, IGraphNode, IGraphEntryNode
    {
        /// <summary>
        /// 内部用のTransition Table
        /// 遷移先を辞書形式で保存している
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private readonly Dictionary<int, IGraphNode> _transitionTable = new();

        private   IGraphBox?               _parent;
        private   CancellationTokenSource? _runningCts;
        protected TBlackboard              Blackboard = default!;

        // HybridGraph.Run から直接起動された時のエントリーポイント
        IGraphNode IGraphEntryNode.Run(object blackboard)
        {
            if (EntryNode is not null)
            {
                throw new InvalidOperationException("Behaviour tree is already started.");
            }

            SetBlackboardRaw(blackboard);
            OnSetupBehaviours(this);

            return this;
        }

        public void Dispose()
        {
            _runningCts?.Cancel();
            _runningCts?.Dispose();

            OnDestroy();
        }

        void IGraphItem.Connect(int trigger, IGraphItem nextNode)
        {
            if (_transitionTable.ContainsKey(trigger))
            {
                throw new ArgumentException($"Already registered trigger: {trigger}");
            }

            _transitionTable.Add(trigger, nextNode.GetEntryNode());
        }

        // StateMachine の中に組み込まれる際に呼ばれるエントリーポイント
        void IGraphItem.OnInitializedInternal(object blackboard, IGraphBox parent)
        {
            if (EntryNode is not null)
            {
                throw new InvalidOperationException("Behaviour tree is already started.");
            }

            _parent = parent;
            SetBlackboardRaw(blackboard);
            OnSetupBehaviours(this);
        }

        // 直接 Run 及び StateMachine のコンテキストでBTに遷移した際に呼ばれるエントリーポイント
        void IGraphItem.OnEnterInternal()
        {
            _parent?.OnEnterInternal();

            _runningCts = new CancellationTokenSource();

            UniTask.Void(async token =>
            {
                var result = await WrappedExecuteAsync(token);
                OnEndExecute(token.IsCancellationRequested ? BtNodeResult.Cancelled : result);
            }, _runningCts.Token);
        }

        void IGraphItem.OnUpdateInternal()
        {
            // No OP
        }

        void IGraphItem.OnExitInternal(IGraphItem nextNode)
        {
            // Cancel Running CTS
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

        internal sealed override void SetBlackboardRaw(object blackboardRaw)
        {
            base.SetBlackboardRaw(blackboardRaw);
            Blackboard = (TBlackboard)blackboardRaw;
        }

        /// <summary>
        /// </summary>
        protected virtual void OnExit()
        {
        }

        /// <summary>
        /// </summary>
        protected virtual void OnDestroy()
        {
        }

        protected virtual void OnEndExecute(BtNodeResult result)
        {
        }
    }
}