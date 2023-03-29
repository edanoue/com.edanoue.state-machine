// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Collections.Generic;

namespace Edanoue.HybridGraph
{
    public abstract class StateMachine<TBlackboard> : IGraphBox, IStateBuilder
    {
        private readonly HashSet<IGraphItem> _children = new();

        private IGraphItem? _initialState;

        // If set, OnEnter will be called.
        private bool _isNotifyOnEnter;

        private   IGraphBox?  _parent;
        protected TBlackboard Blackboard = default!;

        void IGraphItem.Initialize(object blackboard, IGraphBox? parent)
        {
            if (_initialState is not null)
            {
                throw new InvalidOperationException("State machine is already started.");
            }

            Blackboard = (TBlackboard)blackboard ?? throw new ArgumentNullException(nameof(blackboard));
            _parent = parent;
            OnSetupStates(this);

            // Setup validation check
            if (_initialState is null)
            {
                throw new InvalidOperationException("Initial state is not set.");
            }

            OnInitialize();
        }

        void IGraphItem.Connect(int trigger, IGraphItem nextNode)
        {
            foreach (var child in _children)
            {
                child.Connect(trigger, nextNode);
            }
        }

        void IGraphItem.OnEnterInternal()
        {
            if (_isNotifyOnEnter)
            {
                return;
            }

            _isNotifyOnEnter = true;
            _parent?.OnEnterInternal();
            OnEnter();
        }

        void IGraphItem.OnUpdateInternal()
        {
            OnStay();
        }

        void IGraphItem.OnExitInternal(IGraphItem nextNode)
        {
            if (!_isNotifyOnEnter)
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
            _isNotifyOnEnter = false;
        }

        bool IGraphBox.IsDescendantNode(IGraphItem node)
        {
            return IsDescendantNode(node);
        }

        IGraphNode IGraphItem.RootNode => _initialState!.RootNode;

        public void Dispose()
        {
            foreach (var child in _children)
            {
                child.Dispose();
            }

            _children.Clear();
            OnDestroy();
        }

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

        private bool IsDescendantNode(IGraphItem node)
        {
            foreach (var child in _children)
            {
                if (child == node)
                {
                    return true;
                }

                if (child is IGraphBox container)
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
            where TState : class, IGraphItem, new()
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

        protected virtual void OnDestroy()
        {
        }

        /// <summary>
        /// Define states and transitions. Called once when the state machine is initialized.
        /// </summary>
        /// <param name="builder"></param>
        protected abstract void OnSetupStates(IStateBuilder builder);
    }
}