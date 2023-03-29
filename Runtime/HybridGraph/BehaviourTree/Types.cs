// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
namespace Edanoue.HybridGraph
{
    public static class BtSpecialChild
    {
        // Special value for child indices: needs to be initialized
        public const int NOT_INITIALIZED = -1;

        // Special value for child indices: return to parent node
        public const int RETURN_TO_PARENT = -2;
    }

    public enum BtNodeResult : byte
    {
        // Finished as success
        Succeeded,

        // Finished as failure
        Failed,

        // Finished as aborting
        Aborted,

        // Not finished yet
        InProgress
    }
}