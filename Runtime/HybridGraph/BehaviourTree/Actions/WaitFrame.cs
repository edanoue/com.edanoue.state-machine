// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Edanoue.HybridGraph
{
    public static class WaitFrameExtensions
    {
        private const string _DEFAULT_NAME = "WaitFrame";

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="waitFrameCount"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IActionNode WaitFrame(this ICompositePort self, int waitFrameCount, string name = _DEFAULT_NAME)
        {
            var node = new BtActionNodeWaitFrame(self, name, waitFrameCount);
            return node;
        }
    }

    internal sealed class BtActionNodeWaitFrame : BtActionNode
    {
        private readonly int _waitFrameCount;

        internal BtActionNodeWaitFrame(ICompositePort port, string name, int waitFrameCount) : base(port, name)
        {
            _waitFrameCount = waitFrameCount;
        }

        protected override async UniTask<BtNodeResult> ExecuteAsync(CancellationToken token)
        {
            await UniTask.DelayFrame(_waitFrameCount, PlayerLoopTiming.Update, token);
            return BtNodeResult.Succeeded;
        }
    }
}