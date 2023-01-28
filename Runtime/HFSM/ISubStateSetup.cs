// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

namespace Edanoue.StateMachine
{
    /// <summary>
    /// </summary>
    public interface ISubStateSetup<TContext, TTrigger>
    {
        public void AddTransition<TPrevState, TNextState>(TTrigger trigger)
            where TPrevState : HierarchicalStateMachine<TContext, TTrigger>.Node, new()
            where TNextState : HierarchicalStateMachine<TContext, TTrigger>.Node, new();

        public void SetInitialState<T>()
            where T : HierarchicalStateMachine<TContext, TTrigger>.Node, new();
    }
}