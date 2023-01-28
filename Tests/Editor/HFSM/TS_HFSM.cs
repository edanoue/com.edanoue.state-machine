// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using NUnit.Framework;

namespace Edanoue.StateMachine.Tests
{
    using StateMachine = HierarchicalStateMachine<TS_HFSM, TS_HFSM.Trigger>;

    public class TS_HFSM
    {
        [Test]
        public void StateMachine生成のテスト()
        {
            // StateMachine を生成する
            var sm = new StateMachine(this);
            Assert.That(sm, Is.Not.Null);

            // 監視状態 / 戦闘状態の 2つの State を入れる
            sm.AddTransition<監視状態, 戦闘状態>(Trigger.敵を発見した);
            sm.AddTransition<戦闘状態, 監視状態>(Trigger.敵を見失った);

            // 初期状態は監視状態
            sm.SetInitialState<監視状態>();

            // StateMachine を開始する
            sm.UpdateState();

            // 現在の State を確認する
            Assert.That(sm.IsCurrentState<監視状態.巡回中>(), Is.True);

            // 遷移の確認
            sm.SendTriggerAndUpdateState(Trigger.かなり歩いてつかれた);
            Assert.That(sm.IsCurrentState<監視状態.停止中.右を見ている>(), Is.True);

            // 遷移の確認 (監視状態のどこにいても戦闘状態に)
            sm.SendTriggerAndUpdateState(Trigger.敵を発見した);
            Assert.That(sm.IsCurrentState<戦闘状態>(), Is.True);

            // 遷移の確認
            sm.SendTriggerAndUpdateState(Trigger.敵を見失った);
            Assert.That(sm.IsCurrentState<監視状態.巡回中>(), Is.True);
        }


        internal enum Trigger
        {
            敵を発見した,
            敵を見失った,
            かなり歩いてつかれた,
            つかれがとれた,
            右を見終わった,
            左を見終わった
        }

        private class 監視状態 : StateMachine.GroupState
        {
            protected override void SetupSubStates(ISubStateSetup<TS_HFSM, Trigger> group)
            {
                group.AddTransition<巡回中, 停止中>(Trigger.かなり歩いてつかれた);
                group.AddTransition<停止中, 巡回中>(Trigger.つかれがとれた);
                group.SetInitialState<巡回中>();
            }

            internal class 巡回中 : StateMachine.LeafState
            {
            }

            internal class 停止中 : StateMachine.GroupState
            {
                protected override void SetupSubStates(ISubStateSetup<TS_HFSM, Trigger> group)
                {
                    group.AddTransition<右を見ている, 左を見ている>(Trigger.右を見終わった);
                    group.AddTransition<左を見ている, 右を見ている>(Trigger.左を見終わった);
                    group.SetInitialState<右を見ている>();
                }

                internal class 右を見ている : StateMachine.LeafState
                {
                }

                internal class 左を見ている : StateMachine.LeafState
                {
                }
            }
        }

        private class 戦闘状態 : StateMachine.LeafState
        {
            protected override void Enter()
            {
                base.Enter();
            }
        }
    }
}