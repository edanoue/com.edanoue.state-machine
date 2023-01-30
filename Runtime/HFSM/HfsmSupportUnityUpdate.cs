// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

namespace Edanoue.StateMachine
{
    public class HfsmSupportUnityUpdate<TContext, TTrigger> :
        HierarchicalStateMachine<TContext, TTrigger>
    {
        public HfsmSupportUnityUpdate(TContext context) : base(context)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="deltaTime"></param>
        public void DynamicUpdate(float deltaTime)
        {
            (_currentState as LeafStateSupportUnityUpdate)?.DynamicUpdate(deltaTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="deltaTime"></param>
        public void FixedUpdate(float deltaTime)
        {
            (_currentState as LeafStateSupportUnityUpdate)?.FixedUpdate(deltaTime);
        }

        public abstract class LeafStateSupportUnityUpdate : LeafState
        {
            protected internal virtual void DynamicUpdate(float deltaTime)
            {
            }

            protected internal virtual void FixedUpdate(float deltaTime)
            {
            }
        }
    }
}