// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

namespace Edanoue.StateMachine
{
    public class FsmSupportUnityUpdate<TContext, TTrigger> : StateMachine<TContext, TTrigger>
    {
        public FsmSupportUnityUpdate(TContext context) : base(context)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="deltaTime"></param>
        public void DynamicUpdate(float deltaTime)
        {
            (_currentState as StateSupportUnityUpdate)?.DynamicUpdate(deltaTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="deltaTime"></param>
        public void FixedUpdate(float deltaTime)
        {
            (_currentState as StateSupportUnityUpdate)?.FixedUpdate(deltaTime);
        }

        public abstract class StateSupportUnityUpdate : State
        {
            protected internal void DynamicUpdate(float deltaTime)
            {
            }

            protected internal void FixedUpdate(float deltaTime)
            {
            }
        }
    }
}