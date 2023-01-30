// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using NUnit.Framework;

namespace Edanoue.StateMachine.Tests
{
    public class TS_FSM
    {
        [Test]
        public void StateMachine生成のテスト()
        {
            var stateMachine = new StateMachine<TS_FSM, int>(this);
            Assert.NotNull(stateMachine);
        }
    }
}