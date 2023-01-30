// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
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

            protected internal virtual void OnUpdate(IRunningStateMachine<TTrigger> stateMachine)
            {
            }

            internal bool TryGetNextNode(TTrigger trigger, out LeafState nextNode)
            {
                return _transitionTable.TryGetValue(trigger, out nextNode);
            }

            internal void AddNextNode(TTrigger trigger, LeafState nextNode)
            {
                // prev state からは常に一種類のみの Transition が出ているべき
                if (_transitionTable.ContainsKey(trigger))
                {
                    throw new ArgumentException("既に登録済みのTriggerです");
                }

                _transitionTable[trigger] = nextNode;
            }
        }
    }
}