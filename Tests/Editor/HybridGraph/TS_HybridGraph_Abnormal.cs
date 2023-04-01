// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using NUnit.Framework;

namespace Edanoue.HybridGraph.Tests
{
    public class TS_HybridGraph_Abnormal
    {
        [Test]
        [Category("Abnormal")]
        public void Actionが要求する型をBlackboardが実装していない()
        {
            // Action が要求する Interface を Blackboard が実装していない場合, Run の時点で(実行時ではなく)例外が発生する
            var blackboard = new BlackboardMockA();
            Assert.Throws<InvalidOperationException>(() => EdaGraph.Run<BehaviourTreeMockA>(blackboard));
        }

        [Test]
        [Category("Abnormal")]
        public void Decoratorが要求する型をBlackboardが実装していない()
        {
            // Decorator が要求する Interface を Blackboard が実装していない場合, Run の時点で(実行時ではなく)例外が発生する
            var blackboard = new BlackboardMockA();
            Assert.Throws<InvalidOperationException>(() => EdaGraph.Run<BehaviourTreeMockB>(blackboard));
        }

        private class BlackboardMockA
        {
        }

        private interface IBlackboardInterfaceNotImpl
        {
        }

        private class BehaviourTreeMockA : BehaviourTree<BlackboardMockA>
        {
            protected override void OnSetupBehaviours(IRootNode root)
            {
                var sel = root.Add.Sequence();
                sel.Add.Action<BlackboardMockA>(_ => true);
                // 絶対に評価されない Action を追加する (実行時に呼ばれることがない)
                // Action が要求する Interface を Blackboard が実装していない
                sel.Add.Action<IBlackboardInterfaceNotImpl>(_ => true);
            }
        }

        private class BehaviourTreeMockB : BehaviourTree<BlackboardMockA>
        {
            protected override void OnSetupBehaviours(IRootNode root)
            {
                var sel = root.Add.Selector();
                sel.Add.Action<BlackboardMockA>(_ => true);
                // 絶対に評価されない Action を追加する (実行時に呼ばれることがない)
                // Action が要求する Interface を Blackboard が実装していない
                sel.Add.Action<BlackboardMockA>(_ => true).With.If<IBlackboardInterfaceNotImpl>(_ => false);
            }
        }
    }
}