// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

namespace Edanoue.StateMachine
{
    /// <summary>
    /// </summary>
    public interface IStateBuilder<TContext, TTrigger>
    {
        /// <summary>
        /// </summary>
        /// <param name="trigger"></param>
        /// <typeparam name="TPrevState"></typeparam>
        /// <typeparam name="TNextState"></typeparam>
        public void AddTransition<TPrevState, TNextState>(TTrigger trigger)
            where TPrevState : HierarchicalStateMachine<TContext, TTrigger>.Node, new()
            where TNextState : HierarchicalStateMachine<TContext, TTrigger>.Node, new();

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SetInitialState<T>()
            where T : HierarchicalStateMachine<TContext, TTrigger>.Node, new();
    }
}