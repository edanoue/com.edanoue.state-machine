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
        private readonly  Stopwatch             _stopwatchOnExecute   = new();
        internal readonly List<BtDecoratorNode> Decorators            = new();

        // Decorator から ForceExit の命令が来ているかどうかのフラグ
        private BtForceExitStatus _forceExitStatus;

        // OnExecute のあいだだけ発行されているノードローカルの CTS
        private CancellationTokenSource? _onExecuteCts;

        internal object Blackboard { get; set; } = null!;

        internal async UniTask<BtNodeResult> WrappedExecuteAsync(CancellationToken token)
        {
            // --------- Before OnEnter -----------
            if (!DoDecoratorsAllowEnter())
            {
                // If any decorator returns false, execution failed
                return BtNodeResult.Failed;
            }

            // ---------    OnEnter     -----------
            foreach (var decorator in Decorators)
            {
                // Call decorators OnEnter
                decorator.OnEnter(this);
            }

            // ---------    OnExecute   -----------
            _onExecuteCts = new CancellationTokenSource();
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _onExecuteCts.Token);
            while (true)
            {
                foreach (var decorator in Decorators)
                {
                    // Call decorators OnExecute
                    decorator.OnExecute();
                }

                _stopwatchOnExecute.Restart();
                var (isCancelled, result) = await ExecuteAsync(linkedCts.Token).SuppressCancellationThrow();
                _stopwatchOnExecute.Stop();

                if (isCancelled)
                {
                    if (_forceExitStatus != BtForceExitStatus.None)
                    {
                        // OnExit (Force)
                        result = _forceExitStatus switch
                        {
                            BtForceExitStatus.ForceExitSucceeded => BtNodeResult.Succeeded,
                            BtForceExitStatus.ForceExitFailed => BtNodeResult.Failed,
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        _forceExitStatus = BtForceExitStatus.None;

                        _onExecuteCts = null;
                        foreach (var decorator in Decorators)
                        {
                            // Call decorators OnExit
                            decorator.OnExit();
                        }

                        return result;
                    }

                    return BtNodeResult.Aborted;
                }

                if (DoDecoratorsAllowExit())
                {
                    // OnExit
                    _onExecuteCts = null;
                    foreach (var decorator in Decorators)
                    {
                        // Call decorators OnExit
                        decorator.OnExit();
                    }

                    return result;
                }

                if (_stopwatchOnExecute.ElapsedMilliseconds < _MINIMUM_LOOP_TIME_MS)
                {
                    await UniTask.Delay(
                        TimeSpan.FromMilliseconds(_MINIMUM_LOOP_TIME_MS - _stopwatchOnExecute.ElapsedMilliseconds),
                        cancellationToken: linkedCts.Token);
                }
            }
        }

        internal void RequestForceExit(BtForceExitStatus result)
        {
            if (_onExecuteCts is null) // 実行中の CTS が発行されていない(ので実行中ではない)
            {
                return;
            }

            _onExecuteCts.Cancel();
            _forceExitStatus = result;
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

        internal enum BtForceExitStatus
        {
            None,
            ForceExitSucceeded,
            ForceExitFailed
        }
    }
}