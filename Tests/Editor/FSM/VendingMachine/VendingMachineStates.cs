// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

namespace Edanoue.StateMachine.Tests.VendingMachine
{
    using BaseState = StateMachine<VendingMachine, Trigger>.State;

    /// <summary>
    /// ロック状態(アイドル状態)
    /// </summary>
    public sealed class StateLocked : BaseState
    {
        // ユーザーによる操作待機なのでここでは特に何もすることはない
        // ピカピカ光らせたりするなら実装をかく
    }

    /// <summary>
    /// ジュースが買えるだけお金が入っている状態
    /// </summary>
    public sealed class StateEnoughMoney : BaseState
    {
        // ユーザーによる操作待機なのでここでは特に何もすることはない
        // ピカピカ光らせたりするなら実装をかく
    }

    /// <summary>
    /// お金が入っているがジュースを買うには足りない状態
    /// </summary>
    public sealed class StateNotEnoughMoney : BaseState
    {
        protected override void OnEnterState(IRunningStateMachine<Trigger> stateMachine)
        {
            // もしお金が足りていたら, お金足りてる状態に移動する
            if (Context.TotalCoinCount >= VendingMachine.JUICE_PRICE)
            {
                stateMachine.SendTrigger(Trigger.お金が足りた);
            }
        }

        protected override void OnStayState(IRunningStateMachine<Trigger> stateMachine)
        {
            // もしお金が足りていたら, お金足りてる状態に移動する
            if (Context.TotalCoinCount >= VendingMachine.JUICE_PRICE)
            {
                stateMachine.SendTrigger(Trigger.お金が足りた);
            }
        }
    }

    /// <summary>
    /// ジュース排出中の状態
    /// </summary>
    public sealed class StateProvidingJuice : BaseState
    {
        protected override void OnEnterState(IRunningStateMachine<Trigger> stateMachine)
        {
            // ジュースを一つ排出する
            Context.ProvidedJuiceCount += 1;

            // ジュースの値段分お金を減らす
            Context.TotalCoinCount -= VendingMachine.JUICE_PRICE;

            // もしまだジュースを買えるだけお金が残っているならば
            if (Context.TotalCoinCount >= VendingMachine.JUICE_PRICE)
            {
                // お金足りてるTriggerを送る
                stateMachine.SendTrigger(Trigger.お金が足りた);
            }
            // もうジュースを買えるだけのお金が残っていない
            else
            {
                // お金足りないTriggerを送る
                stateMachine.SendTrigger(Trigger.お金が足りない);
            }
        }
    }

    /// <summary>
    /// お釣り排出中の状態
    /// </summary>
    public sealed class StateProvidingCoin : BaseState
    {
        protected override void OnEnterState(IRunningStateMachine<Trigger> stateMachine)
        {
            // 今あるお釣りをすべて排出する
            Context.ProvidedCoinCount += Context.TotalCoinCount;
            Context.TotalCoinCount = 0;

            // お釣りの排出完了
            stateMachine.SendTrigger(Trigger.お釣りの排出が終わった);
        }
    }
}