// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.HybridGraph
{
    public static class ConditionExtensions
    {
        public static IDecoratorNode Condition<T>(this IDecoratorPort self, Func<T, bool> condition)
        {
            return self.Condition(condition, "Condition");
        }

        public static IDecoratorNode Condition<T>(this IDecoratorPort self, Func<T, bool> condition, string name)
        {
            var node = new BtDecoratorNodeCondition<T>(condition, name);
            self.AddDecorator(node);
            return node;
        }
    }

    internal sealed class BtDecoratorNodeCondition<T> : BtDecoratorNode
    {
        private readonly Func<T, bool> _condition;

        internal BtDecoratorNodeCondition(Func<T, bool> condition, string name) : base(name)
        {
            _condition = condition;
        }

        internal override bool CanExecute()
        {
            return _condition.Invoke((T)Blackboard);
        }
    }
}