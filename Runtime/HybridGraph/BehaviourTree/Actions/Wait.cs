﻿// Copyright Edanoue, Inc. All Rights Reserved.

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

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="timeSpan"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IActionNode Wait(this ICompositePort self, TimeSpan timeSpan, string name)
        {
            var node = new BtActionNodeWait(timeSpan, name);
            self.AddNode(node);
            return node;
        }
    }

    internal sealed class BtActionNodeWait : BtActionNode
    {
        private readonly TimeSpan _timeSpan;

        internal BtActionNodeWait(TimeSpan timeSpan, string name) : base(name)
        {
            _timeSpan = timeSpan;
        }

        internal override async UniTask<BtNodeResult> ExecuteAsync(CancellationToken token)
        {
            await UniTask.Delay(_timeSpan, cancellationToken: token);
            return BtNodeResult.Succeeded;
        }
    }
}