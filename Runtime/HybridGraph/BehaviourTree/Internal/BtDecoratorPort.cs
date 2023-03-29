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

        void IDecoratorPort.AddDecorator(BtDecoratorNode decorator)
        {
            decorator.SetBlackboard(_blackboard);
            _decorators.Add(decorator);
        }
    }
}