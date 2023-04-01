// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.HybridGraph
{
    public abstract class BtActionNode : BtExecutableNode, IActionNode
    {
        protected BtActionNode(ICompositePort? port, string name)
        {
            // For BehaviourTreeBase
            if (port is null)
            {
                return;
            }

            NodeName = name;
            port.AddNode(this);
        }

        public IDecoratorPort With => new BtDecoratorPort(Blackboard, Decorators);
    }

    /// <summary>
    /// Blackboard にアクセスする Decorator の基底クラス
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BtActionNode<T> : BtActionNode
    {
        protected new readonly T Blackboard;

        protected BtActionNode(ICompositePort port, string name) : base(port, name)
        {
            try
            {
                Blackboard = (T)base.Blackboard;
            }
            catch (InvalidCastException)
            {
                throw new InvalidOperationException(
                    $"{NodeName} Action が要求する {typeof(T)} を Blackboard ({base.Blackboard.GetType()}) が実装していません.");
            }
        }
    }
}