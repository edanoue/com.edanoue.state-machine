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

        object IDecoratorPort.Blackboard => _blackboard;

        void IDecoratorPort.AddDecorator(BtDecoratorNode decorator)
        {
            _decorators.Add(decorator);
        }
    }
}