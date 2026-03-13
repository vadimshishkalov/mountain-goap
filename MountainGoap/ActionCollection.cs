// <copyright file="ActionCollection.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// An ordered collection of <see cref="Action"/> templates that maintains an inverse index
    /// from state keys to the actions whose preconditions reference those keys. Used on
    /// <see cref="Agent.Actions"/> and passed (as <see cref="IReadOnlyActionIndex"/>) into the
    /// planning subsystem so that <see cref="Internals.ActionGraph"/> can narrow the candidate
    /// set per node without scanning every template.
    /// Supports collection-initializer syntax: <c>new ActionCollection { action1, action2 }</c>.
    /// </summary>
    public class ActionCollection : IReadOnlyActionIndex {
        private readonly List<Action> actions = new();

        // Inverse index: precondition key → actions that reference that key.
        private readonly Dictionary<string, List<Action>> index = new();

        // Actions with no static precondition keys (stateChecker-only); always candidates.
        private readonly List<Action> alwaysCandidates = new();

        /// <inheritdoc/>
        public int Count => actions.Count;

        /// <summary>Gets the action at the given index.</summary>
        public Action this[int i] => actions[i];

        /// <summary>
        /// Adds an action template to the collection and registers it in the precondition index.
        /// </summary>
        public void Add(Action action) {
            actions.Add(action);
            bool indexed = false;
            foreach (var key in action.PreconditionKeys) {
                if (!index.TryGetValue(key, out var list)) {
                    list = new List<Action>();
                    index[key] = list;
                }
                list.Add(action);
                indexed = true;
            }
            if (!indexed) alwaysCandidates.Add(action);
        }

        /// <summary>
        /// Removes an action template from the collection and from the precondition index.
        /// </summary>
        public bool Remove(Action action) {
            if (!actions.Remove(action)) return false;
            bool wasIndexed = false;
            foreach (var key in action.PreconditionKeys) {
                if (index.TryGetValue(key, out var list)) {
                    list.Remove(action);
                    if (list.Count == 0) index.Remove(key);
                }
                wasIndexed = true;
            }
            if (!wasIndexed) alwaysCandidates.Remove(action);
            return true;
        }

        /// <inheritdoc/>
        public void GetCandidates(IEnumerable<string> keys, HashSet<Action> result) {
            foreach (var key in keys) {
                if (index.TryGetValue(key, out var list)) {
                    foreach (var action in list) result.Add(action);
                }
            }
            foreach (var action in alwaysCandidates) result.Add(action);
        }

        /// <inheritdoc/>
        public IEnumerator<Action> GetEnumerator() => actions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => actions.GetEnumerator();
    }
}
