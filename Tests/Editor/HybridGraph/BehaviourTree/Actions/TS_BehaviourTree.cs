// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Edanoue.HybridGraph.BehaviourTree.Actions
{
    public class TS_BehaviourTree
    {
        [Test]
        public void BehaviourTreeをBehaviourTreeに埋め込める()
        {
            var blackboard = new MockBlackboard();
            var graph = EdaGraph.Run<MockBtA>(blackboard);
            Assert.That(blackboard.Counter, Is.EqualTo(6));
        }

        [Test]
        public void SubBehaviourTreeが失敗する()
        {
            var blackboard = new MockBlackboard();
            var graph = EdaGraph.Run<MockBtB>(blackboard);
            Assert.That(blackboard.Counter, Is.EqualTo(3));
        }

        [Test]
        public async Task Decoratorが動く()
        {
            var blackboard = new MockBlackboard();
            var graph = EdaGraph.Run<MockBtC>(blackboard);
            Assert.That(blackboard.Counter, Is.EqualTo(2));
            await UniTask.Delay(TimeSpan.FromMilliseconds(100));
            Assert.That(blackboard.Counter, Is.EqualTo(4));
            await UniTask.Delay(TimeSpan.FromMilliseconds(100));
            Assert.That(blackboard.Counter, Is.EqualTo(4));
        }

        private class MockBlackboard
        {
            public int Counter;
        }

        private class MockBtA : BehaviourTree<MockBlackboard>
        {
            // Blackboard のカウンターを一つ進める Action
            private static bool CountUpAction(MockBlackboard bb)
            {
                bb.Counter++;
                return true;
            }

            protected override void OnSetupBehaviours(IRootNode root)
            {
                var sequence = root.Add.Sequence();
                // SubBehaviourTree を追加する
                sequence.Add.BehaviourTree<MockBtASubBtA>(); // 3回カウントアップ
                sequence.Add.Action<MockBlackboard>(CountUpAction);
                sequence.Add.BehaviourTree<MockBtASubBtB>(); // 2回カウントアップ
            }

            private class MockBtASubBtA : BehaviourTree<MockBlackboard>
            {
                protected override void OnSetupBehaviours(IRootNode root)
                {
                    var sequence = root.Add.Sequence();
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                }
            }

            private class MockBtASubBtB : BehaviourTree<MockBlackboard>
            {
                protected override void OnSetupBehaviours(IRootNode root)
                {
                    var sequence = root.Add.Sequence();
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                }
            }
        }

        private class MockBtB : BehaviourTree<MockBlackboard>
        {
            // Blackboard のカウンターを一つ進める Action
            private static bool CountUpAction(MockBlackboard bb)
            {
                bb.Counter++;
                return true;
            }

            protected override void OnSetupBehaviours(IRootNode root)
            {
                var selector = root.Add.Selector();
                // SubBehaviourTree を追加する
                selector.Add.BehaviourTree<MockSubBtA>(); // 1回カウントアップ(途中で失敗する)
                selector.Add.BehaviourTree<MockSubBtB>(); // 2回カウントアップ
            }

            private class MockSubBtA : BehaviourTree<MockBlackboard>
            {
                protected override void OnSetupBehaviours(IRootNode root)
                {
                    var sequence = root.Add.Sequence();
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                    sequence.Add.Action(() => false);
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                }
            }

            private class MockSubBtB : BehaviourTree<MockBlackboard>
            {
                protected override void OnSetupBehaviours(IRootNode root)
                {
                    var sequence = root.Add.Sequence();
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                }
            }
        }

        private class MockBtC : BehaviourTree<MockBlackboard>
        {
            // Blackboard のカウンターを一つ進める Action
            private static bool CountUpAction(MockBlackboard bb)
            {
                bb.Counter++;
                return true;
            }

            protected override void OnSetupBehaviours(IRootNode root)
            {
                var selector = root.Add.Selector();
                // SubBehaviourTree を追加する
                selector.Add.BehaviourTree<MockSubBtA>()
                    .With.While<MockBlackboard>(bb => bb.Counter < 3);
            }

            private class MockSubBtA : BehaviourTree<MockBlackboard>
            {
                protected override void OnSetupBehaviours(IRootNode root)
                {
                    var sequence = root.Add.Sequence();
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                }
            }
        }
    }
}