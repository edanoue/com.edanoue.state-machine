// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
namespace Edanoue.HybridGraph
{
    public abstract class BtActionNode : BtExecutableNode, IActionNode
    {
        protected BtActionNode(string name) : base(name)
        {
        }

        public IDecoratorPort With => new BtDecoratorPort(Blackboard, Decorators);

        internal sealed override int FindChildToExecute(ref BtNodeResult lastResult)
        {
            return BtSpecialChild.RETURN_TO_PARENT;
        }
    }
}