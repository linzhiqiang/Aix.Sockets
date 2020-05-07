using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.SocketCore.EventLoop
{
    /// <summary>
    /// task任务都转到IEventExecutor中执行，保证在一个线程中执行
    /// </summary>
    public sealed class ExecutorTaskScheduler : TaskScheduler
    {
        readonly IEventExecutor executor;
        bool started;

        public ExecutorTaskScheduler(IEventExecutor executor)
        {
            this.executor = executor;
        }

        protected override void QueueTask(Task task)
        {
            if (this.started)
            {
                this.executor.Execute(new TaskQueueNode(this, task));
            }
            else
            {
                // hack: enables this executor to be seen as default on Executor's worker thread.
                // This is a special case for SingleThreadEventExecutor.Loop initiated task.
                this.started = true;
                this.TryExecuteTask(task);
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (taskWasPreviouslyQueued || !this.executor.InEventLoop)
            {
                return false;
            }

            return this.TryExecuteTask(task);
        }

        protected override IEnumerable<Task> GetScheduledTasks() => null;

        protected override bool TryDequeue(Task task) => false;

        sealed class TaskQueueNode : IRunnable
        {
            readonly ExecutorTaskScheduler scheduler;
            readonly Task task;

            public TaskQueueNode(ExecutorTaskScheduler scheduler, Task task)
            {
                this.scheduler = scheduler;
                this.task = task;
            }

            public void Run() => this.scheduler.TryExecuteTask(this.task);
        }
    }
}
