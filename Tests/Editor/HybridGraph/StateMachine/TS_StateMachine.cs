// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using NUnit.Framework;

namespace Edanoue.HybridGraph.Tests
{
    public class TS_StateMachine
    {
        [Test]
        public void 生成のテスト()
        {
            // StateMachine を起動する
            var blackboard = new BlackboardMock();
            var graph = EdaGraph.Run<MockStateMachine>(blackboard);

            // 起動時点で Idle に入っている
            Assert.That(blackboard.IdleEnterCount, Is.EqualTo(1));

            // Combat (GroupState) に遷移する
            Assert.That(graph.SendTrigger(Trigger.Combat), Is.True);
            graph.Update();

            // Idle.OnExit が呼ばれている
            Assert.That(blackboard.IdleEnterCount, Is.EqualTo(1));
            Assert.That(blackboard.IdleExitCount, Is.EqualTo(1));

            // Combat Group State の Blackboard の確認
            Assert.That(blackboard.CombatEnterCount, Is.EqualTo(1));
            Assert.That(blackboard.CombatExitCount, Is.EqualTo(0));
            Assert.That(blackboard.PunchEnterCount, Is.EqualTo(1));
            Assert.That(blackboard.PunchExitCount, Is.EqualTo(0));
            Assert.That(blackboard.KickEnterCount, Is.EqualTo(0));
            Assert.That(blackboard.KickExitCount, Is.EqualTo(0));

            // Kick に遷移する
            graph.SendTrigger(2);
            graph.Update();

            // Combat Group State の Blackboard の確認
            Assert.That(blackboard.CombatEnterCount, Is.EqualTo(1));
            Assert.That(blackboard.CombatExitCount, Is.EqualTo(0));
            Assert.That(blackboard.PunchEnterCount, Is.EqualTo(1));
            Assert.That(blackboard.PunchExitCount, Is.EqualTo(1));
            Assert.That(blackboard.KickEnterCount, Is.EqualTo(1));
            Assert.That(blackboard.KickExitCount, Is.EqualTo(0));

            // Punch に遷移する
            graph.SendTrigger(3);
            graph.Update();

            // Combat Group State の Blackboard の確認
            Assert.That(blackboard.CombatEnterCount, Is.EqualTo(1));
            Assert.That(blackboard.CombatExitCount, Is.EqualTo(0));
            Assert.That(blackboard.PunchEnterCount, Is.EqualTo(2));
            Assert.That(blackboard.PunchExitCount, Is.EqualTo(1));
            Assert.That(blackboard.KickEnterCount, Is.EqualTo(1));
            Assert.That(blackboard.KickExitCount, Is.EqualTo(1));

            // Idle に遷移する
            graph.SendTrigger(Trigger.ToIdle);
            graph.Update();

            // Combat Group State の Blackboard の確認
            Assert.That(blackboard.IdleEnterCount, Is.EqualTo(2));
            Assert.That(blackboard.IdleExitCount, Is.EqualTo(1));
            Assert.That(blackboard.CombatEnterCount, Is.EqualTo(1));
            Assert.That(blackboard.CombatExitCount, Is.EqualTo(1));
            Assert.That(blackboard.PunchEnterCount, Is.EqualTo(2));
            Assert.That(blackboard.PunchExitCount, Is.EqualTo(2));
            Assert.That(blackboard.KickEnterCount, Is.EqualTo(1));
            Assert.That(blackboard.KickExitCount, Is.EqualTo(1));
        }

        private interface IIdleBlackboard
        {
            public int IdleEnterCount { get; set; }
            public int IdleExitCount { get; set; }
        }

        private interface ICombatBlackboard
        {
            public int CombatEnterCount { get; set; }
            public int CombatExitCount { get; set; }
        }

        private interface IPunchBlackboard
        {
            public int PunchEnterCount { get; set; }
            public int PunchExitCount { get; set; }
        }

        private interface IKickBlackboard
        {
            public int KickEnterCount { get; set; }
            public int KickExitCount { get; set; }
        }

        private class BlackboardMock : IIdleBlackboard, ICombatBlackboard, IPunchBlackboard, IKickBlackboard
        {
            public int CombatEnterCount { get; set; }
            public int CombatExitCount { get; set; }
            public int IdleEnterCount { get; set; }
            public int IdleExitCount { get; set; }
            public int KickEnterCount { get; set; }
            public int KickExitCount { get; set; }
            public int PunchEnterCount { get; set; }
            public int PunchExitCount { get; set; }
        }

        private class MockStateMachine : StateMachine<BlackboardMock>
        {
            protected override void OnSetupStates(IStateBuilder builder)
            {
                builder.SetInitialState<StateIdle>();
                builder.AddTransition<StateIdle, GroupStateCombat>(Trigger.Combat);
                builder.AddTransition<GroupStateCombat, StateIdle>(Trigger.ToIdle);
            }
        }

        public readonly struct Trigger
        {
            private readonly       int     _value;
            public static readonly Trigger ToIdle = new(0);
            public static readonly Trigger Combat = new(1);

            private Trigger(int value)
            {
                _value = value;
            }

            public static implicit operator int(Trigger value)
            {
                return value._value;
            }
        }

        private class StateIdle : LeafState<IIdleBlackboard>
        {
            protected override void OnEnter()
            {
                Blackboard.IdleEnterCount++;
            }

            protected override void OnExit()
            {
                Blackboard.IdleExitCount++;
            }
        }

        private class GroupStateCombat : StateMachine<ICombatBlackboard>
        {
            protected override void OnSetupStates(IStateBuilder builder)
            {
                builder.AddTransition<StatePunch, StateKick>(2); // int を直接使っても良い
                builder.SetInitialState<StatePunch>();
                builder.AddTransition<StateKick, StatePunch>(3);
            }

            protected override void OnEnter()
            {
                Blackboard.CombatEnterCount++;
            }

            protected override void OnExit()
            {
                Blackboard.CombatExitCount++;
            }
        }

        private class StatePunch : LeafState<IPunchBlackboard>
        {
            protected override void OnEnter()
            {
                Blackboard.PunchEnterCount++;
            }

            protected override void OnExit()
            {
                Blackboard.PunchExitCount++;
            }
        }

        private class StateKick : LeafState<IKickBlackboard>
        {
            protected override void OnEnter()
            {
                Blackboard.KickEnterCount++;
            }

            protected override void OnExit()
            {
                Blackboard.KickExitCount++;
            }
        }
    }
}