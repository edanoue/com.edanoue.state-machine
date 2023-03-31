// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System.Collections.Generic;

namespace Edanoue.HybridGraph
{
    internal sealed class BtDecoratorPort : IDecoratorPort
    {
        private readonly object                _blackboard;
        private readonly List<BtDecoratorNode> _decorators;

        public BtDecoratorPort(object blackboard, List<BtDecoratorNode> decorators)
        {
            _blackboard = blackboard;
            _decorators = decorators;
        }

        void IDecoratorPort.AddDecorator(BtDecoratorNode decorator, string nodeName)
        {
            decorator.Blackboard = _blackboard;
            decorator.NodeName = nodeName;
            _decorators.Add(decorator);
        }
    }
}