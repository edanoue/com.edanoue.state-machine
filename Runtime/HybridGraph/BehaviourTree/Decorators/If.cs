// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.HybridGraph
{
    public static class ConditionExtensions
    {
        public static IDecoratorNode If<T>(this IDecoratorPort self, Func<T, bool> condition)
        {
            return self.If(condition, "Condition");
        }

        public static IDecoratorNode If<T>(this IDecoratorPort self, Func<T, bool> condition, string name)
        {
            var node = new BtDecoratorNodeIf<T>(condition, name);
            self.AddDecorator(node);
            return node;
        }
    }

    internal sealed class BtDecoratorNodeIf<T> : BtDecoratorNode
    {
        private readonly Func<T, bool> _condition;

        internal BtDecoratorNodeIf(Func<T, bool> condition, string name) : base(name)
        {
            _condition = condition;
        }

        internal override bool CanExecute()
        {
            return _condition.Invoke((T)Blackboard);
        }
    }
}