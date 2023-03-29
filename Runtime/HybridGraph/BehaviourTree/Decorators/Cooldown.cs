// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.HybridGraph
{
    public static class CooldownExtensions
    {
        public static IDecoratorNode Cooldown(this IDecoratorPort self, in TimeSpan timeSpan)
        {
            return self.Cooldown(in timeSpan, "Cooldown");
        }

        public static IDecoratorNode Cooldown(this IDecoratorPort self, in TimeSpan timeSpan, string name)
        {
            var node = new BtDecoratorNodeCooldown(in timeSpan, name);
            self.AddDecorator(node);
            return node;
        }
    }

    internal sealed class BtDecoratorNodeCooldown : BtDecoratorNode
    {
        private readonly TimeSpan _timeSpan;

        public BtDecoratorNodeCooldown(in TimeSpan timeSpan, string name) : base(name)
        {
            _timeSpan = timeSpan;
        }

        internal override bool CanExecute()
        {
            throw new NotImplementedException();
        }
    }
}