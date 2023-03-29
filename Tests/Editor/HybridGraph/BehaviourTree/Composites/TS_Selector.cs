// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using NUnit.Framework;

namespace Edanoue.HybridGraph.Tests
{
    public class TS_Selector
    {
        [Test]
        public void 子ノードがすべて成功する()
        {
            var blackboard = new MockBlackboard();
            var graph = EdaGraph.Run<MockBtA>(blackboard);
            Assert.That(blackboard.Counter, Is.EqualTo(1));
        }

        [Test]
        public void 最初が失敗するSelectorのテスト()
        {
            var blackboard = new MockBlackboard();
            var graph = EdaGraph.Run<MockBtB>(blackboard);
            Assert.That(blackboard.Counter, Is.EqualTo(3));
        }


        [Test]
        public void 子ノードがすべて失敗すると親も失敗する()
        {
            var blackboard = new MockBlackboard();
            var graph = EdaGraph.Run<MockBtC>(blackboard);
            Assert.That(blackboard.Counter, Is.EqualTo(3));
        }

        [Test]
        public void 子ノードのいずれかが成功すると親も成功する()
        {
            var blackboard = new MockBlackboard();
            var graph = EdaGraph.Run<MockBtD>(blackboard);
            Assert.That(blackboard.Counter, Is.EqualTo(1));
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

                var selector = root.Add.Selector();
                {
                    selector.Add.Action<MockBlackboard>(CountUpAction);
                    selector.Add.Action<MockBlackboard>(CountUpAction);
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

                var selector = root.Add.Selector();
                {
                    selector.Add.Action(() => false);
                    var sequence = selector.Add.Sequence();
                    {
                        sequence.Add.Action<MockBlackboard>(CountUpAction);
                        sequence.Add.Action<MockBlackboard>(CountUpAction);
                        sequence.Add.Action<MockBlackboard>(CountUpAction);
                    }
                    selector.Add.Action<MockBlackboard>(CountUpAction);
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

                var selector = root.Add.Selector();
                {
                    // 子ノードがすべて失敗すると親も失敗する
                    var cSel = selector.Add.Selector();
                    {
                        cSel.Add.Action(() => false);
                        cSel.Add.Action(() => false);
                        cSel.Add.Action(() => false);
                    }
                    // この Sequence が成功する (3回カウントアップ)
                    var cSeq = selector.Add.Sequence();
                    {
                        cSeq.Add.Action<MockBlackboard>(CountUpAction);
                        cSeq.Add.Action<MockBlackboard>(CountUpAction);
                        cSeq.Add.Action<MockBlackboard>(CountUpAction);
                    }
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

                var selector = root.Add.Selector();
                {
                    // 子ノードがどれか成功すると親も成功する
                    var cSel = selector.Add.Selector();
                    {
                        cSel.Add.Action(() => false);
                        cSel.Add.Action<MockBlackboard>(CountUpAction);
                        cSel.Add.Action(() => false);
                    }
                    // この Sequence は実行されない
                    var cSeq = selector.Add.Sequence();
                    {
                        cSeq.Add.Action<MockBlackboard>(CountUpAction);
                        cSeq.Add.Action<MockBlackboard>(CountUpAction);
                        cSeq.Add.Action<MockBlackboard>(CountUpAction);
                    }
                }
            }
        }
    }
}