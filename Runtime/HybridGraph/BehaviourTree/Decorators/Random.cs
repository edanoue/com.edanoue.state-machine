// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace Edanoue.HybridGraph
{
    public static class RandomExtensions
    {
        private const string _DEFAULT_NODE_NAME = "Random";
        private const int    _DEFAULT_SEED      = 0;

        /// <summary>
        /// <para>[0, 1] の範囲で指定した確率でノードを実行する Decorator</para>
        /// <para>0 なら実行されず, 0.5 なら 50%, 1 なら 100% の確率で実行されます.</para>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="probability">[0, 1]</param>
        /// <returns></returns>
        public static BtDecoratorNodeRandom Random(this IDecoratorPort self, float probability)
        {
            return self.Random(probability, _DEFAULT_SEED, _DEFAULT_NODE_NAME);
        }

        public static BtDecoratorNodeRandom Random(this IDecoratorPort self, float probability, string name)
        {
            return self.Random(probability, _DEFAULT_SEED, name);
        }

        public static BtDecoratorNodeRandom Random(this IDecoratorPort self, float probability, int seed)
        {
            return self.Random(probability, seed, _DEFAULT_NODE_NAME);
        }

        public static BtDecoratorNodeRandom Random(this IDecoratorPort self, float probability, int seed, string name)
        {
            var node = new BtDecoratorNodeRandom(self, name, probability, seed);
            return node;
        }
    }

    public sealed class BtDecoratorNodeRandom : BtDecoratorNode
    {
        private float  _probability;
        private Random _random;

        public BtDecoratorNodeRandom(IDecoratorPort port, string name, float probability, int seed) : base(port, name)
        {
            _random = new Random(seed);
            Probability = probability;
        }

        public float Probability
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => 1f - _probability;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _probability = 1.0f - Math.Clamp(value, 0f, 1f);
        }

        public void UpdateSeed(int seed)
        {
            _random = new Random(seed);
        }

        internal override bool CanEnter()
        {
            var r = _random.NextDouble();
            return r > _probability;
        }
    }
}