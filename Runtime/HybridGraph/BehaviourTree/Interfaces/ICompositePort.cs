﻿// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
namespace Edanoue.HybridGraph
{
    public interface ICompositePort
    {
        internal void AddNodeAndSetBlackboard(BtExecutableNode node);
    }
}