// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
namespace Edanoue.HybridGraph
{
    public abstract class BtNode
    {
        internal BtNode()
        {
        }

        protected internal object Blackboard { get; internal set; } = null!;

        protected internal string NodeName { get; internal set; } = string.Empty;
    }
}