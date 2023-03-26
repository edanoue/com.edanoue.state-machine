// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
namespace Edanoue.HybridGraph
{
    public interface IContainer : INetworkItem
    {
        internal bool IsDescendantNode(INetworkItem node);
    }
}