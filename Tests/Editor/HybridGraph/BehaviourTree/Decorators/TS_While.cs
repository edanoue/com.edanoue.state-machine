// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Edanoue.HybridGraph.Tests
{
    public class TS_While
    {
        [Test]
        public async Task WhileDecoratorが機能する()
        {
            // Blackboard のカウンタが 5 になるまでループするシーケンサを持つ BT を起動する
            var blackboard = new MockBlackboard();
            var graph = EdaGraph.Run<MockBtA>(blackboard);

            // (南) 無限ループによる Stack 防止のため, 100ms 感覚でしか実行されないような設定にしています
            Assert.That(blackboard.Counter, Is.EqualTo(2));
            await UniTask.Delay(TimeSpan.FromMilliseconds(100));
            Assert.That(blackboard.Counter, Is.EqualTo(4));
            await UniTask.Delay(TimeSpan.FromMilliseconds(100));
            Assert.That(blackboard.Counter, Is.EqualTo(6)); // ここで抜けている
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
                    // BB のカウンタが 5 になるまでループする Decorator をつける
                    sequence.With.While<MockBlackboard>(bb => bb.Counter < 5);
                    // カウンタアップを行う Action を 2 回追加する
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                    sequence.Add.Action<MockBlackboard>(CountUpAction);
                }
            }
        }
    }
}