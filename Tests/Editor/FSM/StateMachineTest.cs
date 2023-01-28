// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using NUnit.Framework;

namespace Edanoue.StateMachine.Tests
{
    public class StateMachineTest
    {
        [Test]
        public void StateMachine生成のテスト()
        {
            var stateMachine = new StateMachine<StateMachineTest, int>(this);
            Assert.NotNull(stateMachine);
        }
    }
}