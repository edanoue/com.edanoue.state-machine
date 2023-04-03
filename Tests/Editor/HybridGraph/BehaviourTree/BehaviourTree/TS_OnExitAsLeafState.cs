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
    public class TS_OnExitAsLeafState
    {
        [Test]
        public void 直接Runした時の挙動()
        {
            var bb = new MockBlackboard();
            EdaGraph.Run<MockBehaviourTreeA>(bb);
            // 直接実行した場合は, OnExit が呼ばれることはない
            Assert.That(bb.OnExitCounter, Is.EqualTo(0));
        }

        [Test]
        public void BtInSmの時の挙動_InitialState()
        {
            var bb = new MockBlackboard();
            EdaGraph.Run<MockStateMachineA>(bb);
            // InitialState で遷移が発生しない場合は OnExit が呼ばれることはない
            Assert.That(bb.OnExitCounter, Is.EqualTo(0));
        }

        [Test]
        public void BtInSmの時の挙動_遷移()
        {
            var bb = new MockBlackboard();
            var graph = EdaGraph.Run<MockStateMachineB>(bb);
            // 異なる State への遷移, この時点では呼ばれない
            Assert.That(bb.OnExitCounter, Is.EqualTo(0));

            // 遷移による State への Enter, この時点では呼ばれない
            graph.SendTrigger(0);
            graph.Update();
            Assert.That(bb.OnExitCounter, Is.EqualTo(0));

            // 遷移による State からの Exit, この時点では呼ばれる
            graph.SendTrigger(1);
            graph.Update();
            Assert.That(bb.OnExitCounter, Is.EqualTo(1));

            // もう一度呼ばれる
            graph.SendTrigger(0);
            graph.Update();
            graph.SendTrigger(1);
            graph.Update();
            Assert.That(bb.OnExitCounter, Is.EqualTo(2));
        }

        [Test]
        public async Task BtInBtの時の挙動()
        {
            // BehaviourTree in BehaviourTree として設定された時の動作
            // この場合, コンテキストは BT のため実行されることはない
            var bb = new MockBlackboard();
            EdaGraph.Run<MockBehaviourTreeB>(bb);
            Assert.That(bb.OnExitCounter, Is.EqualTo(0));
            await UniTask.Delay(TimeSpan.FromMilliseconds(101));
            Assert.That(bb.OnExitCounter, Is.EqualTo(0));
            await UniTask.Delay(TimeSpan.FromMilliseconds(101));
            Assert.That(bb.OnExitCounter, Is.EqualTo(0));
        }

        private static bool NoOpAction()
        {
            return true;
        }

        private class MockBlackboard
        {
            public int OnExitCounter;
        }

        private sealed class MockBehaviourTreeA : BehaviourTree<MockBlackboard>
        {
            protected override void OnSetupBehaviours(IRootNode root)
            {
                var seq = root.Add.Sequence();
                {
                    seq.Add.Action(NoOpAction);
                    seq.Add.Action(NoOpAction);
                }
            }

            protected override void OnExitAsLeafState()
            {
                Blackboard.OnExitCounter++;
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

            private sealed class SubBehaviourTree : BehaviourTree<MockBlackboard>
            {
                protected override void OnSetupBehaviours(IRootNode root)
                {
                    root.Add.Sequence().Add.Action(NoOpAction);
                }

                protected override void OnExitAsLeafState()
                {
                    Blackboard.OnExitCounter++;
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