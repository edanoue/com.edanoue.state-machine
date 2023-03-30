// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
namespace Edanoue.HybridGraph
{
    public abstract class BtActionNode : BtExecutableNode, IActionNode
    {
        public IDecoratorPort With => new BtDecoratorPort(Blackboard, Decorators);

        internal sealed override int FindChildToExecute(ref BtNodeResult lastResult)
        {
            return BtSpecialChild.RETURN_TO_PARENT;
        }
    }
}