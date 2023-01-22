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
    partial class StateMachine<TContext, TTrigger>
    {
        /// <summary>
        /// State を表現するクラス
        /// </summary>
        public abstract class State
        {
            /// <summary>
            /// 内部用のTransition Table
            /// 遷移先を辞書形式で保存している
            /// StateMachine から代入される
            /// </summary>
            internal readonly Dictionary<TTrigger, State> TransitionTable;

            /// <summary>
            /// 自身を管理するStateMachineへの参照
            /// </summary>
            /// <remarks>
            /// Generics の都合上コンストラクタからは初期化されず, StateMachineから直接代入される
            /// </remarks>
            protected internal StateMachine<TContext, TTrigger> StateMachine = null!;

            protected internal State()
            {
                TransitionTable = new Dictionary<TTrigger, State>();
            }

            /// <summary>
            /// 自身を管理するStateMachineの持つコンテキストへの参照
            /// </summary>
            protected TContext Context => StateMachine.Context;

            /// <summary>
            /// State の名前を取得
            /// </summary>
            /// <value>Stateの名前</value>
            public virtual string Name => GetType().Name;

            /// <summary>
            /// ステート突入時に一度だけ呼ばれる関数
            /// </summary>
            protected internal virtual void Enter()
            {
            }

            /// <summary>
            /// ステート更新時に呼ばれる関数
            /// </summary>
            protected internal virtual void Update(float deltaTime)
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