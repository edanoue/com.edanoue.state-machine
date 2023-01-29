// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable


namespace Edanoue.StateMachine.Tests.VendingMachine
{
    public class VendingMachine
    {
        /// <summary>
        /// ジュースの購入価格
        /// </summary>
        internal const int JUICE_PRICE = 3;

        /// <summary>
        /// State Machine への参照
        /// </summary>
        private readonly StateMachine<VendingMachine, Trigger> _stateMachine;

        public VendingMachine()
        {
            // StateMachine を初期化する
            _stateMachine = new StateMachine<VendingMachine, Trigger>(this);
            {
                // State の 遷移を登録する

                // ロック状態 => お金が足りない状態
                // お金を投入すると状態遷移
                _stateMachine.AddTransition<StateLocked, StateNotEnoughMoney>(Trigger.お金が入った);

                // お金が足りない状態 => お金足りてる状態
                // お金を投入後, 最低購入金額をクリアしていたら遷移する
                _stateMachine.AddTransition<StateNotEnoughMoney, StateEnoughMoney>(Trigger.お金が足りた);

                // お金足りてる状態 => お釣り排出状態
                // お金足りない状態 => お釣り排出状態
                // お釣り排出ボタンが押されると遷移する
                _stateMachine.AddTransition<StateNotEnoughMoney, StateProvidingCoin>(Trigger.おつり返却ボタンを押す);
                _stateMachine.AddTransition<StateEnoughMoney, StateProvidingCoin>(Trigger.おつり返却ボタンを押す);

                // お金足りてる状態 => ジュース排出状態
                // ジュース購入ボタンが押されると遷移する
                _stateMachine.AddTransition<StateEnoughMoney, StateProvidingJuice>(Trigger.ジュース購入ボタンを押す);

                // ジュース排出状態 => お金足りてる状態
                // 購入後にまだお金が足りていたら遷移する
                _stateMachine.AddTransition<StateProvidingJuice, StateEnoughMoney>(Trigger.お金が足りた);

                // ジュース排出状態 => お釣り排出状態
                // 購入後にお金足りてなかったら遷移する
                _stateMachine.AddTransition<StateProvidingJuice, StateProvidingCoin>(Trigger.お金が足りない);

                // お釣り排出状態 => ロック状態
                // お釣りの排出が終わるとロック状態に遷移する
                _stateMachine.AddTransition<StateProvidingCoin, StateLocked>(Trigger.お釣りの排出が終わった);

                // State の 遷移の登録おわり

                // 初期State を 設定する
                _stateMachine.SetInitialState<StateLocked>();

                // 起動しておく
                _stateMachine.UpdateState();
            }
        }

        /// <summary>
        /// 内部に入っているコインの枚数
        /// </summary>
        public int TotalCoinCount { get; internal set; }

        /// <summary>
        /// 排出されたジュースの数
        /// </summary>
        /// <value></value>
        public int ProvidedJuiceCount { get; internal set; }

        /// <summary>
        /// 排出されてお釣りポケットにあるコインの枚数
        /// </summary>
        /// <value></value>
        public int ProvidedCoinCount { get; internal set; }

        public bool IsCurrentState<T>()
            where T : StateMachine<VendingMachine, Trigger>.State
        {
            return _stateMachine.IsCurrentState<T>();
        }

        /// <summary>
        /// コインを自動販売機に入れる操作
        /// </summary>
        public void EnterCoin(int coinCount)
        {
            if (coinCount <= 0)
            {
                return;
            }

            TotalCoinCount += coinCount;
            _stateMachine.SendTrigger(Trigger.お金が入った);
            _stateMachine.UpdateState();
        }

        public void PushButtonJuice()
        {
            _stateMachine.SendTrigger(Trigger.ジュース購入ボタンを押す, true);
        }

        public void PushButtonChange()
        {
            _stateMachine.SendTrigger(Trigger.おつり返却ボタンを押す, true);
        }
    }
}