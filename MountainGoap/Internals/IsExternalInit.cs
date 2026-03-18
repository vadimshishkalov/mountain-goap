// <copyright file="IsExternalInit.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

// Polyfill required to use `init`-only properties when targeting netstandard2.1 with C# 9+.
namespace System.Runtime.CompilerServices {
    internal static class IsExternalInit {
    }
}
