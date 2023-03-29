// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
namespace Edanoue.HybridGraph
{
    public interface IRootNode
    {
        public ICompositePort Add { get; }
    }

    public interface IActionNode
    {
        public IDecoratorPort With { get; }
    }

    public interface IDecoratorNode
    {
        public IDecoratorPort With { get; }
    }

    public interface ICompositeNode
    {
        public ICompositePort Add { get; }
        public IDecoratorPort With { get; }
    }

    public interface ICompositePort
    {
        internal void AddNode(BtExecutableNode node);
    }

    public interface IDecoratorPort
    {
        internal void AddDecorator(BtDecoratorNode decorator);
    }
}