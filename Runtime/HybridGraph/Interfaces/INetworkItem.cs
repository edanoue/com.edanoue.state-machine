// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
namespace Edanoue.HybridGraph
{
    public interface INetworkItem
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

    public interface IContainer : INetworkItem
    {
        internal bool IsDescendantNode(INetworkItem node);
    }

    public interface INode : INetworkItem
    {
        internal bool TryGetNextNode(int trigger, out INode nextNode);
    }
}