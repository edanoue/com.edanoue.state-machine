// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Edanoue.HybridGraph.BehaviourTree.Decorators
{
    public class TS_OnExit
    {
        [Test]
        public async Task OnExitが機能する()
        {
            var blackboard = new MockBlackboard();
            var graph = EdaGraph.Run<MockBtA>(blackboard);

            Assert.That(blackboard.SelectorOnExitCount, Is.EqualTo(0));
            Assert.That(blackboard.ActionOneOnExitCount, Is.EqualTo(1));
            Assert.That(blackboard.ActionTwoOnExitCount, Is.EqualTo(0));

            await UniTask.Delay(TimeSpan.FromMilliseconds(100));

            Assert.That(blackboard.SelectorOnExitCount, Is.EqualTo(0));
            Assert.That(blackboard.ActionOneOnExitCount, Is.EqualTo(2));
            Assert.That(blackboard.ActionTwoOnExitCount, Is.EqualTo(0));

            await UniTask.Delay(TimeSpan.FromMilliseconds(100));

            Assert.That(blackboard.SelectorOnExitCount, Is.EqualTo(0));
            Assert.That(blackboard.ActionOneOnExitCount, Is.EqualTo(2));
            Assert.That(blackboard.ActionTwoOnExitCount, Is.EqualTo(1));

            await UniTask.Delay(TimeSpan.FromMilliseconds(100));

            Assert.That(blackboard.SelectorOnExitCount, Is.EqualTo(1));
            Assert.That(blackboard.ActionOneOnExitCount, Is.EqualTo(2));
            Assert.That(blackboard.ActionTwoOnExitCount, Is.EqualTo(2));
        }

        private class MockBlackboard
        {
            public int ActionOneOnExitCount;
            public int ActionTwoOnExitCount;
            public int Counter;
            public int SelectorOnExitCount;
        }

        private class MockBtA : BehaviourTree<MockBlackboard>
        {
            protected override void OnSetupBehaviours(IRootNode root)
            {
                static bool CountUpAction(MockBlackboard bb)
                {
                    bb.Counter++;
                    return true;
                }

                var selector = root.Add.Selector();
                selector
                    .With.Loop(4)
                    .With.OnExit<MockBlackboard>(bb => bb.SelectorOnExitCount++);
                {
                    var a1 = selector.Add.Action<MockBlackboard>(CountUpAction);
                    {
                        a1
                            .With.If<MockBlackboard>(bb => bb.Counter < 2)
                            .With.OnExit<MockBlackboard>(bb => bb.ActionOneOnExitCount++);
                    }
                    var a2 = selector.Add.Action<MockBlackboard>(CountUpAction);
                    {
                        a2.With.OnExit<MockBlackboard>(bb => bb.ActionTwoOnExitCount++);
                    }
                }
            }
        }
    }
}