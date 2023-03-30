// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using NUnit.Framework;

namespace Edanoue.HybridGraph.Tests
{
    public class TS_BehaviourTree
    {
        [Test]
        public void BehaviourTreeの動作確認()
        {
            var blackboard = new MockBlackboard();
            var graph = EdaGraph.Run<MockBtA>(blackboard);
            Assert.That(blackboard.Counter, Is.EqualTo(6));
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
    }
}