// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using NUnit.Framework;

namespace Edanoue.HybridGraph.Tests
{
    public class TS_HybridGraph2
    {
        [Test]
        public void BehaviourTreeInStateMachine_初期StateがBT()
        {
            // StateMachine を起動する
            var blackboard = new BlackboardMock();
            var graph = EdaGraph.Run<MockStateMachine>(blackboard);

            Assert.That(blackboard.CombatEnterCount, Is.EqualTo(1));
            Assert.That(blackboard.CombatExitCount, Is.EqualTo(0));
            Assert.That(blackboard.PunchCounter, Is.EqualTo(0));
            Assert.That(blackboard.KickCounter, Is.EqualTo(3));

            graph.SendTrigger(Trigger.ToPunch);
            graph.Update();

            Assert.That(blackboard.CombatEnterCount, Is.EqualTo(1));
            Assert.That(blackboard.CombatExitCount, Is.EqualTo(0));
            Assert.That(blackboard.PunchCounter, Is.EqualTo(1));
            Assert.That(blackboard.KickCounter, Is.EqualTo(3));
        }

        private class BlackboardMock
        {
            public int CombatEnterCount { get; set; }
            public int CombatExitCount { get; set; }
            public int PunchCounter { get; set; }
            public int KickCounter { get; set; }
        }

        private class MockStateMachine : StateMachine<BlackboardMock>
        {
            protected override void OnSetupStates(IStateBuilder builder)
            {
                builder.SetInitialState<GroupStateCombat>();
            }
        }

        public readonly struct Trigger
        {
            private readonly       int     _value;
            public static readonly Trigger ToKick  = new(2);
            public static readonly Trigger ToPunch = new(3);

            private Trigger(int value)
            {
                _value = value;
            }

            public static implicit operator int(Trigger value)
            {
                return value._value;
            }
        }

        private class GroupStateCombat : StateMachine<BlackboardMock>
        {
            protected override void OnSetupStates(IStateBuilder builder)
            {
                builder.SetInitialState<BehaviourTreeKick>();
                builder.AddTransition<StatePunch, BehaviourTreeKick>(Trigger.ToKick); // int を直接使っても良い
                builder.AddTransition<BehaviourTreeKick, StatePunch>(Trigger.ToPunch);
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

        private class StatePunch : LeafState<BlackboardMock>
        {
            protected override void OnEnter()
            {
                Blackboard.PunchCounter++;
            }
        }

        private class BehaviourTreeKick : BehaviourTree<BlackboardMock>
        {
            protected override void OnSetupBehaviours(IRootNode root)
            {
                // 3連キック!
                var seq = root.Add.Sequence();
                seq.Add.Action<BlackboardMock>(bb =>
                {
                    bb.KickCounter++;
                    return true;
                });
                seq.Add.Action<BlackboardMock>(bb =>
                {
                    bb.KickCounter++;
                    return true;
                });
                seq.Add.Action<BlackboardMock>(bb =>
                {
                    bb.KickCounter++;
                    return true;
                });
            }
        }
    }
}