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
    public partial class StateMachine<TContext, TTrigger> : IDisposable
    {
        private readonly HashSet<State> _stateList = new();
        private          State?         _nextState;
        private          State?         _prevState;

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
        private bool IsRunning => CurrentState is not null;

        /// <summary>
        /// 現在のStateの名前を取得
        /// </summary>
        /// <returns></returns>
        public string CurrentStateName => IsRunning ? CurrentState!.Name : string.Empty;

        /// <summary>
        /// 現在の State を取得 (継承先のクラス用)
        /// </summary>
        protected State? CurrentState { get; private set; }

        public void Dispose()
        {
            foreach (var state in _stateList)
            {
                state.Dispose();
            }

            _stateList.Clear();
            _prevState = null;
            CurrentState = null;
            _nextState = null;
        }

        /// <summary>
        /// 指定した State が現在の State なら True
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsCurrentState<T>()
            where T : State
        {
            return typeof(T) == CurrentState?.GetType();
        }

        /// <summary>
        /// 指定した State が一つ前の State なら True
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsPrevState<T>()
            where T : State
        {
            return typeof(T) == _prevState?.GetType();
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

            if (typeof(TPrevState) == typeof(TNextState))
            {
                throw new ArgumentException("TPrevState と TNextState が同じです.");
            }

            // Stateのインスタンスを取得する
            // まだStateが内部で作成されていなければ, このときに生成を行う
            var prevState = GetOrCreateState<TPrevState>();
            var nextState = GetOrCreateState<TNextState>();

            // prev state からは常に一種類のみの Transition が出ているべき
            if (prevState.TransitionTable.ContainsKey(trigger))
            {
                throw new ArgumentException("既に登録済みのTriggerです");
            }

            // prev state にTransition を登録
            prevState.TransitionTable[trigger] = nextState;
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
        /// StateMachineを更新する関数
        /// </summary>
        public void UpdateState(float deltaTime = 0f)
        {
            // 初回のみ呼ばれる部分
            if (!IsRunning)
            {
                // ステートを切り替える
                // 初期ステートが設定されていないならエラー
                CurrentState = _nextState ?? throw new InvalidOperationException("開始ステートが設定されていません");
                _prevState = null;
                _nextState = null;

                // ステートを開始する
                // エラーが発生する可能性がある
                CurrentState.Enter();

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
                CurrentState!.Update(deltaTime);
            }

            // 次の遷移先が代入されていたら, ステートを切り替える
            while (_nextState is not null)
            {
                // 以前のステートを終了する
                CurrentState!.Exit();

                // ステートの切り替え処理
                _prevState = CurrentState;
                CurrentState = _nextState;
                _nextState = null;

                // 次のステートを開始する
                CurrentState.Enter();
            }
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
            return CurrentState!.TransitionTable.TryGetValue(trigger, out _nextState);
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
                StateMachine = this
            };
            _stateList.Add(newState);
            return newState;
        }
    }
}