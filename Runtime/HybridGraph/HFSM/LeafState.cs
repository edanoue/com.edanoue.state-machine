// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Collections.Generic;

namespace Edanoue.HybridGraph
{
    /// <summary>
    /// </summary>
    /// <typeparam name="TBlackboard"></typeparam>
    public abstract class LeafState<TBlackboard> : INode
    {
        /// <summary>
        /// 内部用のTransition Table
        /// 遷移先を辞書形式で保存している
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private readonly Dictionary<int, INode> _transitionTable = new();

        private IContainer? _parent;

        /// <summary>
        /// Get the blackboard.
        /// </summary>
        protected TBlackboard Blackboard = default!;

        void INetworkItem.Initialize(object blackboard, IContainer? parent)
        {
            Blackboard = (TBlackboard)blackboard;
            _parent = parent;
            OnInitialize();
        }

        void INetworkItem.Connect(int trigger, INetworkItem nextNode)
        {
            if (_transitionTable.ContainsKey(trigger))
            {
                throw new ArgumentException($"Already registered trigger: {trigger}");
            }

            _transitionTable.Add(trigger, nextNode.RootNode);
        }

        void INetworkItem.OnEnterInternal()
        {
            _parent?.OnEnterInternal();
            OnEnter();
        }

        void INetworkItem.OnStayInternal()
        {
            OnStay();
        }

        void INetworkItem.OnExitInternal(INetworkItem nextNode)
        {
            OnExit();
            _parent?.OnExitInternal(nextNode);
        }

        bool INode.TryGetNextNode(int trigger, out INode nextNode)
        {
            return _transitionTable.TryGetValue(trigger, out nextNode);
        }

        INode INetworkItem.RootNode => this;

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
    }
}