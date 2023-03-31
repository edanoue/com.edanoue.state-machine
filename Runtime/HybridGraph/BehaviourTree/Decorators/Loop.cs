// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.HybridGraph
{
    public static class LoopExtensions
    {
        private const string _DEFAULT_NAME = "Loop";

        /// <summary>
        /// 指定した回数ノードの実行を繰り返す Decorator. -1 を指定した場合は無限ループする
        /// </summary>
        /// <param name="self"></param>
        /// <param name="count">-1 を指定した場合は 無限ループ, または 1 以上の回数を指定</param>
        /// <returns></returns>
        public static IDecoratorNode Loop(this IDecoratorPort self, int count)
        {
            return self.Loop(count, _DEFAULT_NAME);
        }

        public static IDecoratorNode Loop(this IDecoratorPort self, int count, string name)
        {
            var node = new BtDecoratorNodeLoop(self, name, count);
            return node;
        }
    }

    internal sealed class BtDecoratorNodeLoop : BtDecoratorNode
    {
        private readonly int _startCount;
        private          int _counter;

        internal BtDecoratorNodeLoop(IDecoratorPort port, string name, int count) : base(port, name)
        {
            if (count is < -1 or 0)
            {
                throw new ArgumentException("count must be -1 or above 1", nameof(count));
            }

            _startCount = count;
            _counter = _startCount;
        }

        internal override bool CanEnter()
        {
            return _counter is -1 or > 0;
        }

        internal override bool CanExit()
        {
            return _counter == 0;
        }

        internal override void OnExecute()
        {
            if (_counter > 0)
            {
                _counter--;
            }
        }

        internal override void OnExit()
        {
            _counter = _startCount;
        }
    }
}