// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System.Collections.Generic;

namespace Edanoue.HybridGraph
{
    internal sealed class BtCompositePort : ICompositePort
    {
        private readonly object                 _blackboard;
        private readonly List<BtExecutableNode> _children;

        public BtCompositePort(object blackboard, List<BtExecutableNode> children)
        {
            _blackboard = blackboard;
            _children = children;
        }

        void ICompositePort.AddNode(BtExecutableNode node)
        {
            node.Blackboard = _blackboard;
            _children.Add(node);
        }
    }
}