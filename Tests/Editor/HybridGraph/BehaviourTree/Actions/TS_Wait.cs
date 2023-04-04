// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Edanoue.HybridGraph.BehaviourTree.Actions
{
    public class TS_Wait
    {
        [Test]
        public async Task Waitの挙動確認A()
        {
            var blackboard = new MockBlackboard();
            using var graph = EdaGraph.Run<MockBtA>(blackboard);

            Assert.That(blackboard.ActionCalledCount, Is.EqualTo(0));
            Assert.That(blackboard.WaitOnEnterCount, Is.EqualTo(1));
            Assert.That(blackboard.WaitOnExitCount, Is.EqualTo(0));

            await UniTask.Delay(TimeSpan.FromMilliseconds(500));
            Assert.That(blackboard.ActionCalledCount, Is.EqualTo(1));
            Assert.That(blackboard.WaitOnEnterCount, Is.EqualTo(2));
            Assert.That(blackboard.WaitOnExitCount, Is.EqualTo(1));

            await UniTask.Delay(TimeSpan.FromMilliseconds(500));
            Assert.That(blackboard.ActionCalledCount, Is.EqualTo(2));
            Assert.That(blackboard.WaitOnEnterCount, Is.EqualTo(3));
            Assert.That(blackboard.WaitOnExitCount, Is.EqualTo(2));

            await UniTask.Delay(TimeSpan.FromMilliseconds(500));
            Assert.That(blackboard.ActionCalledCount, Is.EqualTo(3));
            Assert.That(blackboard.WaitOnEnterCount, Is.EqualTo(4));
            Assert.That(blackboard.WaitOnExitCount, Is.EqualTo(3));
        }

        private class MockBlackboard
        {
            public int ActionCalledCount;
            public int WaitOnEnterCount;
            public int WaitOnExitCount;
        }

        private class MockBtA : BehaviourTree<MockBlackboard>
        {
            // Blackboard のカウンターを一つ進める Action
            private static bool CountUpAction(MockBlackboard bb)
            {
                bb.ActionCalledCount++;
                return true;
            }

            protected override void OnSetupBehaviours(IRootNode root)
            {
                var seqA = root.Add.Sequence();
                var seqB = seqA.Add.Sequence();
                seqB.With.While<MockBlackboard>(_ => true);
                {
                    seqB.Add.Wait(TimeSpan.FromMilliseconds(500))
                        .With.OnEnter<MockBlackboard>(bb => bb.WaitOnEnterCount++)
                        .With.OnExit<MockBlackboard>(bb => bb.WaitOnExitCount++);
                    seqB.Add.Action<MockBlackboard>(CountUpAction);
                }
            }
        }
    }
}