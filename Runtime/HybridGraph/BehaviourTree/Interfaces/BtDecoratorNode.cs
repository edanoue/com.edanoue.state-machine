// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
namespace Edanoue.HybridGraph
{
    public abstract class BtDecoratorNode : BtNode, IDecoratorNode
    {
        protected BtDecoratorNode(IDecoratorPort port, string name)
        {
            And = port;
            Blackboard = port.Blackboard;
            NodeName = name;
            port.AddDecorator(this);
        }

        public IDecoratorPort And { get; }

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

        internal virtual void OnExit()
        {
        }
    }
}