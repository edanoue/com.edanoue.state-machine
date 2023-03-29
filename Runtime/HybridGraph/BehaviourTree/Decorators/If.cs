﻿// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.HybridGraph
{
    public static class IfExtensions
    {
        private const string _DEFAULT_NAME = "If";

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="condition"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IDecoratorNode If<T>(this IDecoratorPort self, Func<T, bool> condition)
        {
            return self.If(condition, _DEFAULT_NAME);
        }

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="condition"></param>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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

        internal override bool CanEnter()
        {
            return _condition.Invoke((T)Blackboard);
        }

        internal override bool CanExit()
        {
            return true;
        }
    }
}