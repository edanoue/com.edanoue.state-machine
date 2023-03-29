﻿// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Edanoue.HybridGraph
{
    public static class ActionAsyncExtensions
    {
        private const string _DEFAULT_NAME = "ActionAsync";

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="asyncAction"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IActionNode ActionAsync<T>(this ICompositePort self,
            Func<T, CancellationToken, UniTask<bool>> asyncAction)
        {
            return self.ActionAsync(asyncAction, _DEFAULT_NAME);
        }

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="asyncAction"></param>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IActionNode ActionAsync<T>(this ICompositePort self,
            Func<T, CancellationToken, UniTask<bool>> asyncAction, string name)
        {
            var node = new BtActionNodeActionAsync<T>(asyncAction, name);
            self.AddNode(node);
            return node;
        }
    }

    internal sealed class BtActionNodeActionAsync<T> : BtActionNode
    {
        private readonly Func<T, CancellationToken, UniTask<bool>> _action;

        public BtActionNodeActionAsync(Func<T, CancellationToken, UniTask<bool>> action, string name) : base(name)
        {
            _action = action;
        }

        internal override async UniTask<BtNodeResult> ExecuteAsync(CancellationToken token)
        {
            var result = await _action.Invoke((T)Blackboard, token);
            return result ? BtNodeResult.Succeeded : BtNodeResult.Failed;
        }
    }
}