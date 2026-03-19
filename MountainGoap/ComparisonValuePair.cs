// <copyright file="ComparisonValuePair.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    /// <summary>
    /// List of operators that can be used for comparison.
    /// </summary>
    public class ComparisonValuePair {
        /// <summary>
        /// Gets or sets the value to be compared against.
        /// </summary>
        public object? Value { get; init; } = null;

        /// <summary>
        /// Gets or initializes the operator to be used for comparison.
        /// </summary>
        public ComparisonOperator Operator { get; init; } = ComparisonOperator.Undefined;
    }
}