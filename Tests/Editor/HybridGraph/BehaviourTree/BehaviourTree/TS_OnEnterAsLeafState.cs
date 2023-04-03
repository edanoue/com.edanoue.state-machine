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
    public class TS_OnEnterAsLeafState
    {
        [Test]
        public async Task 直接Runした時の挙動()
        {
            // Graph として (直接 Run) 動作した時の挙動
            // OnEnterAsLeafState が一度だけ呼ばれる (Behaviour の動作は一切影響を及ぼさない)
            var bb = new MockBlackboard();
            EdaGraph.Run<MockBehaviourTreeA>(bb);
            Assert.That(bb.OnEnterCounter, Is.EqualTo(1));
            await UniTask.Delay(TimeSpan.FromMilliseconds(101));
            Assert.That(bb.OnEnterCounter, Is.EqualTo(1));
        }

        [Test]
        public void BtInSmの時の挙動_InitialState()
        {
            // LeafState として SM に Initial State として設定されたときの動作
            // OnEnterAsLeafState が一度だけ呼ばれる
            var bb = new MockBlackboard();
            EdaGraph.Run<MockStateMachineA>(bb);
            Assert.That(bb.OnEnterCounter, Is.EqualTo(1));
        }

        [Test]
        public void BtInSmの時の挙動_遷移()
        {
            // LeafState として SM に 設定されたときの動作
            // 遷移時に OnEnterAsLeafState が呼ばれる
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
            // BehaviourTree in BehaviourTree として設定された時の動作
            // この場合, コンテキストは BT のため実行されることはない
            var bb = new MockBlackboard();
            EdaGraph.Run<MockBehaviourTreeB>(bb);
            Assert.That(bb.OnEnterCounter, Is.EqualTo(0));
            await UniTask.Delay(TimeSpan.FromMilliseconds(101));
            Assert.That(bb.OnEnterCounter, Is.EqualTo(0));
            await UniTask.Delay(TimeSpan.FromMilliseconds(101));
            Assert.That(bb.OnEnterCounter, Is.EqualTo(0));
        }

        private static bool NoOpAction()
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
                    seq.Add.Action(NoOpAction).With.Loop(2);
                }
            }

            protected override void OnEnterAsLeafState()
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
                    root.Add.Sequence().Add.Action(NoOpAction);
                }

                protected override void OnEnterAsLeafState()
                {
                    Blackboard.OnEnterCounter++;
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