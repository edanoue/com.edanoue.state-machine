// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Edanoue.HybridGraph
{
    public abstract class BtExecutableNode : BtNode
    {
        // (南) Loop 系のノードとすぐ終わる Action の組み合わせで発生するハングを防止するためのタイマー
        private const     int                   _MINIMUM_LOOP_TIME_MS = 100;
        private readonly  Stopwatch             _sw                   = new();
        internal readonly List<BtDecoratorNode> Decorators            = new();

        internal async UniTask<BtNodeResult> WrappedExecuteAsync(CancellationToken token)
        {
            // --------- Before OnEnter -----------
            if (!DoDecoratorsAllowEnter())
            {
                return BtNodeResult.Failed;
            }

            // ---------    OnEnter     -----------
            // Call decorators OnEnter
            foreach (var decorator in Decorators)
            {
                decorator.OnEnter();
            }

            while (true)
            {
                // Call decorators OnReEnter
                foreach (var decorator in Decorators)
                {
                    decorator.OnExecute();
                }

                _sw.Restart();
                var result = await ExecuteAsync(token);
                _sw.Stop();
                if (DoDecoratorsAllowExit())
                {
                    // OnExit
                    foreach (var decorator in Decorators)
                    {
                        decorator.OnExit();
                    }

                    return result;
                }

                if (_sw.ElapsedMilliseconds < _MINIMUM_LOOP_TIME_MS)
                {
                    await UniTask.Delay(TimeSpan.FromMilliseconds(_MINIMUM_LOOP_TIME_MS - _sw.ElapsedMilliseconds),
                        cancellationToken: token);
                }
            }
        }

        protected abstract UniTask<BtNodeResult> ExecuteAsync(CancellationToken token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool DoDecoratorsAllowEnter()
        {
            var decoratorCount = Decorators.Count;
            if (decoratorCount == 0)
            {
                return true;
            }

            for (var i = 0; i < decoratorCount; i++)
            {
                var decorator = Decorators[i];
                if (decorator.CanEnter())
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool DoDecoratorsAllowExit()
        {
            var decoratorCount = Decorators.Count;
            if (decoratorCount == 0)
            {
                return true;
            }

            for (var i = 0; i < decoratorCount; i++)
            {
                var decorator = Decorators[i];
                if (decorator.CanExit())
                {
                    continue;
                }

                return false;
            }

            return true;
        }
    }
}