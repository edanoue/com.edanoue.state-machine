// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;

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
        /// <summary>
        /// State を表現するクラス
        /// </summary>
        public abstract class Node : IDisposable
        {
            // ReSharper disable once InconsistentNaming
            internal GroupState? _parent;

            // StateMachine での生成時 直接代入される
            // ReSharper disable once InconsistentNaming
            internal HierarchicalStateMachine<TContext, TTrigger> _stateMachine = null!;

            protected internal Node()
            {
            }

            /// <summary>
            /// 自身を管理するStateMachineの持つコンテキストへの参照
            /// </summary>
            protected TContext Context => _stateMachine.Context;

            public virtual void Dispose()
            {
            }

            internal void OnEnterStateInternal(IRunningStateMachine<TTrigger> stateMachine)
            {
                // 親が存在していたら(GroupState に所属していたら) 親の OnEnter を呼ぶ
                if (_parent is not null)
                {
                    // 何回も呼ばれるのを防止するための ロックの確認を行う
                    if (!_parent._enterLock)
                    {
                        _parent.OnEnterStateInternal(stateMachine);
                        // 一度親の OnEnter を呼んだらロックをこちらから掛けておく
                        _parent._enterLock = true;
                    }
                }

                OnEnterState(stateMachine);
            }

            internal void OnExitStateInternal(LeafState nextState)
            {
                OnExitState();

                // 親が存在していたら(GroupState に所属していたら)
                if (_parent is not null)
                {
                    // この時点で 遷移先は確定しているため, 親 の子孫に当たるかどうかを確認する
                    if (!_parent.IsDescendantState(nextState))
                    {
                        _parent.OnExitStateInternal(nextState);
                        _parent._enterLock = false;
                    }
                }
            }

            protected virtual void OnEnterState(IRunningStateMachine<TTrigger> stateMachine)
            {
            }

            protected virtual void OnExitState()
            {
            }
        }
    }
}