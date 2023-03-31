// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.HybridGraph
{
    public static class OnEnterExtensions
    {
        private const string _DEFAULT_NODE_NAME = "OnEnter";

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static BtDecoratorNodeOnEnter<T> OnEnter<T>(this IDecoratorPort self, Action<T> action)
        {
            return self.OnEnter(action, _DEFAULT_NODE_NAME);
        }

        public static BtDecoratorNodeOnEnter<T> OnEnter<T>(this IDecoratorPort self, Action<T> action, string name)
        {
            var node = new BtDecoratorNodeOnEnter<T>(self, name, action);
            return node;
        }

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static BtDecoratorNodeOnEnter OnEnter(this IDecoratorPort self, Action action)
        {
            return self.OnEnter(action, _DEFAULT_NODE_NAME);
        }

        public static BtDecoratorNodeOnEnter OnEnter(this IDecoratorPort self, Action action, string name)
        {
            var node = new BtDecoratorNodeOnEnter(self, name, action);
            return node;
        }
    }

    public sealed class BtDecoratorNodeOnEnter : BtDecoratorNode
    {
        private readonly Action _action;

        public BtDecoratorNodeOnEnter(IDecoratorPort port, string name, Action action) : base(port, name)
        {
            _action = action;
        }

        internal override void OnEnter()
        {
            _action.Invoke();
        }
    }

    public sealed class BtDecoratorNodeOnEnter<T> : BtDecoratorNode
    {
        private readonly Action<T> _action;

        public BtDecoratorNodeOnEnter(IDecoratorPort port, string name, Action<T> action) : base(port, name)
        {
            _action = action;
        }

        internal override void OnEnter()
        {
            _action.Invoke((T)Blackboard);
        }
    }
}