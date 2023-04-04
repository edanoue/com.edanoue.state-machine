// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Edanoue.HybridGraph
{
    public static class WaitExtensions
    {
        private const string _DEFAULT_NAME = "Wait";

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static IActionNode Wait(this ICompositePort self, TimeSpan timeSpan, string name = _DEFAULT_NAME)
        {
            var node = new BtActionNodeWait(self, name, timeSpan);
            return node;
        }
    }

    internal sealed class BtActionNodeWait : BtActionNode
    {
        private readonly TimeSpan _timeSpan;

        internal BtActionNodeWait(ICompositePort port, string name, TimeSpan timeSpan) : base(port, name)
        {
            _timeSpan = timeSpan;
        }

        protected override async UniTask<BtNodeResult> ExecuteAsync(CancellationToken token)
        {
            await UniTask.Delay(_timeSpan, cancellationToken: token);
            return BtNodeResult.Succeeded;
        }
    }
}