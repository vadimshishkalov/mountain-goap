// <copyright file="PlannerWorkerPool.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System;
    using System.Threading.Channels;
    using System.Threading.Tasks;

    /// <summary>
    /// A fixed-size pool of dedicated worker threads that execute agent planning jobs.
    /// Agents enqueue themselves via <see cref="Agent.PlanAsync"/> or the default <see cref="StepMode"/>;
    /// workers call <see cref="Agent.RunPlan"/> in a loop. Use <see cref="Default"/> for a
    /// process-lifetime shared pool used by all worlds.
    /// </summary>
    public sealed class PlannerWorkerPool : IDisposable {
        /// <summary>Gets the default shared pool, sized to <see cref="Environment.ProcessorCount"/> workers.</summary>
        public static PlannerWorkerPool Default { get; } = new PlannerWorkerPool();

        private readonly Channel<Agent> _channel;
        private readonly Task[] _workers;
        private volatile bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlannerWorkerPool"/> class.
        /// </summary>
        /// <param name="workerCount">
        /// Number of dedicated worker threads. Values &lt;= 0 default to <see cref="Environment.ProcessorCount"/>.
        /// </param>
        public PlannerWorkerPool(int workerCount = 0) {
            int n = workerCount > 0 ? workerCount : Environment.ProcessorCount;
            _channel = Channel.CreateUnbounded<Agent>(
                new UnboundedChannelOptions { SingleReader = false, SingleWriter = false });
            _workers = new Task[n];
            for (int i = 0; i < n; i++)
                _workers[i] = Task.Factory.StartNew(WorkerLoop, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Enqueues an agent for asynchronous planning.
        /// </summary>
        /// <param name="agent">Agent to plan for.</param>
        /// <returns><c>true</c> if enqueued successfully; <c>false</c> if the pool has been disposed.</returns>
        internal bool Enqueue(Agent agent) {
            if (_disposed) return false;
            return _channel.Writer.TryWrite(agent);
        }

        /// <summary>
        /// Signals workers to stop after draining in-flight jobs, then blocks until all workers exit.
        /// </summary>
        public void Dispose() {
            if (_disposed) return;
            _disposed = true;
            _channel.Writer.Complete();
            Task.WhenAll(_workers).GetAwaiter().GetResult();
        }

        private async Task WorkerLoop() {
            await foreach (var agent in _channel.Reader.ReadAllAsync())
                agent.RunPlan();
        }
    }
}
