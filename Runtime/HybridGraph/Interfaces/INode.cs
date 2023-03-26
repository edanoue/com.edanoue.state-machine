// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
namespace Edanoue.HybridGraph
{
    public interface INode : INetworkItem
    {
        internal bool TryGetNextNode(int trigger, out INode nextNode);
    }
}