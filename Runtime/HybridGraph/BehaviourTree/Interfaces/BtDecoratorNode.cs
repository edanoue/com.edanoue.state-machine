// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.HybridGraph
{
    /// <summary>
    /// Decorator の基底クラス. Blackboard にアクセスする必要がある場合は Generics 版を使用してください.
    /// </summary>
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

        internal virtual void OnEnter(BtExecutableNode node)
        {
        }

        internal virtual void OnPreExecute()
        {
        }

        internal virtual void OnExit()
        {
        }

        internal virtual BtNodeResultForce GetAbortResult()
        {
            return BtNodeResultForce.Failed;
        }
    }

    /// <summary>
    /// Blackboard にアクセスする Decorator の基底クラス
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BtDecoratorNode<T> : BtDecoratorNode
    {
        protected readonly T Blackboard;

        protected BtDecoratorNode(IDecoratorPort port, string name) : base(port, name)
        {
            try
            {
                Blackboard = (T)port.Blackboard;
            }
            catch (InvalidCastException)
            {
                throw new InvalidOperationException(
                    $"{NodeName} Decorator が要求する {typeof(T)} を Blackboard ({port.Blackboard.GetType()}) が実装していません.");
            }
        }
    }
}