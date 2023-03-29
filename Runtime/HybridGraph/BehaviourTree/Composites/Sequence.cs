// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
namespace Edanoue.HybridGraph
{
    public static class SequenceExtensions
    {
        public static ICompositeNode Sequence(this ICompositePort self)
        {
            return self.Sequence("Sequence");
        }

        public static ICompositeNode Sequence(this ICompositePort self, string name)
        {
            var node = new BtCompositeNodeSequence(name);
            self.AddNode(node);
            return node;
        }
    }

    /// <summary>
    /// </summary>
    internal sealed class BtCompositeNodeSequence : BtCompositeNode
    {
        internal BtCompositeNodeSequence(string name) : base(name)
        {
        }

        protected override int GetNextChildIndex(int prevChild, in BtNodeResult lastResult)
        {
            var nextChild = BtSpecialChild.RETURN_TO_PARENT;

            if (prevChild == BtSpecialChild.NOT_INITIALIZED)
            {
                // Newly activated: start from the first child
                nextChild = 0;
            }
            else if (lastResult == BtNodeResult.Succeeded && prevChild + 1 < Children.Count)
            {
                // Last child succeeded: move to the next child
                nextChild = prevChild + 1;
            }

            return nextChild;
        }
    }
}