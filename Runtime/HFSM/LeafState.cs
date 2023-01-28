// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System.Collections.Generic;

namespace Edanoue.StateMachine
{
    /// <summary>
    /// StateMachine クラスの Partial Class として
    /// State クラスを実装する
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TTrigger"></typeparam>
    partial class HierarchicalStateMachine<TContext, TTrigger>
    {
        public abstract class LeafState : Node
        {
            /// <summary>
            /// 内部用のTransition Table
            /// 遷移先を辞書形式で保存している
            /// </summary>
            // ReSharper disable once InconsistentNaming
            private readonly Dictionary<TTrigger, LeafState> _transitionTable = new();

            internal sealed override void EnterInternal()
            {
                Enter();
            }

            internal sealed override void UpdateInternal()
            {
                Update();
            }

            internal sealed override void ExitInternal()
            {
                Exit();
            }

            protected virtual void Enter()
            {
            }

            protected virtual void Update()
            {
            }

            protected virtual void Exit()
            {
            }

            internal bool TryGetNextNode(TTrigger trigger, out LeafState nextNode)
            {
                return _transitionTable.TryGetValue(trigger, out nextNode);
            }

            internal bool IsAlreadyExistTransition(TTrigger trigger)
            {
                return _transitionTable.ContainsKey(trigger);
            }

            internal void AddNextNode(TTrigger trigger, LeafState nextNode)
            {
                _transitionTable[trigger] = nextNode;
            }
        }
    }
}