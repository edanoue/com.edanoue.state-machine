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
            // StateMachine での生成時 直接代入される
            // ReSharper disable once InconsistentNaming
            internal HierarchicalStateMachine<TContext, TTrigger> _stateMachine = null!;

            protected internal Node()
            {
            }

            public virtual void Dispose()
            {
            }
        }
    }
}