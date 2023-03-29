// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.HybridGraph
{
    public interface IGraphItem : IDisposable
    {
        /// <summary>
        /// </summary>
        internal IGraphNode RootNode { get; }

        /// <summary>
        /// </summary>
        /// <param name="blackboard"></param>
        /// <param name="parent"></param>
        internal void Initialize(object blackboard, IGraphBox? parent);

        /// <summary>
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="nextNode"></param>
        internal void Connect(int trigger, IGraphItem nextNode);

        internal void OnEnterInternal();

        internal void OnUpdateInternal();

        internal void OnExitInternal(IGraphItem nextNode);
    }
}