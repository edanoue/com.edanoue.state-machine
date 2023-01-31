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

            internal void OnEnterInternal(IRunningStateMachine<TTrigger> stateMachine)
            {
                if (_parent is not null)
                {
                    if (!_parent._enterLock)
                    {
                        _parent.OnEnterInternal(stateMachine);
                        _parent._enterLock = true;
                    }
                }

                OnEnter(stateMachine);
            }

            internal void OnExitInternal(IRunningStateMachine<TTrigger> stateMachine)
            {
                OnExit(stateMachine);

                if (_parent is not null)
                {
                    if (_parent._enterLock)
                    {
                        _parent.OnExitInternal(stateMachine);
                        _parent._enterLock = false;
                    }
                }
            }

            protected virtual void OnEnter(IRunningStateMachine<TTrigger> stateMachine)
            {
            }

            protected virtual void OnExit(IRunningStateMachine<TTrigger> stateMachine)
            {
            }
        }
    }
}