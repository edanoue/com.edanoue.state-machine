// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

namespace Edanoue.StateMachine.Tests.VendingMachine
{
    public enum Trigger
    {
        お金が入った,
        お金が足りた,
        お金が足りない,
        ジュース購入ボタンを押す,
        おつり返却ボタンを押す,
        お釣りの排出が終わった
    }
}