// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
namespace Edanoue.HybridGraph
{
    /// <summary>
    /// BehaviourTree のノードの基底クラス
    /// </summary>
    public abstract class BtNode
    {
        /// <summary>
        /// Gets the blackboard.
        /// </summary>
        protected internal object Blackboard { get; internal set; } = null!;

        /// <summary>
        /// Gets the name of the node for debugging.
        /// </summary>
        protected internal string NodeName { get; internal set; } = string.Empty;
    }
}