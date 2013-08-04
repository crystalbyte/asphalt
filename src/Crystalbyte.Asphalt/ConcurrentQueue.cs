#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace Crystalbyte.Asphalt {
    public sealed class ConcurrentQueue {
        private readonly object _locker = new object();
        private readonly Task[] _workers;
        private readonly Queue<Task> _taskQueue = new Queue<Task>();

        public ConcurrentQueue(int workerCount) {
            _workers = new Task[workerCount];

            // Create and start a separate thread for each worker
            for (var i = 0; i < workerCount; i++)
                (_workers[i] = new Task(Consume)).Start();
        }

        public void Shutdown(bool waitForWorkers) {
            // Enqueue one null item per worker to make each exit.
            foreach (var worker in _workers)
                Enqueue(null);

            // Wait for workers to finish
            if (waitForWorkers)
                Task.WaitAll(_workers);
        }

        public Task<T> Enqueue<T>(Func<T> func) {
            var task = new Task<T>(func, TaskCreationOptions.AttachedToParent);
            Debug.WriteLine("Task with hash {0} has been queued.", task.GetHashCode());
            lock (_locker) {
                _taskQueue.Enqueue(task); // We must pulse because we're
                Monitor.Pulse(_locker); // changing a blocking condition.
            }
            return task;
        }

        public Task Enqueue(Action action) {
            var task = new Task(action);
            Debug.WriteLine("Task with hash {0} has been queued.", task.GetHashCode());
            lock (_locker) {
                _taskQueue.Enqueue(task); // We must pulse because we're
                Monitor.Pulse(_locker); // changing a blocking condition.
            }
            return task;
        }

        private void Consume() {
            while (true) // Keep consuming until
            {
                // told otherwise.
                Task task;
                lock (_locker) {
                    while (_taskQueue.Count == 0)
                        Monitor.Wait(_locker);
                    task = _taskQueue.Dequeue();
                }
                if (task == null)
                    return; // This signals our exit.
#if DEBUG
  task.ContinueWith(t => Debug.WriteLine("Task with hash {0} has finished.", t.GetHashCode()));
#endif
                task.RunSynchronously(); // Execute item.
            }
        }
    }
}
