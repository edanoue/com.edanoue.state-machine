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

        internal BtCompositeNode(string name) : base(name)
        {
        }

        public ICompositePort Add => new BtCompositeNodePort(Blackboard, Children);
        public IDecoratorPort With => new BtDecoratorPort(Blackboard, Decorators);

        /// <summary>
        /// Get next child index to process and store it in the context
        /// </summary>
        /// <param name="prevChild"></param>
        /// <param name="lastResult"></param>
        /// <returns></returns>
        protected abstract int GetNextChildIndex(int prevChild, in BtNodeResult lastResult);

        internal override int FindChildToExecute(ref BtNodeResult lastResult)
        {
            if (Children.Count == 0)
            {
                return BtSpecialChild.RETURN_TO_PARENT;
            }

            var childIndex = GetNextChildIndex(_currentChildIndex, in lastResult);
            while (childIndex >= 0 && childIndex < Children.Count)
            {
                // Check decorators
                if (DoDecoratorsAllowExecution(childIndex))
                {
                    return childIndex;
                }

                lastResult = BtNodeResult.Failed;
                childIndex = GetNextChildIndex(childIndex, lastResult);
            }

            return BtSpecialChild.RETURN_TO_PARENT;
        }

        private bool DoDecoratorsAllowExecution(int childIndex)
        {
            var child = Children[childIndex];

            if (child.Decorators.Count == 0)
            {
                return true;
            }

            for (var decoratorIndex = 0; decoratorIndex < child.Decorators.Count; decoratorIndex++)
            {
                var decorator = child.Decorators[decoratorIndex];
                if (decorator.CanExecute())
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        internal override async UniTask<BtNodeResult> ExecuteAsync(CancellationToken token)
        {
            var lastResult = BtNodeResult.Failed;
            _currentChildIndex = FindChildToExecute(ref lastResult);

            while (_currentChildIndex != BtSpecialChild.RETURN_TO_PARENT)
            {
                lastResult = await Children[_currentChildIndex].ExecuteAsync(token);
                _currentChildIndex = FindChildToExecute(ref lastResult);
            }

            return lastResult;
        }
    }
}