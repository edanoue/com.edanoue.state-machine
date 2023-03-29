// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Edanoue.HybridGraph.Tests
{
    public class TS_BehaviourTree
    {
        [Test]
        public async Task Health10以上のときの挙動の確認()
        {
            // Health 10 以上のときは
            // 1. Attack を実行 (1秒かかる)
            // 終了

            // Blackboard の作成
            var blackboard = new MockBlackboard
            {
                Health = 100 // Attack Action が実行されるように Health を設定
            };

            // Behaviour Tree を起動する
            EdaGraph.Run<MinionBehaviourTree>(blackboard);

            // Health が 10以上のため, Attack Action が実行されたことを確認する
            Assert.That(blackboard.ActionAttackCallCount, Is.EqualTo(1));
            Assert.That(blackboard.ActionMoveToTowerCallCount, Is.EqualTo(0));
        }

        [Test]
        public async Task Health10未満のときの挙動の確認()
        {
            // Health 10 以上のときは
            // 1. MoveTo を実行 (1秒かかる)
            // 終了

            // Blackboard の作成
            var blackboard = new MockBlackboard
            {
                Health = 5 // MoveToTower Action が実行されるように Health を設定
            };

            // Behaviour Tree を起動する
            EdaGraph.Run<MinionBehaviourTree>(blackboard);

            // Health が 10以上のため, Attack Action が実行されたことを確認する
            Assert.That(blackboard.ActionAttackCallCount, Is.EqualTo(0));
            Assert.That(blackboard.ActionMoveToTowerCallCount, Is.EqualTo(1));
        }

        private class MockBlackboard
        {
            public int   ActionAttackCallCount;
            public int   ActionMoveToTowerCallCount;
            public float Health;
        }


        private class MinionBehaviourTree : BehaviourTree<MockBlackboard>
        {
            protected override void OnSetupBehaviours(IRootNode root)
            {
                // Selector/
                //   Attack Action (If Health > 10)
                //   Sequence/ 
                //     Move to tower
                //     Wait

                // セレクタを作成する
                var selectorA = root.Add.Selector("Selector A");
                {
                    // A. HP が 10 以上の時に 5m 以内の敵を攻撃する Task 
                    selectorA.Add.ActionAsync<MockBlackboard>(async (bb, token) =>
                        {
                            // テスト用の BB の更新
                            bb.ActionAttackCallCount++;

                            // 1秒かけて敵を攻撃する
                            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token);
                            return true;
                        }, "Attack")
                        .With.Condition<MockBlackboard>(x => x.Health > 10f);

                    // B. シーケンス
                    var sequence = selectorA.Add.Sequence();
                    {
                        // B-1. 一番前のタワーに行く
                        sequence.Add.ActionAsync<MockBlackboard>(async (bb, token) =>
                        {
                            bb.ActionMoveToTowerCallCount++;

                            // 1秒かけてタワーに移動する
                            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token);
                            return true;
                        }, "Move to tower");

                        // B-2. 2秒 待機する (後隙)
                        sequence.Add.Wait(TimeSpan.FromSeconds(2f));
                    }
                }
            }
        }
    }
}