// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
namespace Edanoue.HybridGraph
{
    public static class BehaviourTreeExtensions
    {
        private const string _DEFAULT_NAME = "BehaviourTree";

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static ICompositeNode BehaviourTree<T>(this ICompositePort self)
            where T : BehaviourTreeBase, new()
        {
            return self.BehaviourTree<T>(_DEFAULT_NAME);
        }

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ICompositeNode BehaviourTree<T>(this ICompositePort self, string name)
            where T : BehaviourTreeBase, new()
        {
            var node = new T();
            self.AddNode(node, name);
            return node;
        }
    }
}