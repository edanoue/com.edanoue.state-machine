// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Edanoue.HybridGraph.Tests
{
    public class TS_BehaviourTree
    {
        [Test]
        public void Health10以上のときの挙動の確認()
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
            // Health 10 未満のときは

            // Blackboard の作成
            var blackboard = new MockBlackboard
            {
                Health = 5 // MoveToTower Action が実行されるように Health を設定
            };
            EdaGraph.Run<MinionBehaviourTree>(blackboard);

            Assert.That(blackboard.ActionAttackCallCount, Is.EqualTo(0));
            Assert.That(blackboard.ActionMoveToTowerCallCount, Is.EqualTo(1));
            Assert.That(blackboard.ActionJumpCallCount, Is.EqualTo(0));

            // Wait to finish MoveToTower and wait Action
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f));

            Assert.That(blackboard.ActionAttackCallCount, Is.EqualTo(0));
            Assert.That(blackboard.ActionMoveToTowerCallCount, Is.EqualTo(1));
            Assert.That(blackboard.ActionJumpCallCount, Is.EqualTo(0));

            await UniTask.Delay(TimeSpan.FromSeconds(0.31f));

            Assert.That(blackboard.ActionAttackCallCount, Is.EqualTo(0));
            Assert.That(blackboard.ActionMoveToTowerCallCount, Is.EqualTo(1));
            Assert.That(blackboard.ActionJumpCallCount, Is.EqualTo(1));
        }

        private class MockBlackboard
        {
            public int   ActionAttackCallCount;
            public int   ActionJumpCallCount;
            public int   ActionMoveToTowerCallCount;
            public float Health;
        }


        private class MinionBehaviourTree : BehaviourTree<MockBlackboard>
        {
            protected override void OnSetupBehaviours(IRootNode root)
            {
                // Selector/
                //   Attack Action <If: Health > 10>
                //   Sequence/ 
                //     Move to tower (0.2 sec)
                //     Wait (0.3 sec)
                //     Jump 

                // セレクタを作成する
                var selectorA = root.Add.Selector("Selector A");
                {
                    static async UniTask<bool> AttackAction(MockBlackboard bb, CancellationToken token)
                    {
                        // テスト用の BB の更新
                        bb.ActionAttackCallCount++;

                        // 1秒かけて敵を攻撃する
                        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token);
                        return true;
                    }

                    // A. HP が 10 以上の時に 5m 以内の敵を攻撃する Task 
                    selectorA.Add.ActionAsync<MockBlackboard>(AttackAction, "Attack")
                        .With.If<MockBlackboard>(x => x.Health > 10f);

                    // B. シーケンス
                    var sequence = selectorA.Add.Sequence();
                    {
                        // B-1. 一番前のタワーに行く
                        sequence.Add.ActionAsync<MockBlackboard>(async (bb, token) =>
                        {
                            bb.ActionMoveToTowerCallCount++;

                            // 0.2秒 かけてタワーに移動する
                            await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: token);
                            return true;
                        }, "MoveToTower");

                        // B-2. 0.3 秒 待機する (後隙)
                        sequence.Add.Wait(TimeSpan.FromSeconds(0.2f));

                        // B-3. ジャンプする
                        sequence.Add.Action<MockBlackboard>(bb =>
                        {
                            bb.ActionJumpCallCount++;
                            return true;
                        });
                    }
                }
            }
        }
    }
}