// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace Edanoue.HybridGraph.Tests
{
    public class TS_HybridGraph_Dispose
    {
        [Test]
        [Category("Abnormal")]
        public async Task DisposeAndWait()
        {
            var blackboard = new BlackboardMockA();

            // Run Graph
            var graph = EdaGraph.Run<BehaviourTreeMockA>(blackboard);
            Assert.That(blackboard.Counter, Is.EqualTo(1));

            // Wait 500ms
            await UniTask.Delay(TimeSpan.FromMilliseconds(510));
            Assert.That(blackboard.Counter, Is.EqualTo(2));

            // Wait 200ms
            await UniTask.Delay(TimeSpan.FromMilliseconds(400));
            Assert.That(blackboard.Counter, Is.EqualTo(2));

            // Dispose
            graph.Dispose();
            Assert.That(blackboard.Counter, Is.EqualTo(2));

            // Do not running graph
            await UniTask.Delay(TimeSpan.FromMilliseconds(600));
            Assert.That(blackboard.Counter, Is.EqualTo(2));
        }

        private static bool CountUpAction(BlackboardMockA bb)
        {
            bb.Counter++;
            return true;
        }

        public class BlackboardMockA
        {
            public int Counter;
        }

        private class BehaviourTreeMockA : BehaviourTree<BlackboardMockA>
        {
            protected override void OnSetupBehaviours(IRootNode root)
            {
                var sel = root.Add.Selector();
                sel.With.Loop(-1);

                {
                    var seq = sel.Add.Sequence();
                    {
                        seq.Add.Action<BlackboardMockA>(CountUpAction);
                        seq.Add.Wait(TimeSpan.FromMilliseconds(500));
                    }
                }
            }
        }
    }
}