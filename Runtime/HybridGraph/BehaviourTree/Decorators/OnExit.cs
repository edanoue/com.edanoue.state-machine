// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.HybridGraph
{
    public static class OnExitExtensions
    {
        private const string _DEFAULT_NODE_NAME = "OnExit";

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static BtDecoratorNodeOnExit<T> OnExit<T>(this IDecoratorPort self, Action<T> action)
        {
            return self.OnExit(action, _DEFAULT_NODE_NAME);
        }

        public static BtDecoratorNodeOnExit<T> OnExit<T>(this IDecoratorPort self, Action<T> action, string name)
        {
            var node = new BtDecoratorNodeOnExit<T>(self, name, action);
            return node;
        }

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static BtDecoratorNodeOnExit OnExit(this IDecoratorPort self, Action action)
        {
            return self.OnExit(action, _DEFAULT_NODE_NAME);
        }

        public static BtDecoratorNodeOnExit OnExit(this IDecoratorPort self, Action action, string name)
        {
            var node = new BtDecoratorNodeOnExit(self, name, action);
            return node;
        }
    }

    public sealed class BtDecoratorNodeOnExit : BtDecoratorNode
    {
        private readonly Action _action;

        public BtDecoratorNodeOnExit(IDecoratorPort port, string name, Action action) : base(port, name)
        {
            _action = action;
        }

        internal override void OnExit()
        {
            _action.Invoke();
        }
    }

    public sealed class BtDecoratorNodeOnExit<T> : BtDecoratorNode
    {
        private readonly Action<T> _action;

        public BtDecoratorNodeOnExit(IDecoratorPort port, string name, Action<T> action) : base(port, name)
        {
            _action = action;
        }

        internal override void OnExit()
        {
            _action.Invoke((T)Blackboard);
        }
    }
}