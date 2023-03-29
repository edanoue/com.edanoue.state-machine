// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Edanoue.HybridGraph
{
    public static class ActionExtensions
    {
        public static IActionNode Action(this ICompositePort self, Func<bool> action)
        {
            return self.Action(action, "Action");
        }

        public static IActionNode Action(this ICompositePort self, Func<bool> action, string name)
        {
            var node = new BtActionNodeAction(action, name);
            self.AddNode(node);
            return node;
        }

        public static IActionNode Action<T>(this ICompositePort self, Func<T, bool> action)
        {
            return self.Action(action, "Action");
        }

        public static IActionNode Action<T>(this ICompositePort self, Func<T, bool> action, string name)
        {
            var node = new BtActionNodeAction<T>(action, name);
            self.AddNode(node);
            return node;
        }
    }

    internal sealed class BtActionNodeAction : BtActionNode
    {
        private readonly Func<bool> _action;

        internal BtActionNodeAction(Func<bool> action, string name) : base(name)
        {
            _action = action;
        }

        internal override UniTask<BtNodeResult> ExecuteAsync(CancellationToken token)
        {
            if (_action.Invoke())
            {
                return UniTask.FromResult(BtNodeResult.Succeeded);
            }

            return UniTask.FromResult(BtNodeResult.Failed);
        }
    }

    internal sealed class BtActionNodeAction<T> : BtActionNode
    {
        private readonly Func<T, bool> _action;

        internal BtActionNodeAction(Func<T, bool> action, string name) : base(name)
        {
            _action = action;
        }

        internal override UniTask<BtNodeResult> ExecuteAsync(CancellationToken token)
        {
            if (_action.Invoke((T)Blackboard))
            {
                return UniTask.FromResult(BtNodeResult.Succeeded);
            }

            return UniTask.FromResult(BtNodeResult.Failed);
        }
    }
}