// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Edanoue.HybridGraph
{
    public abstract class BtCompositeNode : BtExecutableNode, ICompositeNode
    {
        protected readonly List<BtExecutableNode> Children           = new();
        private            int                    _currentChildIndex = BtSpecialChild.NOT_INITIALIZED;

        public ICompositePort Add => new BtCompositeNodePort(Blackboard, Children);
        public IDecoratorPort With => new BtDecoratorPort(Blackboard, Decorators);

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
        private int FindChildToExecute(ref BtNodeResult lastResult)
        {
            if (Children.Count == 0)
            {
                return BtSpecialChild.RETURN_TO_PARENT;
            }

            var childIndex = GetNextChildIndex(_currentChildIndex, in lastResult);
            while (childIndex >= 0 && childIndex < Children.Count)
            {
                // Check decorators
                if (DoDecoratorsAllowEnter(childIndex))
                {
                    return childIndex;
                }

                lastResult = BtNodeResult.Failed;
                childIndex = GetNextChildIndex(childIndex, lastResult);
            }

            return BtSpecialChild.RETURN_TO_PARENT;
        }

        private bool DoDecoratorsAllowEnter(int childIndex)
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
                if (decorator.CanEnter())
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private bool DoThisDecoratorsAllowExit()
        {
            // No decorators, allow exit
            if (Decorators.Count == 0)
            {
                return true;
            }

            for (var decoratorIndex = 0; decoratorIndex < Decorators.Count; decoratorIndex++)
            {
                var decorator = Decorators[decoratorIndex];
                if (decorator.CanExit())
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        internal override async UniTask<BtNodeResult> ExecuteAsync(CancellationToken token)
        {
            // 無限ループ によるハングを防止する Stopwatch
            // 以下のシチュエーションで無限ループが発生
            // 子に Async 系のノードが無いとき かつ このノードに無限ループ系の Decorator が付いているとき
            var stopWatch = new Stopwatch();

            while (true)
            {
                stopWatch.Restart();

                // --- OnNodeActivation ---
                // ノードに入ってきた時の初期化処理
                _currentChildIndex = BtSpecialChild.NOT_INITIALIZED;
                var lastResult = BtNodeResult.Failed;

                // 次に実行する子のノードの Index を取得する
                _currentChildIndex = FindChildToExecute(ref lastResult);

                while (_currentChildIndex != BtSpecialChild.RETURN_TO_PARENT)
                {
                    lastResult = await Children[_currentChildIndex].ExecuteAsync(token);
                    _currentChildIndex = FindChildToExecute(ref lastResult);
                }

                stopWatch.Stop();

                // --- OnNodeDeactivation ---
                // 自身の Decorator により Exit が許可されたら親に戻る
                if (DoThisDecoratorsAllowExit())
                {
                    return lastResult;
                }

                // 無限ループ(によるハング)防止用の Await 処理
                // TODO: ここでの最低の待機感覚, Global に設定できるようにするか, Decorator ごとに設定できるようにするべき
                var elapsedMilliseconds = stopWatch.ElapsedMilliseconds;
                if (elapsedMilliseconds < 100)
                {
                    await UniTask.Delay(TimeSpan.FromMilliseconds(100 - elapsedMilliseconds), cancellationToken: token);
                }
            }
        }
    }
}