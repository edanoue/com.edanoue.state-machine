// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using NUnit.Framework;

namespace Edanoue.HybridGraph.Tests
{
    public class TS_Sequence
    {
        [Test]
        public void すべて成功するSequenceのテスト()
        {
            var blackboard = new MockBlackboard();
            var graph = EdaGraph.Run<MockBtA>(blackboard);
            Assert.That(blackboard.Counter, Is.EqualTo(3));
        }

        [Test]
        public void 途中で失敗するSequenceのテスト()
        {
            var blackboard = new MockBlackboard();
            var graph = EdaGraph.Run<MockBtB>(blackboard);
            Assert.That(blackboard.Counter, Is.EqualTo(2));
        }

        [Test]
        public void 子ノードがすべて成功すると親も成功する()
        {
            var blackboard = new MockBlackboard();
            var graph = EdaGraph.Run<MockBtC>(blackboard);
            Assert.That(blackboard.Counter, Is.EqualTo(9));
        }

        [Test]
        public void 子ノードのいずれかが失敗すると親も失敗する()
        {
            var blackboard = new MockBlackboard();
            var graph = EdaGraph.Run<MockBtD>(blackboard);
            Assert.That(blackboard.Counter, Is.EqualTo(5));
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
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
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
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                    sequence.Add.Action(() => false);
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                }
            }
        }

        private class MockBtC : BehaviourTree<MockBlackboard>
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
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                    // 子ノードがすべて成功する (次に進む)
                    var cSeqA = sequence.Add.Sequence();
                    {
                        cSeqA.Add.Action<MockBlackboard>(CountUpAction);
                        cSeqA.Add.Action<MockBlackboard>(CountUpAction);
                    }
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                    var cSeqB = sequence.Add.Sequence();
                    {
                        cSeqB.Add.Action<MockBlackboard>(CountUpAction);
                        var ccSeqA = cSeqB.Add.Sequence();
                        {
                            ccSeqA.Add.Action<MockBlackboard>(CountUpAction);
                            ccSeqA.Add.Action<MockBlackboard>(CountUpAction);
                        }
                        cSeqB.Add.Action<MockBlackboard>(CountUpAction);
                    }
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                }
            }
        }

        private class MockBtD : BehaviourTree<MockBlackboard>
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
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                    var cSeqA = sequence.Add.Sequence();
                    {
                        cSeqA.Add.Action<MockBlackboard>(CountUpAction);
                        cSeqA.Add.Action<MockBlackboard>(CountUpAction);
                    }
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                    var cSeqB = sequence.Add.Sequence();
                    {
                        cSeqB.Add.Action<MockBlackboard>(CountUpAction);
                        // 子ノードが失敗すると親も失敗する
                        var ccSeqA = cSeqB.Add.Sequence();
                        {
                            ccSeqA.Add.Action(() => false);
                            ccSeqA.Add.Action<MockBlackboard>(CountUpAction);
                        }
                        cSeqB.Add.Action<MockBlackboard>(CountUpAction);
                    }
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                }
            }
        }
    }
}