// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.HybridGraph
{
    public static class WhileExtensions
    {
        private const string _DEFAULT_NAME = "While";

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="condition"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IDecoratorNode While<T>(this IDecoratorPort self, Func<T, bool> condition)
        {
            return self.While(condition, _DEFAULT_NAME);
        }

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="condition"></param>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IDecoratorNode While<T>(this IDecoratorPort self, Func<T, bool> condition, string name)
        {
            var node = new BtDecoratorNodeWhile<T>(condition, name);
            self.AddDecorator(node);
            return node;
        }
    }

    internal sealed class BtDecoratorNodeWhile<T> : BtDecoratorNode
    {
        private readonly Func<T, bool> _condition;

        internal BtDecoratorNodeWhile(Func<T, bool> condition, string name) : base(name)
        {
            _condition = condition;
        }

        internal override bool CanEnter()
        {
            return _condition.Invoke((T)Blackboard);
        }

        internal override bool CanExit()
        {
            return !_condition.Invoke((T)Blackboard);
        }
    }
}