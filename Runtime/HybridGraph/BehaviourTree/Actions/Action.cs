// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Edanoue.HybridGraph
{
    public static class ActionExtensions
    {
        private const string _DEFAULT_NAME = "Action";

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IActionNode Action(this ICompositePort self, Func<bool> action)
        {
            return self.Action(action, _DEFAULT_NAME);
        }

        public static IActionNode Action(this ICompositePort self, Func<bool> action, string name)
        {
            var node = new BtActionNodeAction(self, name, action);
            return node;
        }

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IActionNode Action<T>(this ICompositePort self, Func<T, bool> action)
        {
            return self.Action(action, _DEFAULT_NAME);
        }

        public static IActionNode Action<T>(this ICompositePort self, Func<T, bool> action, string name)
        {
            var node = new BtActionNodeAction<T>(self, name, action);
            return node;
        }
    }

    internal sealed class BtActionNodeAction : BtActionNode
    {
        private readonly Func<bool> _action;

        internal BtActionNodeAction(ICompositePort port, string name, Func<bool> action) : base(port, name)
        {
            _action = action;
        }

        protected override UniTask<BtNodeResult> ExecuteAsync(CancellationToken token)
        {
            return UniTask.FromResult(_action.Invoke() ? BtNodeResult.Succeeded : BtNodeResult.Failed);
        }
    }

    internal sealed class BtActionNodeAction<T> : BtActionNode<T>
    {
        private readonly Func<T, bool> _action;

        internal BtActionNodeAction(ICompositePort port, string name, Func<T, bool> action) : base(port, name)
        {
            _action = action;
        }

        protected override UniTask<BtNodeResult> ExecuteAsync(CancellationToken token)
        {
            return UniTask.FromResult(_action.Invoke(Blackboard) ? BtNodeResult.Succeeded : BtNodeResult.Failed);
        }
    }
}