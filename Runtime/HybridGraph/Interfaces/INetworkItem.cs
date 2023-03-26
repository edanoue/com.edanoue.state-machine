// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.HybridGraph
{
    public interface INetworkItem : IDisposable
    {
        /// <summary>
        /// </summary>
        internal INode RootNode { get; }

        /// <summary>
        /// </summary>
        /// <param name="blackboard"></param>
        /// <param name="parent"></param>
        internal void Initialize(object blackboard, IContainer? parent);

        /// <summary>
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="nextNode"></param>
        internal void Connect(int trigger, INetworkItem nextNode);

        internal void OnEnterInternal();

        internal void OnStayInternal();

        internal void OnExitInternal(INetworkItem nextNode);
    }
}