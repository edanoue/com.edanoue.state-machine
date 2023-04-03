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
        /// <param name="abortResult"></param>
        /// <param name="name"></param>
        /// <typeparam name="T">Condition 内で使用する Blackboard の型</typeparam>
        /// <returns></returns>
        public static IDecoratorNode If<T>(this IDecoratorPort self, Func<T, bool> condition,
            BtNodeResultForce abortResult = BtNodeResultForce.Failed, string name = _DEFAULT_NAME)
        {
            var node = new BtDecoratorNodeIf<T>(self, name, condition, abortResult);
            return node;
        }
    }

    internal sealed class BtDecoratorNodeIf<T> : BtDecoratorNode<T>
    {
        private readonly BtNodeResultForce _abortResult;
        private readonly Func<T, bool>     _condition;

        internal BtDecoratorNodeIf(IDecoratorPort port, string name, Func<T, bool> condition,
            BtNodeResultForce abortResult) : base(port, name)
        {
            _condition = condition;
            _abortResult = abortResult;
        }

        internal override bool CanEnter()
        {
            return _condition.Invoke(Blackboard);
        }

        internal override BtNodeResultForce GetAbortResult()
        {
            return _abortResult;
        }
    }
}