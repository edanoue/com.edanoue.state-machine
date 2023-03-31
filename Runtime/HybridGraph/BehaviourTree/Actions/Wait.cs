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
        public static IActionNode Wait(this ICompositePort self, TimeSpan timeSpan)
        {
            return self.Wait(timeSpan, _DEFAULT_NAME);
        }

        public static IActionNode Wait(this ICompositePort self, TimeSpan timeSpan, string name)
        {
            var node = new BtActionNodeWait(timeSpan);
            self.AddNode(node, name);
            return node;
        }
    }

    internal sealed class BtActionNodeWait : BtActionNode
    {
        private readonly TimeSpan _timeSpan;

        internal BtActionNodeWait(TimeSpan timeSpan)
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