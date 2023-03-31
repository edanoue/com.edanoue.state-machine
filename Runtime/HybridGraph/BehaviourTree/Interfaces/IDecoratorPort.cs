// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
namespace Edanoue.HybridGraph
{
    public interface IDecoratorPort
    {
        internal object Blackboard { get; }
        internal void AddDecorator(BtDecoratorNode decorator);
    }
}