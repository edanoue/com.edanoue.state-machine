// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace Edanoue.HybridGraph.BehaviourTree.BehaviourTree
{
    public class TS_OnEndExecute
    {
        [Test]
        public void RootにSucceededで戻ってくるBTをRunする()
        {
            // 1回の実行で Root に戻ってくる BTの 実行
            // すべての Behaviour が実行に成功する
            var bb = new MockBlackboard();
            EdaGraph.Run<MockBehaviourTreeA>(bb);
            Assert.That(bb.OnEndExecuteCounter, Is.EqualTo(1));
            Assert.That(bb.OnEndExecuteResult, Is.EqualTo(BtNodeResult.Succeeded));
        }

        [Test]
        public void RootにFailedで戻ってくるBTをRunする()
        {
            // 1回の実行で Root に戻ってくる BTの 実行
            // 途中の Behaviour で Failed が帰ってくる
            var bb = new MockBlackboard();
            EdaGraph.Run<MockBehaviourTreeB>(bb);
            Assert.That(bb.OnEndExecuteCounter, Is.EqualTo(1));
            Assert.That(bb.OnEndExecuteResult, Is.EqualTo(BtNodeResult.Failed));
        }

        [Test]
        public async Task LoopするBtをRunする()
        {
            // 内部で Loop する BTの 実行
            var bb = new MockBlackboard();
            var graph = EdaGraph.Run<MockBehaviourTreeC>(bb);
            // 実行直後は Loop しているので結果が返ってこない
            Assert.That(bb.OnEndExecuteCounter, Is.EqualTo(0));

            // Graph を Dispose すると Cancelled が帰ってくる
            graph.Dispose();
            await UniTask.Delay(TimeSpan.FromMilliseconds(1)); // 次のフレームで呼ばれる
            Assert.That(bb.OnEndExecuteCounter, Is.EqualTo(1));
            Assert.That(bb.OnEndExecuteResult, Is.EqualTo(BtNodeResult.Cancelled));
        }

        private static bool SuccessAction(MockBlackboard bb)
        {
            return true;
        }

        private static bool FailedAction(MockBlackboard bb)
        {
            return false;
        }

        private class MockBlackboard
        {
            public int          OnEndExecuteCounter;
            public BtNodeResult OnEndExecuteResult;
        }

        private sealed class MockBehaviourTreeA : BehaviourTree<MockBlackboard>
        {
            protected override void OnSetupBehaviours(IRootNode root)
            {
                var seq = root.Add.Sequence();
                {
                    seq.Add.Action<MockBlackboard>(SuccessAction);
                }
            }

            protected override void OnEndExecute(BtNodeResult result)
            {
                Blackboard.OnEndExecuteCounter++;
                Blackboard.OnEndExecuteResult = result;
            }
        }

        private sealed class MockBehaviourTreeB : BehaviourTree<MockBlackboard>
        {
            protected override void OnSetupBehaviours(IRootNode root)
            {
                var seq = root.Add.Sequence();
                {
                    seq.Add.Action<MockBlackboard>(FailedAction);
                }
            }

            protected override void OnEndExecute(BtNodeResult result)
            {
                Blackboard.OnEndExecuteCounter++;
                Blackboard.OnEndExecuteResult = result;
            }
        }

        private sealed class MockBehaviourTreeC : BehaviourTree<MockBlackboard>
        {
            protected override void OnSetupBehaviours(IRootNode root)
            {
                var seq = root.Add.Sequence();
                seq.With.Loop(-1);
                {
                    seq.Add.Action<MockBlackboard>(SuccessAction);
                }
            }

            protected override void OnEndExecute(BtNodeResult result)
            {
                Blackboard.OnEndExecuteCounter++;
                Blackboard.OnEndExecuteResult = result;
            }
        }
    }
}