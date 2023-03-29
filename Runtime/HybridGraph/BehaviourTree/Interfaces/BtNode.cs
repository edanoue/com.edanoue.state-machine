// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
namespace Edanoue.HybridGraph
{
    public abstract class BtNode
    {
        private readonly string _name;
        protected        object Blackboard = null!;

        internal BtNode(string name)
        {
            _name = name;
        }

        internal void SetBlackboard(object blackboard)
        {
            Blackboard = blackboard;
        }
    }
}