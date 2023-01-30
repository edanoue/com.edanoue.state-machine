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
            // ReSharper disable once InconsistentNaming
            internal readonly Dictionary<TTrigger, State> _transitionTable = new();

            /// <summary>
            /// </summary>
            // ReSharper disable once InconsistentNaming
            internal StateMachine<TContext, TTrigger> _stateMachine = null!;

            protected internal State()
            {
            }

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
            protected internal virtual void OnEnter(IRunningStateMachine<TTrigger> stateMachine)
            {
            }

            /// <summary>
            /// <see cref="StateMachine{TContext,TTrigger}.UpdateState"/>> を呼ばれた際に, 遷移先が決定していない場合に呼ばれる関数です
            /// </summary>
            protected internal virtual void OnUpdate(IRunningStateMachine<TTrigger> stateMachine)
            {
            }

            /// <summary>
            /// ステートを抜ける際に一度だけ呼ばれる関数です
            /// 既に遷移先が仮で決定しており, この 関数が終わると次の State に遷移します.
            /// ここで遷移先を変更すると, 仮で決定している遷移先を上書きします.
            /// </summary>
            protected internal virtual void OnExit(IRunningStateMachine<TTrigger> stateMachine)
            {
            }
        }
    }
}