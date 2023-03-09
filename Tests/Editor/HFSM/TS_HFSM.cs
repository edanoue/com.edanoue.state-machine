// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using NUnit.Framework;

namespace Edanoue.StateMachine.Tests
{
    public class TS_HFSM
    {
        private int _停止中GroupのOnEnterが呼ばれた回数;
        private int _停止中GroupのOnExitが呼ばれた回数;
        private int _監視状態GroupのOnEnterが呼ばれた回数;
        private int _監視状態GroupのOnExitが呼ばれた回数;

        [Test]
        public void StateMachine生成のテスト()
        {
            _監視状態GroupのOnEnterが呼ばれた回数 = 0;
            _監視状態GroupのOnExitが呼ばれた回数 = 0;
            _停止中GroupのOnEnterが呼ばれた回数 = 0;
            _停止中GroupのOnExitが呼ばれた回数 = 0;

            // StateMachine を生成する
            var sm = new StateMachine(this);
            Assert.That(sm, Is.Not.Null);

            // StateMachine を開始する
            sm.UpdateState();

            // 現在の State を確認する
            Assert.That(sm.IsCurrentState<StateMachine.監視状態.巡回中>(), Is.True);

            // OnEnter の確認
            Assert.That(_監視状態GroupのOnEnterが呼ばれた回数, Is.EqualTo(1));
            Assert.That(_監視状態GroupのOnExitが呼ばれた回数, Is.EqualTo(0));

            // 遷移の確認
            sm.SendTrigger(StateMachine.Trigger.かなり歩いてつかれた);
            sm.UpdateState();
            Assert.That(sm.IsCurrentState<StateMachine.監視状態.停止中.右を見ている>(), Is.True);

            // OnEnter の確認
            Assert.That(_監視状態GroupのOnEnterが呼ばれた回数, Is.EqualTo(1));
            Assert.That(_監視状態GroupのOnExitが呼ばれた回数, Is.EqualTo(0));
            Assert.That(_停止中GroupのOnEnterが呼ばれた回数, Is.EqualTo(1));
            Assert.That(_停止中GroupのOnExitが呼ばれた回数, Is.EqualTo(0));

            // 遷移の確認
            sm.SendTrigger(StateMachine.Trigger.つかれがとれた);
            sm.UpdateState();
            Assert.That(sm.IsCurrentState<StateMachine.監視状態.巡回中>(), Is.True);

            // OnEnter の確認
            Assert.That(_監視状態GroupのOnEnterが呼ばれた回数, Is.EqualTo(1));
            Assert.That(_監視状態GroupのOnExitが呼ばれた回数, Is.EqualTo(0));
            Assert.That(_停止中GroupのOnEnterが呼ばれた回数, Is.EqualTo(1));
            Assert.That(_停止中GroupのOnExitが呼ばれた回数, Is.EqualTo(1));

            // 遷移の確認 (監視状態のどこにいても戦闘状態に)
            sm.SendTrigger(StateMachine.Trigger.敵を発見した);
            sm.UpdateState();
            Assert.That(sm.IsCurrentState<StateMachine.戦闘状態.距離を取る>(), Is.True);

            // OnEnter の確認
            Assert.That(_監視状態GroupのOnEnterが呼ばれた回数, Is.EqualTo(1));
            Assert.That(_監視状態GroupのOnExitが呼ばれた回数, Is.EqualTo(1));
            Assert.That(_停止中GroupのOnEnterが呼ばれた回数, Is.EqualTo(1));
            Assert.That(_停止中GroupのOnExitが呼ばれた回数, Is.EqualTo(1));

            // 遷移の確認
            sm.SendTrigger(StateMachine.Trigger.敵を見失った);
            // 無効な Trigger を送付するが無視される
            sm.SendTrigger(StateMachine.Trigger.右を見終わった);
            sm.UpdateState();
            Assert.That(sm.IsCurrentState<StateMachine.監視状態.巡回中>(), Is.True);

            // OnEnter の確認
            Assert.That(_監視状態GroupのOnEnterが呼ばれた回数, Is.EqualTo(2));
            Assert.That(_監視状態GroupのOnExitが呼ばれた回数, Is.EqualTo(1));
        }

        private class StateMachine : HierarchicalStateMachine<TS_HFSM, StateMachine.Trigger>
        {
            public enum Trigger
            {
                敵を発見した,
                敵を見失った,
                かなり歩いてつかれた,
                つかれがとれた,
                右を見終わった,
                左を見終わった,
                プレイヤーにある程度近づいた,
                プレイヤーを殴った
            }

            public StateMachine(TS_HFSM context) : base(context)
            {
            }

            protected override void SetupStates(IStateBuilder<TS_HFSM, Trigger> builder)
            {
                // 監視状態 / 戦闘状態の 2つの State を入れる
                builder.AddTransition<監視状態, 戦闘状態>(Trigger.敵を発見した);
                builder.AddTransition<戦闘状態, 監視状態>(Trigger.敵を見失った);

                // 初期状態は監視状態
                builder.SetInitialState<監視状態>();
            }

            public class 監視状態 : GroupState
            {
                protected override void SetupSubStates(IStateBuilder<TS_HFSM, Trigger> group)
                {
                    group.AddTransition<巡回中, 停止中>(Trigger.かなり歩いてつかれた);
                    group.AddTransition<停止中, 巡回中>(Trigger.つかれがとれた);
                    group.SetInitialState<巡回中>();
                }

                protected override void OnEnterState(IRunningStateMachine<Trigger> stateMachine)
                {
                    Context._監視状態GroupのOnEnterが呼ばれた回数++;
                }

                protected override void OnExitState()
                {
                    Context._監視状態GroupのOnExitが呼ばれた回数++;
                }

                internal class 巡回中 : LeafState
                {
                }

                internal class 停止中 : GroupState
                {
                    protected override void SetupSubStates(IStateBuilder<TS_HFSM, Trigger> group)
                    {
                        group.AddTransition<右を見ている, 左を見ている>(Trigger.右を見終わった);
                        group.AddTransition<左を見ている, 右を見ている>(Trigger.左を見終わった);
                        group.SetInitialState<右を見ている>();
                    }

                    protected override void OnEnterState(IRunningStateMachine<Trigger> stateMachine)
                    {
                        Context._停止中GroupのOnEnterが呼ばれた回数++;
                    }

                    protected override void OnExitState()
                    {
                        Context._停止中GroupのOnExitが呼ばれた回数++;
                    }

                    internal class 右を見ている : LeafState
                    {
                    }

                    internal class 左を見ている : LeafState
                    {
                    }
                }
            }

            public class 戦闘状態 : GroupState
            {
                protected override void SetupSubStates(IStateBuilder<TS_HFSM, Trigger> group)
                {
                    group.AddTransition<距離を取る, 殴る>(Trigger.プレイヤーにある程度近づいた);
                    group.AddTransition<殴る, 距離を取る>(Trigger.プレイヤーを殴った);
                    group.SetInitialState<距離を取る>();
                }

                internal class 殴る : LeafState
                {
                }

                internal class 距離を取る : LeafState
                {
                }
            }
        }
    }
}