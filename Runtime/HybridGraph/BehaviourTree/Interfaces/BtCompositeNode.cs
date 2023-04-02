// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Edanoue.HybridGraph
{
    public abstract class BtCompositeNode : BtExecutableNode, ICompositeNode
    {
        protected readonly List<BtExecutableNode> Children           = new();
        private            int                    _currentChildIndex = BtSpecialChild.NOT_INITIALIZED;

        public ICompositePort Add => new BtCompositePort(BlackboardRaw, Children);
        public IDecoratorPort With => new BtDecoratorPort(BlackboardRaw, Decorators);

        /// <summary>
        /// Get next child index to process and store it in the context
        /// </summary>
        /// <param name="prevChild"></param>
        /// <param name="lastResult"></param>
        /// <returns></returns>
        protected abstract int GetNextChildIndex(int prevChild, in BtNodeResult lastResult);

        /// <summary>
        /// </summary>
        /// <param name="lastResult"></param>
        /// <returns></returns>
        private int FindChildToExecute(in BtNodeResult lastResult)
        {
            if (Children.Count == 0)
            {
                return BtSpecialChild.RETURN_TO_PARENT;
            }

            var childIndex = GetNextChildIndex(_currentChildIndex, in lastResult);
            while (childIndex >= 0 && childIndex < Children.Count)
            {
                return childIndex;
            }

            return BtSpecialChild.RETURN_TO_PARENT;
        }

        private bool DoDecoratorsAllowExit(int childIndex)
        {
            var child = Children[childIndex];

            // No decorators, allow enter
            if (child.Decorators.Count == 0)
            {
                return true;
            }

            for (var decoratorIndex = 0; decoratorIndex < child.Decorators.Count; decoratorIndex++)
            {
                var decorator = child.Decorators[decoratorIndex];
                if (decorator.CanExit())
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        protected override async UniTask<BtNodeResult> ExecuteAsync(CancellationToken token)
        {
            // --- OnNodeActivation ---
            // ノードに入ってきた時の初期化処理
            _currentChildIndex = BtSpecialChild.NOT_INITIALIZED;
            var lastResult = BtNodeResult.Failed;

            // 次に実行する子のノードの Index を取得する
            _currentChildIndex = FindChildToExecute(in lastResult);

            while (_currentChildIndex != BtSpecialChild.RETURN_TO_PARENT)
            {
                lastResult = await Children[_currentChildIndex].WrappedExecuteAsync(token);
                _currentChildIndex = FindChildToExecute(in lastResult);
            }

            // --- OnNodeDeactivation ---
            // 自身の Decorator により Exit が許可されたら親に戻る
            return lastResult;
        }
    }
}