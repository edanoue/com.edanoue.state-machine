// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

namespace Edanoue.StateMachine.Tests.VendingMachine
{
    /// <summary>
    /// ロック状態(アイドル状態)
    /// </summary>
    public sealed class StateLocked : StateMachine<VendingMachine, Trigger>.State
    {
        // ユーザーによる操作待機なのでここでは特に何もすることはない
        // ピカピカ光らせたりするなら実装をかく
    }

    /// <summary>
    /// ジュースが買えるだけお金が入っている状態
    /// </summary>
    public sealed class StateEnoughMoney : StateMachine<VendingMachine, Trigger>.State
    {
        // ユーザーによる操作待機なのでここでは特に何もすることはない
        // ピカピカ光らせたりするなら実装をかく
    }

    /// <summary>
    /// お金が入っているがジュースを買うには足りない状態
    /// </summary>
    public sealed class StateNotEnoughMoney : StateMachine<VendingMachine, Trigger>.State
    {
        protected override void Enter()
        {
            // もしお金が足りていたら, お金足りてる状態に移動する
            if (Context.TotalCoinCount >= Context.m_juicePrice)
            {
                StateMachine.SendTrigger(Trigger.お金が足りた);
                StateMachine.UpdateState();
            }
        }

        protected override void Update(float deltaTime)
        {
            // もしお金が足りていたら, お金足りてる状態に移動する
            if (Context.TotalCoinCount >= Context.m_juicePrice)
            {
                StateMachine.SendTrigger(Trigger.お金が足りた);
                StateMachine.UpdateState();
            }
        }
    }

    /// <summary>
    /// ジュース排出中の状態
    /// </summary>
    public sealed class StateProvidingJuice : StateMachine<VendingMachine, Trigger>.State
    {
        protected override void Enter()
        {
            // ジュースを一つ排出する
            Context.ProvidedJuiceCount += 1;

            // ジュースの値段分お金を減らす
            Context.TotalCoinCount -= Context.m_juicePrice;

            // もしまだジュースを買えるだけお金が残っているならば
            if (Context.TotalCoinCount >= Context.m_juicePrice)
            {
                // お金足りてるTriggerを送る
                StateMachine.SendTrigger(Trigger.お金が足りた);
                StateMachine.UpdateState();
            }
            // もうジュースを買えるだけのお金が残っていない
            else
            {
                // お金足りないTriggerを送る
                StateMachine.SendTrigger(Trigger.お金が足りない);
                StateMachine.UpdateState();
            }
        }
    }

    /// <summary>
    /// お釣り排出中の状態
    /// </summary>
    public sealed class StateProvidingCoin : StateMachine<VendingMachine, Trigger>.State
    {
        protected override void Enter()
        {
            // 今あるお釣りをすべて排出する
            Context.ProvidedCoinCount += Context.TotalCoinCount;
            Context.TotalCoinCount = 0;

            // お釣りの排出完了
            StateMachine.SendTrigger(Trigger.お釣りの排出が終わった);
            StateMachine.UpdateState();
        }
    }
}