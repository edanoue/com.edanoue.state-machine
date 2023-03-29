// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.HybridGraph
{
    public abstract class BtDecoratorNode : BtNode, IDecoratorNode
    {
        protected BtDecoratorNode(string name) : base(name)
        {
        }

        public IDecoratorPort With => throw new NotImplementedException();

        internal abstract bool CanExecute();
    }
}