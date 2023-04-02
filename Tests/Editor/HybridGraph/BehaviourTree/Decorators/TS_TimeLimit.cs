// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Edanoue.HybridGraph.BehaviourTree.Decorators
{
    public class TS_TimeOut
    {
        [Test]
        public async Task TimeLimitDecoratorが機能する()
        {
            var bb = new MockBlackboard();
            using var graph = EdaGraph.Run<MockBtA>(bb);

            Assert.That(bb.Counter, Is.EqualTo(0));
            await UniTask.Delay(TimeSpan.FromSeconds(0.11f));
            Assert.That(bb.Counter, Is.EqualTo(1));
            await UniTask.Delay(TimeSpan.FromSeconds(0.11f));
            Assert.That(bb.Counter, Is.EqualTo(1)); // TimeLimit により 2 回目の Action は実行されない
        }

        [Test]
        public async Task TimeLimitDecoratorが子の動作も停止させる()
        {
            var bb = new MockBlackboard();
            using var graph = EdaGraph.Run<MockBtB>(bb);

            Assert.That(bb.Counter, Is.EqualTo(0));
            await UniTask.Delay(TimeSpan.FromMilliseconds(250));
            Assert.That(bb.Counter, Is.EqualTo(1));
            await UniTask.Delay(TimeSpan.FromMilliseconds(450));
            Assert.That(bb.Counter, Is.EqualTo(1)); // TimeLimit により 2 回目の Action は実行されない
        }

        [Test]
        public async Task TimeLimitDecoratorが結果を上書きする_Failed()
        {
            var bb = new MockBlackboard();
            using var graph = EdaGraph.Run<MockBtC>(bb);

            await UniTask.Delay(TimeSpan.FromMilliseconds(200));
            Assert.That(bb.Counter, Is.EqualTo(0));
        }

        [Test]
        public async Task TimeLimitDecoratorが結果を上書きする_Succeeded()
        {
            var bb = new MockBlackboard();
            using var graph = EdaGraph.Run<MockBtD>(bb);

            await UniTask.Delay(TimeSpan.FromMilliseconds(200));
            Assert.That(bb.Counter, Is.EqualTo(1));
        }

        private static async UniTask<bool> CountUpAction(MockBlackboard bb, CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromMilliseconds(100), cancellationToken: token);
            bb.Counter++;
            return true;
        }

        private static async UniTask<bool> CountUpActionTwo(MockBlackboard bb, CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromMilliseconds(200), cancellationToken: token);
            bb.Counter++;
            return true;
        }

        private sealed class MockBlackboard
        {
            public int Counter;
        }

        private class MockBtA : BehaviourTree<MockBlackboard>
        {
            protected override void OnSetupBehaviours(IRootNode root)
            {
                var seqA = root.Add.Sequence();
                {
                    var a = seqA.Add.ActionAsync<MockBlackboard>(CountUpAction);
                    a.With.TimeLimit(TimeSpan.FromSeconds(0.15f)).With.Loop(-1);
                }
            }
        }

        private class MockBtB : BehaviourTree<MockBlackboard>
        {
            protected override void OnSetupBehaviours(IRootNode root)
            {
                var seqA = root.Add.Sequence();
                seqA.With.TimeLimit(TimeSpan.FromMilliseconds(300));
                {
                    // 無限ループする Sequence
                    var seqB = seqA.Add.Sequence();
                    seqB.With.Loop(-1);
                    {
                        seqB.Add.ActionAsync<MockBlackboard>(CountUpActionTwo);
                    }
                }
            }
        }

        private class MockBtC : BehaviourTree<MockBlackboard>
        {
            protected override void OnSetupBehaviours(IRootNode root)
            {
                var seq = root.Add.Sequence();
                {
                    // TimeLimit により実行に失敗する Action
                    seq.Add.ActionAsync<MockBlackboard>(CountUpAction)
                        .With.TimeLimit(TimeSpan.FromMilliseconds(50));
                    seq.Add.ActionAsync<MockBlackboard>(CountUpAction);
                }
            }
        }

        private class MockBtD : BehaviourTree<MockBlackboard>
        {
            protected override void OnSetupBehaviours(IRootNode root)
            {
                var seq = root.Add.Sequence();
                {
                    // TimeLimit により実行は行われないが, 成功する Action
                    seq.Add.ActionAsync<MockBlackboard>(CountUpAction)
                        .With.TimeLimit(TimeSpan.FromMilliseconds(50), BtNodeResultForce.Succeeded);
                    seq.Add.ActionAsync<MockBlackboard>(CountUpAction);
                }
            }
        }
    }
}