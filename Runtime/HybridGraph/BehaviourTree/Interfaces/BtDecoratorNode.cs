// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
namespace Edanoue.HybridGraph
{
    public abstract class BtDecoratorNode : BtNode, IDecoratorNode
    {
        protected BtDecoratorNode(string name) : base(name)
        {
        }

        internal abstract bool CanEnter();

        internal abstract bool CanExit();
    }
}