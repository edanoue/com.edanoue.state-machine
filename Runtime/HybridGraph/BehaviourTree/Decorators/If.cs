// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.HybridGraph
{
    public static class IfExtensions
    {
        private const string _DEFAULT_NAME = "If";

        /// <summary>
        /// <para>指定した condition を満たす場合のみノードを実行する Decorator</para>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="condition">bool を返す Func</param>
        /// <typeparam name="T">Condition 内で使用する Blackboard の型</typeparam>
        /// <returns></returns>
        public static IDecoratorNode If<T>(this IDecoratorPort self, Func<T, bool> condition)
        {
            return self.If(condition, _DEFAULT_NAME);
        }

        public static IDecoratorNode If<T>(this IDecoratorPort self, Func<T, bool> condition, string name)
        {
            var node = new BtDecoratorNodeIf<T>(self, name, condition);
            return node;
        }
    }

    internal sealed class BtDecoratorNodeIf<T> : BtDecoratorNode<T>
    {
        private readonly Func<T, bool> _condition;

        internal BtDecoratorNodeIf(IDecoratorPort port, string name, Func<T, bool> condition) : base(port, name)
        {
            _condition = condition;
        }

        internal override bool CanEnter()
        {
            return _condition.Invoke(Blackboard);
        }
    }
}