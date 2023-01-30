// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Collections.Generic;

namespace Edanoue.StateMachine
{
    /// <summary>
    /// StateMachineクラス
    /// </summary>
    /// <typeparam name="TContext">コンテキストの型</typeparam>
    /// <typeparam name="TTrigger">トリガーの型</typeparam>
    public partial class StateMachine<TContext, TTrigger> :
        IRunningStateMachine<TTrigger>,
        IDisposable
    {
        private readonly HashSet<State> _stateList = new();

        // ReSharper disable once InconsistentNaming
        private protected State? _currentState;
        private           State? _nextState;

        /// <summary>
        /// StateMachine の初期化を行う
        /// </summary>
        /// <param name="context"></param>
        public StateMachine(TContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // メンバーの初期化を行う
            Context = context;
        }

        /// <summary>
        /// ステートマシンが所有しているコンテキスト
        /// State からアクセスされる
        /// </summary>
        /// <value></value>
        private TContext Context { get; }

        /// <summary>
        /// 現在StateMachine が起動中かどうか
        /// </summary>
        private bool IsRunning => _currentState is not null;

        public void Dispose()
        {
            foreach (var state in _stateList)
            {
                state.Dispose();
            }

            _stateList.Clear();
            _currentState = null;
            _nextState = null;
        }

        /// <summary>
        /// State Machine に 発生したTrigger を送信する関数
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns>現在のStateに指定のTriggerが登録されていればtrue</returns>
        public bool SendTrigger(TTrigger trigger)
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException("ステートマシンはまだ起動していません.");
            }

            // 現在の State の transitionMap を見て, 移行先のStateが存在する場合, nextState を更新する
            var foundNextState = _currentState!._transitionTable.TryGetValue(trigger, out var nextState);
            if (foundNextState)
            {
                _nextState = nextState;
            }

            return foundNextState;
        }

        /// <summary>
        /// StateMachineを更新する関数
        /// </summary>
        public void UpdateState()
        {
            // 初回のみ呼ばれる部分
            if (!IsRunning)
            {
                // ステートを切り替える
                // 初期ステートが設定されていないならエラー
                _currentState = _nextState ?? throw new InvalidOperationException("開始ステートが設定されていません");
                _nextState = null;

                // ステートを開始する
                _currentState.OnEnter(this);

                // ここですでに次のステートが決定している可能性がある
                // まだ決定していない場合は処理を抜ける
                if (_nextState is null)
                {
                    return;
                }
            }

            // 次のState が確定していない場合のみ, Update が呼ばれる
            if (_nextState is null)
            {
                // 現在のStateのUpdate関数を呼ぶ
                _currentState!.OnUpdate(this);
            }

            // 次の遷移先が代入されていたら, ステートを切り替える
            while (_nextState is not null)
            {
                // 以前のステートを終了する
                _currentState!.OnExit(this);

                // ステートの切り替え処理
                _currentState = _nextState;
                _nextState = null;

                // 次のステートを開始する
                _currentState.OnEnter(this);
            }
        }

        /// <summary>
        /// 指定した State が現在の State なら True
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsCurrentState<T>()
            where T : State
        {
            return _currentState is T;
        }

        /// <summary>
        /// State Machine に State同士の Transition を設定する関数
        /// State Machine の初期設定で必ず呼ぶ必要がある
        /// </summary>
        /// <param name="trigger">Stateが移動する原因となるTrigger</param>
        /// <typeparam name="TPrevState">起点となるState</typeparam>
        /// <typeparam name="TNextState">終点となるState</typeparam>
        /// <returns></returns>
        public void AddTransition<TPrevState, TNextState>(TTrigger trigger)
            where TPrevState : State, new()
            where TNextState : State, new()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("すでに起動中のStateMachineです");
            }

            // 同じ State 同士の遷移を許可する
            /*
            if (typeof(TPrevState) == typeof(TNextState))
            {
                throw new ArgumentException("TPrevState と TNextState が同じです.");
            }
            */

            // Stateのインスタンスを取得する
            // まだStateが内部で作成されていなければ, このときに生成を行う
            var prevState = GetOrCreateState<TPrevState>();
            var nextState = GetOrCreateState<TNextState>();

            // prev state からは常に一種類のみの Transition が出ているべき
            if (prevState._transitionTable.ContainsKey(trigger))
            {
                throw new ArgumentException("既に登録済みのTriggerです");
            }

            // prev state にTransition を登録
            prevState._transitionTable[trigger] = nextState;
        }

        /// <summary>
        /// State Machine の初期Stateを設定する関数
        /// State Machine の初期設定で必ず呼ぶ必要がある
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        public void SetInitialState<TState>()
            where TState : State, new()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("すでに起動中のStateMachineです");
            }

            _nextState = GetOrCreateState<TState>();
        }

        /// <summary>
        /// 既に生成済みの State ならそれを, なければ新規生成して返す関数
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        private TState GetOrCreateState<TState>()
            where TState : State, new()
        {
            foreach (var state in _stateList)
            {
                if (state is TState t)
                {
                    return t;
                }
            }

            // State の新規作成
            var newState = new TState
            {
                _stateMachine = this
            };
            _stateList.Add(newState);
            return newState;
        }
    }
}