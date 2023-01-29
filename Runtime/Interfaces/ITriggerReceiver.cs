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
        /// <param name="autoUpdate"></param>
        /// <returns></returns>
        public bool SendTrigger(TTrigger trigger, bool autoUpdate = false);

        /// <summary>
        /// </summary>
        public void UpdateState();
    }
}