// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Edanoue.HybridGraph.BehaviourTree.Decorators
{
    public class TS_Loop
    {
        [Test]
        public async Task LoopDecoratorが機能する()
        {
            var blackboard = new MockBlackboard();
            var graph = EdaGraph.Run<MockBtA>(blackboard);

            // (南) 無限ループによる Stack 防止のため, 100ms 感覚でしか実行されないような設定にしています
            Assert.That(blackboard.Counter, Is.EqualTo(3));
            await UniTask.Delay(TimeSpan.FromMilliseconds(100));
            Assert.That(blackboard.Counter, Is.EqualTo(6));
            await UniTask.Delay(TimeSpan.FromMilliseconds(100));
            Assert.That(blackboard.Counter, Is.EqualTo(6));
        }

        [Test]
        public async Task NestedLoopDecoratorが機能する()
        {
            var blackboard = new MockBlackboard();
            var graph = EdaGraph.Run<MockBtB>(blackboard);

            // (南) 無限ループによる Stack 防止のため, 100ms 感覚でしか実行されないような設定にしています
            Assert.That(blackboard.Counter, Is.EqualTo(2));
            await UniTask.Delay(TimeSpan.FromMilliseconds(100));
            Assert.That(blackboard.Counter, Is.EqualTo(5));
            await UniTask.Delay(TimeSpan.FromMilliseconds(100));
            Assert.That(blackboard.Counter, Is.EqualTo(6));
            await UniTask.Delay(TimeSpan.FromMilliseconds(100));
            Assert.That(blackboard.Counter, Is.EqualTo(6));
        }

        private class MockBlackboard
        {
            public int Counter;
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

                var sequence = root.Add.Sequence();
                {
                    sequence.With.Loop(2);
                    {
                        sequence.Add.Action<MockBlackboard>(CountUpAction);
                        sequence.Add.Action<MockBlackboard>(CountUpAction);
                        sequence.Add.Action<MockBlackboard>(CountUpAction);
                    }
                }
            }
        }

        private class MockBtB : BehaviourTree<MockBlackboard>
        {
            protected override void OnSetupBehaviours(IRootNode root)
            {
                static bool CountUpAction(MockBlackboard bb)
                {
                    bb.Counter++;
                    return true;
                }

                var sequence = root.Add.Sequence();
                {
                    sequence.With.Loop(2);
                    {
                        sequence.Add.Action<MockBlackboard>(CountUpAction);
                        sequence.Add.Action<MockBlackboard>(CountUpAction)
                            .With.Loop(2);
                    }
                }
            }
        }
    }
}