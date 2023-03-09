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
        public abstract class GroupState :
            Node,
            IStateBuilder<TContext, TTrigger>
        {
            private readonly HashSet<Node> _childNodeList = new();
            internal         bool          _enterLock;
            private          Node?         _initialState;

            /// <summary>
            /// </summary>
            /// <param name="trigger"></param>
            /// <typeparam name="TPrevState"></typeparam>
            /// <typeparam name="TNextState"></typeparam>
            void IStateBuilder<TContext, TTrigger>.AddTransition<TPrevState, TNextState>(TTrigger trigger)
            {
                _stateMachine.AddTransition<TPrevState, TNextState>(trigger);

                // State Machine から生成された State の参照を取得
                var prevState = _stateMachine.GetState<TPrevState>();
                var nextState = _stateMachine.GetState<TNextState>();

                // 自身の子として登録しておく
                if (!_childNodeList.Contains(prevState))
                {
                    _childNodeList.Add(prevState);
                    prevState._parent = this;
                }

                if (!_childNodeList.Contains(nextState))
                {
                    _childNodeList.Add(nextState);
                    nextState._parent = this;
                }
            }

            /// <summary>
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <exception cref="InvalidOperationException"></exception>
            void IStateBuilder<TContext, TTrigger>.SetInitialState<T>()
            {
                if (_stateMachine.IsRunning)
                {
                    throw new InvalidOperationException("すでに起動中のStateMachineです");
                }

                // StateMachine 本体から参照を取得する
                var initialState = _stateMachine.GetState<T>();

                if (!_childNodeList.Contains(initialState))
                {
                    throw new InvalidOperationException("設定した State は自身の子のStateではありません.");
                }

                _initialState = initialState;
            }

            internal bool IsDescendantState(LeafState state)
            {
                foreach (var child in _childNodeList)
                {
                    if (child == state)
                    {
                        return true;
                    }

                    if (child is not GroupState gs)
                    {
                        continue;
                    }

                    if (gs.IsDescendantState(state))
                    {
                        return true;
                    }
                }

                return false;
            }

            internal LeafState GetInitialState()
            {
                if (_initialState is null)
                {
                    throw new InvalidOperationException("Initial State が設定されていません");
                }

                if (_initialState is LeafState ls)
                {
                    return ls;
                }

                if (_initialState is GroupState groupState)
                {
                    // GroupState の場合は再帰的に呼ぶ
                    return groupState.GetInitialState();
                }

                throw new InvalidOperationException("InitialState の解決に失敗しました");
            }

            internal void GetAllChildLeafState(ref HashSet<LeafState> leafStates)
            {
                foreach (var childNode in _childNodeList)
                {
                    switch (childNode)
                    {
                        case LeafState ls:
                            leafStates.Add(ls);
                            break;
                        case GroupState gs:
                            gs.GetAllChildLeafState(ref leafStates);
                            break;
                    }
                }
            }

            /// <summary>
            /// この GroupState 内部の遷移を構築する.
            /// </summary>
            /// <param name="builder"></param>
            protected internal abstract void SetupSubStates(IStateBuilder<TContext, TTrigger> builder);
        }
    }
}