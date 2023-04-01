// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Edanoue.HybridGraph
{
    public enum TimeLimitResult
    {
        Failed,
        Succeeded
    }

    public static class TimeLimitExtensions
    {
        private const string _DEFAULT_NODE_NAME = "TimeLimit";

        /// <summary>
        /// 指定した時間が経過した際, ノードを強制終了する Decorator
        /// </summary>
        /// <param name="self"></param>
        /// <param name="timeLimit">時間を指定</param>
        /// <param name="result">指定した時間が経過した際にノードが返す値</param>
        /// <returns></returns>
        public static BtDecoratorNodeTimeLimit TimeLimit(this IDecoratorPort self, TimeSpan timeLimit,
            TimeLimitResult result)
        {
            return self.TimeLimit(timeLimit, result, _DEFAULT_NODE_NAME);
        }

        public static BtDecoratorNodeTimeLimit TimeLimit(this IDecoratorPort self, TimeSpan timeLimit,
            TimeLimitResult result, string name)
        {
            var node = new BtDecoratorNodeTimeLimit(self, name, timeLimit, result);
            return node;
        }
    }

    public sealed class BtDecoratorNodeTimeLimit : BtDecoratorNode
    {
        private readonly TimeLimitResult          _result;
        private readonly TimeSpan                 _timeLimit;
        private          CancellationTokenSource? _timerCts;

        public BtDecoratorNodeTimeLimit(IDecoratorPort port, string name, TimeSpan timeLimit, TimeLimitResult result) :
            base(port, name)
        {
            _timeLimit = timeLimit;
            _result = result;
        }

        internal override void OnEnter(BtExecutableNode node)
        {
            _timerCts = new CancellationTokenSource();
            UniTask.Void(async token =>
            {
                await UniTask.Delay(_timeLimit, cancellationToken: token);
                if (!token.IsCancellationRequested)
                {
                    var status = _result switch
                    {
                        TimeLimitResult.Failed => BtExecutableNode.BtForceExitStatus.ForceExitFailed,
                        TimeLimitResult.Succeeded => BtExecutableNode.BtForceExitStatus.ForceExitSucceeded,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    node.RequestForceExit(status);
                }
            }, _timerCts.Token);
        }

        internal override void OnExit()
        {
            _timerCts?.Cancel();
            _timerCts?.Dispose();
            _timerCts = null;
        }
    }
}