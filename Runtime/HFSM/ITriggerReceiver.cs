// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

namespace Edanoue.StateMachine
{
    /// <summary>
    /// </summary>
    /// <typeparam name="TTrigger"></typeparam>
    public interface ITriggerReceiver<in TTrigger>
    {
        /// <summary>
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public bool SendTrigger(TTrigger trigger);

        /// <summary>
        /// </summary>
        public void UpdateState();

        /// <summary>
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public bool SendTriggerAndUpdateState(TTrigger trigger);
    }
}