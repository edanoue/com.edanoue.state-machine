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
        public abstract class GroupState : Node
        {
            private readonly List<Node> _childNodeList = new();
            private          Node?      _initialState;

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

            internal void GetAllChildLeafState(ref List<LeafState> leafStates)
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

            internal sealed override void EnterInternal()
            {
            }

            internal sealed override void UpdateInternal()
            {
            }

            internal sealed override void ExitInternal()
            {
            }

            protected internal abstract void SetupSubStates();

            /// <summary>
            /// </summary>
            /// <param name="trigger"></param>
            /// <typeparam name="TPrevState"></typeparam>
            /// <typeparam name="TNextState"></typeparam>
            /// <exception cref="InvalidOperationException"></exception>
            /// <exception cref="ArgumentException"></exception>
            protected void AddTransition<TPrevState, TNextState>(TTrigger trigger)
                where TPrevState : Node, new()
                where TNextState : Node, new()
            {
                // State Machine のものを登録しておく
                _stateMachine.AddTransition<TPrevState, TNextState>(trigger);

                // State Machine から生成された State の参照を取得
                var prevState = _stateMachine.GetState<TPrevState>();
                var nextState = _stateMachine.GetState<TNextState>();

                // 自身の子として登録しておく
                if (!_childNodeList.Contains(prevState))
                {
                    _childNodeList.Add(prevState);
                }

                if (!_childNodeList.Contains(nextState))
                {
                    _childNodeList.Add(nextState);
                }
            }

            /// <summary>
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <exception cref="InvalidOperationException"></exception>
            protected void SetInitialState<T>()
                where T : Node
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
        }
    }
}