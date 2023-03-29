// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.HybridGraph
{
    public static class RandomExtensions
    {
        private const string _DEFAULT_NODE_NAME = "Random";
        private const int    _DEFAULT_SEED      = 0;

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="probability">[0, 1]</param>
        /// <returns></returns>
        public static IDecoratorNode Random(this IDecoratorPort self, float probability)
        {
            return self.Random(probability, _DEFAULT_SEED, _DEFAULT_NODE_NAME);
        }

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="probability">[0, 1]</param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IDecoratorNode Random(this IDecoratorPort self, float probability, string name)
        {
            return self.Random(probability, _DEFAULT_SEED, name);
        }

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="probability">[0, 1]</param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static IDecoratorNode Random(this IDecoratorPort self, float probability, int seed)
        {
            return self.Random(probability, seed, _DEFAULT_NODE_NAME);
        }

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="probability">[0, 1]</param>
        /// <param name="seed"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IDecoratorNode Random(this IDecoratorPort self, float probability, int seed, string name)
        {
            var node = new BtDecoratorNodeRandom(probability, seed, name);
            self.AddDecorator(node);
            return node;
        }
    }

    internal sealed class BtDecoratorNodeRandom : BtDecoratorNode
    {
        private readonly double _probability;
        private readonly Random _random;

        public BtDecoratorNodeRandom(float probability, int seed, string name) : base(name)
        {
            _random = new Random(seed);
            _probability = 1.0d - Math.Clamp(probability, 0, 1);
        }

        internal override bool CanExecute()
        {
            var r = _random.NextDouble();
            return r > _probability;
        }
    }
}