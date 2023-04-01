// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
namespace Edanoue.HybridGraph
{
    public static class SelectorExtensions
    {
        private const string _DEFAULT_NAME = "Selector";

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static ICompositeNode Selector(this ICompositePort self)
        {
            return self.Selector(_DEFAULT_NAME);
        }

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ICompositeNode Selector(this ICompositePort self, string name)
        {
            var node = new BtCompositeNodeSelector(self, name);
            return node;
        }
    }

    /// <summary>
    /// </summary>
    internal sealed class BtCompositeNodeSelector : BtCompositeNode
    {
        internal BtCompositeNodeSelector(ICompositePort port, string name)
        {
            NodeName = name;
            port.AddNode(this);
        }

        protected override int GetNextChildIndex(int prevChild, in BtNodeResult lastResult)
        {
            var nextChild = BtSpecialChild.RETURN_TO_PARENT;

            if (prevChild == BtSpecialChild.NOT_INITIALIZED)
            {
                // Newly activated: start from the first child
                nextChild = 0;
            }
            else if (lastResult == BtNodeResult.Failed && prevChild + 1 < Children.Count)
            {
                // Last child succeeded: move to the next child
                nextChild = prevChild + 1;
            }

            return nextChild;
        }
    }
}