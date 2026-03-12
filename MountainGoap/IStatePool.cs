// <copyright file="IStatePool.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    /// <summary>
    /// Internal contract for planning state pools.
    /// </summary>
    internal interface IStatePool {
        PlanningNodeState RentNodeState(PlanningBaseState baseState);
        void ReturnNodeState(PlanningNodeState state);
        PlanningBaseState RentBaseState(System.Collections.Concurrent.ConcurrentDictionary<string, object?> source);
        void ReturnBaseState(PlanningBaseState state);
    }
}
