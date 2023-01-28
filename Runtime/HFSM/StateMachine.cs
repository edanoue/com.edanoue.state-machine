// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Collections.Generic;

namespace Edanoue.StateMachine
{
    /// <summary>
    /// 階層構造を取れる StateMachine
    /// </summary>
    /// <typeparam name="TContext">コンテキストの型</typeparam>
    /// <typeparam name="TTrigger">トリガーの型</typeparam>
    public partial class HierarchicalStateMachine<TContext, TTrigger> :
        ITriggerReceiver<TTrigger>,
        IDisposable
    {
        private readonly List<Node> _stateList = new();
        private          LeafState? _currentState;
        private          LeafState? _nextState;

        /// <summary>
        /// StateMachine の初期化を行う
        /// </summary>
        /// <param name="context"></param>
        public HierarchicalStateMachine(TContext context)
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
            return _currentState!.TryGetNextNode(trigger, out _nextState);
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
                _currentState.EnterInternal();

                // ここですでに次のステートが決定している可能性がある
                // まだ決定していない場合は処理を抜ける
                if (_nextState is null)
                {
                    return;
                }
            }

            // 以降の処理では CurrentState は notnull
            // 次のState が確定していない場合のみ, Update が呼ばれる
            if (_nextState is null)
            {
                // 現在のStateのUpdate関数を呼ぶ
                _currentState!.UpdateInternal();
            }

            // 次の遷移先が代入されていたら, ステートを切り替える
            while (_nextState is not null)
            {
                // 以前のステートを終了する
                _currentState!.ExitInternal();

                // ステートの切り替え処理
                _currentState = _nextState;
                _nextState = null;

                // 次のステートを開始する
                _currentState.EnterInternal();
            }
        }

        public bool SendTriggerAndUpdateState(TTrigger trigger)
        {
            if (!SendTrigger(trigger))
            {
                return false;
            }

            UpdateState();
            return true;
        }

        /// <summary>
        /// 指定した State が現在の State なら True
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsCurrentState<T>()
            where T : LeafState
        {
            return typeof(T) == _currentState?.GetType();
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
            where TPrevState : Node, new()
            where TNextState : Node, new()
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

            // PrevState が LeafState のばあい
            if (prevState is LeafState prevLeafState)
            {
                switch (nextState)
                {
                    // NextState が GroupState のばあい
                    case GroupState nextGroupState:
                    {
                        // NextState から GroupState の InitialState に向けて Transition を貼る
                        var nextInitialState = nextGroupState.GetInitialState();
                        AddTransition(prevLeafState, nextInitialState, trigger);
                        break;
                    }
                    // NextState が LeafState のばあい
                    case LeafState nextLeafState:
                    {
                        AddTransition(prevLeafState, nextLeafState, trigger);
                        break;
                    }
                }
            }

            // PrevState が Group State のばあい
            else if (prevState is GroupState groupPrevState)
            {
                // GroupState に含まれるすべてのノードからの Transition を作成する
                var allPrevLeafStates = new List<LeafState>();
                groupPrevState.GetAllChildLeafState(ref allPrevLeafStates);

                switch (nextState)
                {
                    // NextState が GroupState のばあい
                    case GroupState nextGroupState:
                    {
                        // NextState から GroupState の InitialState に向けて Transition を貼る
                        var nextGroupInitialState = nextGroupState.GetInitialState();
                        foreach (var p in allPrevLeafStates)
                        {
                            AddTransition(p, nextGroupInitialState, trigger);
                        }

                        break;
                    }
                    // NextState が LeafState のばあい
                    case LeafState nextLeafState:
                    {
                        foreach (var p in allPrevLeafStates)
                        {
                            AddTransition(p, nextLeafState, trigger);
                        }

                        break;
                    }
                }
            }
        }

        private static void AddTransition(LeafState prev, LeafState next, TTrigger trigger)
        {
            // prev state からは常に一種類のみの Transition が出ているべき
            if (prev.IsAlreadyExistTransition(trigger))
            {
                throw new ArgumentException("既に登録済みのTriggerです");
            }

            prev.AddNextNode(trigger, next);
        }

        /// <summary>
        /// State Machine の初期Stateを設定する関数
        /// State Machine の初期設定で必ず呼ぶ必要がある
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SetInitialState<T>()
            where T : Node, new()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("すでに起動中のStateMachineです");
            }

            var initialState = GetOrCreateState<T>();

            if (initialState is LeafState ls)
            {
                _nextState = ls;
            }
            else if (initialState is GroupState gs)
            {
                _nextState = gs.GetInitialState();
            }
            else
            {
                throw new InvalidOperationException("初期Stateが解決出来ませんでした");
            }
        }

        /// <summary>
        /// 既に生成済みの State ならそれを, なければ新規生成して返す関数
        /// </summary>
        /// <returns></returns>
        private T GetOrCreateState<T>()
            where T : Node, new()
        {
            // 既に生成されている State ならそれを返す
            foreach (var state in _stateList)
            {
                if (state is T t)
                {
                    return t;
                }
            }

            // State の新規作成
            var newState = new T();
            {
                newState._stateMachine = this;
            }

            // もし Group State なら
            if (newState is GroupState groupState)
            {
                // Sub-state を構築する
                groupState.SetupSubStates();
            }

            _stateList.Add(newState);
            return newState;
        }

        /// <summary>
        /// 既に生成済みの State ならそれを, なければ新規生成して返す関数
        /// </summary>
        /// <returns></returns>
        private T GetState<T>()
            where T : Node
        {
            // 既に生成されている State ならそれを返す
            foreach (var state in _stateList)
            {
                if (state is T t)
                {
                    return t;
                }
            }

            throw new InvalidOperationException("指定した State は生成されていません, 事前に AddTransition を呼んで下さい");
        }
    }
}