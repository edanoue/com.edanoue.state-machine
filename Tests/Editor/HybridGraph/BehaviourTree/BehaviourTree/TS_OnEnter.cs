// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace Edanoue.HybridGraph.BehaviourTree.BehaviourTree
{
    public class TS_OnEnter
    {
        [Test]
        public void 直接Runした時の挙動()
        {
            var bb = new MockBlackboard();
            EdaGraph.Run<MockBehaviourTreeA>(bb);
            Assert.That(bb.OnEnterCounter, Is.EqualTo(1));
        }

        [Test]
        public void BtInSmの時の挙動_InitialState()
        {
            var bb = new MockBlackboard();
            EdaGraph.Run<MockStateMachineA>(bb);
            Assert.That(bb.OnEnterCounter, Is.EqualTo(1));
        }

        [Test]
        public void BtInSmの時の挙動_遷移()
        {
            var bb = new MockBlackboard();
            var graph = EdaGraph.Run<MockStateMachineB>(bb);
            Assert.That(bb.OnEnterCounter, Is.EqualTo(0));

            graph.SendTrigger(0);
            graph.Update();
            Assert.That(bb.OnEnterCounter, Is.EqualTo(1));

            graph.SendTrigger(1);
            graph.Update();
            Assert.That(bb.OnEnterCounter, Is.EqualTo(1));

            graph.SendTrigger(0);
            graph.Update();
            Assert.That(bb.OnEnterCounter, Is.EqualTo(2));
        }

        [Test]
        public async Task BtInBtの時の挙動()
        {
            var bb = new MockBlackboard();
            EdaGraph.Run<MockBehaviourTreeB>(bb);
            Assert.That(bb.OnEnterCounter, Is.EqualTo(1));
            await UniTask.Delay(TimeSpan.FromMilliseconds(101));
            Assert.That(bb.OnEnterCounter, Is.EqualTo(2));
            await UniTask.Delay(TimeSpan.FromMilliseconds(101));
            Assert.That(bb.OnEnterCounter, Is.EqualTo(2));
        }

        [Test]
        public void BtInBtの時のDecoratorのOnEnterと併用したときの挙動()
        {
            var bb = new MockBlackboard();
            EdaGraph.Run<MockBehaviourTreeC>(bb);
            Assert.That(bb.OnEnterCounter, Is.EqualTo(2));
        }

        private static bool NoOpAction(MockBlackboard bb)
        {
            return true;
        }

        private class MockBlackboard
        {
            public int OnEnterCounter;
        }

        private sealed class MockBehaviourTreeA : BehaviourTree<MockBlackboard>
        {
            protected override void OnSetupBehaviours(IRootNode root)
            {
                var seq = root.Add.Sequence();
                {
                    seq.Add.Action<MockBlackboard>(NoOpAction);
                    seq.Add.Action<MockBlackboard>(NoOpAction);
                }
            }

            protected override void OnEnter()
            {
                Blackboard.OnEnterCounter++;
            }
        }

        private sealed class MockBehaviourTreeB : BehaviourTree<MockBlackboard>
        {
            protected override void OnSetupBehaviours(IRootNode root)
            {
                var seq = root.Add.Sequence();
                {
                    seq.Add.BehaviourTree<SubBehaviourTree>().With.Loop(2);
                }
            }

            private class SubBehaviourTree : BehaviourTree<MockBlackboard>
            {
                protected override void OnSetupBehaviours(IRootNode root)
                {
                    var seq = root.Add.Sequence();
                    seq.Add.Action<MockBlackboard>(NoOpAction);
                }

                protected override void OnEnter()
                {
                    Blackboard.OnEnterCounter++;
                }
            }
        }

        private sealed class MockBehaviourTreeC : BehaviourTree<MockBlackboard>
        {
            protected override void OnSetupBehaviours(IRootNode root)
            {
                var seq = root.Add.Sequence();
                {
                    // OnEnter Decorator をつける
                    seq.Add.BehaviourTree<SubBehaviourTree>()
                        .With.OnEnter<MockBlackboard>(bb => bb.OnEnterCounter++);
                }
            }

            private class SubBehaviourTree : BehaviourTree<MockBlackboard>
            {
                protected override void OnSetupBehaviours(IRootNode root)
                {
                    var seq = root.Add.Sequence();
                    seq.Add.Action<MockBlackboard>(NoOpAction);
                }

                protected override void OnEnter()
                {
                    // 先に Decorator 側の OnEnter が呼ばれていることを確認する
                    if (Blackboard.OnEnterCounter == 1)
                    {
                        Blackboard.OnEnterCounter++;
                    }
                }
            }
        }

        private sealed class MockStateMachineA : StateMachine<MockBlackboard>
        {
            protected override void OnSetupStates(IStateBuilder builder)
            {
                builder.SetInitialState<MockBehaviourTreeA>();
            }
        }

        private sealed class MockStateMachineB : StateMachine<MockBlackboard>
        {
            protected override void OnSetupStates(IStateBuilder builder)
            {
                builder.AddTransition<IdleState, MockStateMachineA>(0);
                builder.AddTransition<MockStateMachineA, IdleState>(1);
                builder.SetInitialState<IdleState>();
            }

            private sealed class IdleState : LeafState<MockBlackboard>
            {
            }
        }
    }
}