// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.HybridGraph
{
    public abstract class BtDecoratorNode : BtNode, IDecoratorNode
    {
        protected BtDecoratorNode(IDecoratorPort port, string name)
        {
            With = port;
            NodeName = name;
            port.AddDecorator(this);
        }

        public IDecoratorPort With { get; }

        internal virtual bool CanEnter()
        {
            return true;
        }

        internal virtual bool CanExit()
        {
            return true;
        }

        internal virtual void OnEnter()
        {
        }

        internal virtual void OnExecute()
        {
        }

        internal virtual void OnExit()
        {
        }
    }

    public abstract class BtDecoratorNode<T> : BtDecoratorNode
    {
        protected readonly T Blackboard;

        protected BtDecoratorNode(IDecoratorPort port, string name) : base(port, name)
        {
            try
            {
                Blackboard = (T)port.Blackboard;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException($"Blackboard が {typeof(T)} を実装していません", e);
            }
        }
    }
}