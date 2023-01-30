// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

namespace Edanoue.StateMachine
{
    /// <summary>
    /// </summary>
    /// <typeparam name="TTrigger"></typeparam>
    public interface IRunningStateMachine<in TTrigger>
    {
        /// <summary>
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public bool SendTrigger(TTrigger trigger);
    }
}