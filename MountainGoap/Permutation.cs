// <copyright file="Permutation.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>
namespace MountainGoap {
    /// <summary>
    /// Lightweight value type representing a single parameter permutation.
    /// Uses parallel arrays instead of a dictionary to avoid per-permutation allocations.
    /// </summary>
    internal readonly struct Permutation {
        internal readonly string[] Keys;
        internal readonly object?[] Values;
        internal readonly int Count;

        internal Permutation(string[] keys, object?[] values, int count) {
            Keys = keys;
            Values = values;
            Count = count;
        }
    }
}
