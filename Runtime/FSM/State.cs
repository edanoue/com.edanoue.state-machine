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
    partial class StateMachine<TContext, TTrigger>
    {
        /// <summary>
        /// State を表現するクラス
        /// </summary>
        public abstract class State : IDisposable
        {
            /// <summary>
            /// 内部用のTransition Table
            /// 遷移先を辞書形式で保存している
            /// StateMachine から代入される
            /// </summary>
            internal readonly Dictionary<TTrigger, State> TransitionTable = new();

            /// <summary>
            /// </summary>
            internal StateMachine<TContext, TTrigger> _stateMachine = null!;

            protected internal State()
            {
            }

            /// <summary>
            /// 自身を管理するStateMachineへの参照
            /// </summary>
            protected ITriggerReceiver<TTrigger> StateMachine => _stateMachine;


            /// <summary>
            /// 自身を管理するStateMachineの持つコンテキストへの参照
            /// </summary>
            protected TContext Context => _stateMachine.Context;

            /// <summary>
            /// </summary>
            public virtual void Dispose()
            {
            }

            /// <summary>
            /// ステート突入時に一度だけ呼ばれる関数
            /// </summary>
            protected internal virtual void Enter()
            {
            }

            /// <summary>
            /// ステート更新時に呼ばれる関数
            /// </summary>
            protected internal virtual void Update()
            {
            }

            /// <summary>
            /// ステートを抜ける際に一度だけ呼ばれる関数
            /// </summary>
            protected internal virtual void Exit()
            {
            }
        }
    }
}