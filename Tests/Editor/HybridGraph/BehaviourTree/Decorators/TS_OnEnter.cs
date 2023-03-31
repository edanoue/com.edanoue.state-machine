// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Edanoue.HybridGraph.BehaviourTree.Decorators
{
    public class TS_OnEnter
    {
        [Test]
        public async Task OnEnterが機能する()
        {
            var blackboard = new MockBlackboard();
            var graph = EdaGraph.Run<MockBtA>(blackboard);

            Assert.That(blackboard.SelectorOnEnterCount, Is.EqualTo(1));
            Assert.That(blackboard.ActionOneOnEnterCount, Is.EqualTo(1));
            Assert.That(blackboard.ActionTwoOnEnterCount, Is.EqualTo(0));

            await UniTask.Delay(TimeSpan.FromMilliseconds(100));

            Assert.That(blackboard.SelectorOnEnterCount, Is.EqualTo(1));
            Assert.That(blackboard.ActionOneOnEnterCount, Is.EqualTo(2));
            Assert.That(blackboard.ActionTwoOnEnterCount, Is.EqualTo(0));

            await UniTask.Delay(TimeSpan.FromMilliseconds(100));

            Assert.That(blackboard.SelectorOnEnterCount, Is.EqualTo(1));
            Assert.That(blackboard.ActionOneOnEnterCount, Is.EqualTo(2));
            Assert.That(blackboard.ActionTwoOnEnterCount, Is.EqualTo(1));

            await UniTask.Delay(TimeSpan.FromMilliseconds(100));

            Assert.That(blackboard.SelectorOnEnterCount, Is.EqualTo(1));
            Assert.That(blackboard.ActionOneOnEnterCount, Is.EqualTo(2));
            Assert.That(blackboard.ActionTwoOnEnterCount, Is.EqualTo(2));
        }

        private class MockBlackboard
        {
            public int ActionOneOnEnterCount;
            public int ActionTwoOnEnterCount;
            public int Counter;
            public int SelectorOnEnterCount;
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
                    .With.OnEnter<MockBlackboard>(bb => bb.SelectorOnEnterCount++);
                {
                    var a1 = selector.Add.Action<MockBlackboard>(CountUpAction);
                    {
                        a1
                            .With.If<MockBlackboard>(bb => bb.Counter < 2)
                            .With.OnEnter<MockBlackboard>(bb => bb.ActionOneOnEnterCount++);
                    }
                    var a2 = selector.Add.Action<MockBlackboard>(CountUpAction);
                    {
                        a2.With.OnEnter<MockBlackboard>(bb => bb.ActionTwoOnEnterCount++);
                    }
                }
            }
        }
    }
}