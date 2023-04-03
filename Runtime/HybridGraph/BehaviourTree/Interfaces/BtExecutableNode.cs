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

        internal object BlackboardRaw { get; private protected set; } = null!;

        internal virtual void SetBlackboardRaw(object blackboardRaw)
        {
            BlackboardRaw = blackboardRaw;
        }

        internal async UniTask<BtNodeResult> WrappedExecuteAsync(CancellationToken token)
        {
            // --------- Before OnEnter -----------
            if (!DoDecoratorsAllowEnter(out var abortResult))
            {
                // If any decorator returns false, execution failed
                return abortResult switch
                {
                    BtNodeResultForce.Failed => BtNodeResult.Failed,
                    BtNodeResultForce.Succeeded => BtNodeResult.Succeeded,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            // ---------    OnEnter     -----------
            WrappedOnEnter();

            // ---------    OnExecute   -----------
            _onExecuteCts = new CancellationTokenSource();
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _onExecuteCts.Token);
            while (true)
            {
                foreach (var decorator in Decorators)
                {
                    // Call decorators OnExecute
                    decorator.OnPreExecute();
                }

                _stopwatchOnExecute.Restart();
                var (isCancelled, result) = await ExecuteAsync(linkedCts.Token).SuppressCancellationThrow();
                _stopwatchOnExecute.Stop();

                if (isCancelled)
                {
                    if (_forceExitStatus != BtForceExitStatus.None)
                    {
                        // Force OnExit with decorator
                        result = _forceExitStatus switch
                        {
                            BtForceExitStatus.ForceExitSucceeded => BtNodeResult.Succeeded,
                            BtForceExitStatus.ForceExitFailed => BtNodeResult.Failed,
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        _forceExitStatus = BtForceExitStatus.None;
                        // OnExit
                        WrappedOnExit();
                        return result;
                    }

                    return BtNodeResult.Cancelled;
                }

                if (DoDecoratorsAllowExit())
                {
                    // OnExit
                    WrappedOnExit();
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WrappedOnEnter()
        {
            foreach (var decorator in Decorators)
            {
                // Call decorators OnEnter
                decorator.OnEnter(this);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WrappedOnExit()
        {
            _onExecuteCts = null;
            foreach (var decorator in Decorators)
            {
                // Call decorators OnExit
                decorator.OnExit();
            }
        }

        internal void RequestForceExit(BtNodeResultForce result)
        {
            if (_onExecuteCts is null) // 実行中の CTS が発行されていない(ので実行中ではない)
            {
                return;
            }

            _onExecuteCts.Cancel();
            _forceExitStatus = result switch
            {
                BtNodeResultForce.Failed => BtForceExitStatus.ForceExitFailed,
                BtNodeResultForce.Succeeded => BtForceExitStatus.ForceExitSucceeded,
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
            };
        }

        protected abstract UniTask<BtNodeResult> ExecuteAsync(CancellationToken token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool DoDecoratorsAllowEnter(out BtNodeResultForce abortResult)
        {
            var decoratorCount = Decorators.Count;
            abortResult = BtNodeResultForce.Failed;

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

                abortResult = decorator.GetAbortResult();
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


        private enum BtForceExitStatus
        {
            None,
            ForceExitSucceeded,
            ForceExitFailed
        }
    }
}