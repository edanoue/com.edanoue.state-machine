// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Edanoue.HybridGraph
{
    public abstract class BtExecutableNode : BtNode
    {
        internal readonly List<BtDecoratorNode> Decorators = new();


        protected BtExecutableNode(string name) : base(name)
        {
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        internal abstract int FindChildToExecute(ref BtNodeResult lastResult);

        internal abstract UniTask<BtNodeResult> ExecuteAsync(CancellationToken token);
    }
}