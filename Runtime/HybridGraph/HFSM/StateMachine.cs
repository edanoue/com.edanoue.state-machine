// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Collections.Generic;

namespace Edanoue.HybridGraph
{
    public abstract class StateMachine<TBlackboard> : IContainer, IStateBuilder
    {
        private readonly HashSet<INetworkItem> _children = new();

        private INetworkItem? _initialState;

        private bool _lockOnEnterInternal;

        private   IContainer? _parent;
        protected TBlackboard Blackboard = default!;

        void INetworkItem.Initialize(object blackboard, IContainer? parent)
        {
            if (_initialState is not null)
            {
                throw new InvalidOperationException("State machine is already started.");
            }

            Blackboard = (TBlackboard)blackboard ?? throw new ArgumentNullException(nameof(blackboard));
            _parent = parent;
            OnSetupStates(this);

            if (_initialState is null)
            {
                throw new InvalidOperationException("Initial state is not set.");
            }

            OnInitialize();
        }

        void INetworkItem.Connect(int trigger, INetworkItem nextNode)
        {
            foreach (var child in _children)
            {
                child.Connect(trigger, nextNode);
            }
        }

        void INetworkItem.OnEnterInternal()
        {
            if (_lockOnEnterInternal)
            {
                return;
            }

            _lockOnEnterInternal = true;
            _parent?.OnEnterInternal();
            OnEnter();
        }

        void INetworkItem.OnStayInternal()
        {
            OnStay();
        }

        void INetworkItem.OnExitInternal(INetworkItem nextNode)
        {
            if (!_lockOnEnterInternal)
            {
                return;
            }

            // 次のノードが自分の子孫ノードなら Container の OnExit は実行しない
            if (IsDescendantNode(nextNode))
            {
                return;
            }

            OnExit();
            _parent?.OnExitInternal(nextNode);
            _lockOnEnterInternal = false;
        }

        bool IContainer.IsDescendantNode(INetworkItem node)
        {
            return IsDescendantNode(node);
        }

        INode INetworkItem.RootNode => _initialState!.RootNode;

        void IStateBuilder.SetInitialState<TState>()
        {
            if (_initialState is not null)
            {
                throw new InvalidOperationException("Initial state is already set.");
            }

            _initialState = GetOrCreateState<TState>();
        }

        void IStateBuilder.AddTransition<TPrev, TNext>(int trigger)
        {
            var prevState = GetOrCreateState<TPrev>();
            var nextState = GetOrCreateState<TNext>();
            prevState.Connect(trigger, nextState);
        }

        private bool IsDescendantNode(INetworkItem node)
        {
            foreach (var child in _children)
            {
                if (child == node)
                {
                    return true;
                }

                if (child is IContainer container)
                {
                    if (container.IsDescendantNode(node))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private TState GetOrCreateState<TState>()
            where TState : class, INetworkItem, new()
        {
            // 既に生成されている State ならそれを返す
            foreach (var state in _children)
            {
                if (state is TState t)
                {
                    return t;
                }
            }

            // State の新規作成
            var newState = new TState();
            try
            {
                newState.Initialize(Blackboard!, this);
            }
            catch (InvalidCastException)
            {
                throw new InvalidOperationException(
                    $"{typeof(TBlackboard)} is not compatible with {typeof(TState)} blackboard type.");
            }

            _children.Add(newState);
            return newState;
        }

        protected virtual void OnInitialize()
        {
        }

        protected virtual void OnEnter()
        {
        }

        protected virtual void OnStay()
        {
        }

        protected virtual void OnExit()
        {
        }

        /// <summary>
        /// Define states and transitions. Called once when the state machine is initialized.
        /// </summary>
        /// <param name="builder"></param>
        protected abstract void OnSetupStates(IStateBuilder builder);
    }
}