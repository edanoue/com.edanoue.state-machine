// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using NUnit.Framework;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace Edanoue.HybridGraph.BehaviourTree.Decorators
{
    public class TS_If
    {
        [Test]
        public void IfDecoratorが機能する()
        {
            var blackboard = new MockBlackboard();
            EdaGraph.Run<MockBtA>(blackboard);
            Assert.That(blackboard.Counter, Is.EqualTo(0));
            Assert.That(blackboard.Result, Is.EqualTo(BtNodeResult.Failed));
        }

        [Test]
        public void AbortResult機能()
        {
            var blackboard = new MockBlackboard();
            EdaGraph.Run<MockBtB>(blackboard);
            Assert.That(blackboard.Counter, Is.EqualTo(1));
            Assert.That(blackboard.Result, Is.EqualTo(BtNodeResult.Succeeded));
        }

        private static bool CountUpAction(MockBlackboard bb)
        {
            bb.Counter++;
            return true;
        }

        private class MockBlackboard
        {
            public int          Counter;
            public BtNodeResult Result;
        }

        private class MockBtA : BehaviourTree<MockBlackboard>
        {
            protected override void OnSetupBehaviours(IRootNode root)
            {
                var sequence = root.Add.Sequence();
                {
                    sequence.Add.Action<MockBlackboard>(CountUpAction)
                        .With.If<MockBlackboard>(_ => false);
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                }
            }

            protected override void OnReturnedRootNode(BtNodeResult result)
            {
                Blackboard.Result = result;
            }
        }

        private class MockBtB : MockBtA
        {
            protected override void OnSetupBehaviours(IRootNode root)
            {
                var sequence = root.Add.Sequence();
                {
                    sequence.Add.Action<MockBlackboard>(CountUpAction)
                        .With.If<MockBlackboard>(_ => false, BtNodeResultForce.Succeeded);
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                }
            }
        }
    }
}