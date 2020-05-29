using Aix.SocketCore.Foundation;
using Aix.SocketCore.Utils;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.SocketCore.EventLoop
{
    public class SingleThreadEventExecutor : IEventExecutor
    {
        ConcurrentQueue<IRunnable> _taskQueue;
        //readonly ManualResetEventSlim _emptyEvent = new ManualResetEventSlim(false, 1);
        //int waitingConsumers = 0;
        volatile bool _isStart = false;

        public event Func<Exception, Task> OnException;

        protected readonly PriorityQueue<IScheduledRunnable> ScheduledTaskQueue = new PriorityQueue<IScheduledRunnable>();

        TaskScheduler _scheduler;
        MyThread _myThread;
        public SingleThreadEventExecutor()
        {
            //_taskQueue = new BlockingCollection<IRunnable>(new ConcurrentQueue<IRunnable>());
            _taskQueue = new ConcurrentQueue<IRunnable>(); 
            this._scheduler = new ExecutorTaskScheduler(this);
            _myThread = new MyThread(loop);
        }

        public void Start()
        {
            if (_isStart) return;
            _isStart = true;
            _myThread.Start();
        }
        public void loop()
        {
            Task.Factory.StartNew(async () =>
            {
                while (_isStart)
                {
                    try
                    {
                        RunAllTasks();
                    }
                    catch (Exception ex)
                    {
                        await handlerException(ex);
                    }
                }
            }, CancellationToken.None, TaskCreationOptions.None, _scheduler);
        }

        private void RunAllTasks()
        {
            FetchFromScheduledTaskQueue();
            IRunnable task = PollTask();
            if (task == null) return;
            int runTasks = 0;
            while (true)
            {
                runTasks++;
                task.Run();

                if (runTasks >= 64)
                {
                    break;
                }

                task = PollTask();
                if (task == null)
                {
                    break;
                }
            }
        }

        private IRunnable PollTask()
        {
            IRunnable task = null;
            if (!_taskQueue.TryDequeue(out task))
            {
                lock (this._taskQueue)
                {
                    if (!_taskQueue.TryDequeue(out task))
                    {
                        IScheduledRunnable nextScheduledTask = this.ScheduledTaskQueue.Peek();
                        if (nextScheduledTask != null)
                        {
                            var tempDelay = nextScheduledTask.TimeStamp - DateUtils.GetTimeStamp();
                            if (tempDelay > 0)
                            {

                                Monitor.Wait(_taskQueue, (int)tempDelay);
                            }
                        }
                        else
                        {
                            Monitor.Wait(_taskQueue);  // 交出权，等待通知唤醒(Monitor.Pulse(queue))
                        }
                    }
                }
            }
            return task;
        }

        bool FetchFromScheduledTaskQueue()
        {
            IScheduledRunnable scheduledTask = this.PollScheduledTask();
            while (scheduledTask != null)
            {
                //this._taskQueue.Enqueue(scheduledTask);
                Execute(scheduledTask);
                scheduledTask = this.PollScheduledTask();
            }
            return true;
        }

        protected IScheduledRunnable PollScheduledTask()
        {
            IScheduledRunnable scheduledTask = this.ScheduledTaskQueue.Peek();
            if (scheduledTask == null)
            {
                return null;
            }

            if (scheduledTask.TimeStamp <= DateUtils.GetTimeStamp())
            {
                //Console.WriteLine($"延迟到期:{DateTime.Now.ToString("HH:mm:ss")}");
                this.ScheduledTaskQueue.Dequeue();
                return scheduledTask;
            }
            return null;
        }

        private async Task handlerException(Exception ex)
        {
            if (OnException != null)
            {
                await OnException(ex);
            }
        }

        #region IEventExecutor
        public bool InEventLoop => this.IsInEventLoop(MyThread.CurrentThread);

        private bool IsInEventLoop(MyThread t)
        {
            return this._myThread == t;
        }
        public void Execute(IRunnable task)
        {
            lock (_taskQueue)
            {
                _taskQueue.Enqueue(task);
                // if (waitingConsumers > 0) //唤醒等待新元素的消费者
                {
                    Monitor.Pulse(_taskQueue);
                }
            }
        }

        public void Execute(Action action)
        {
            Execute(new ActionRunnable(action));
        }

        //public void Execute(Func<Task> action)
        //{
        //    Execute(new FuncRunnable(action));
        //}

        private void Schedule(IScheduledRunnable task)
        {
            this.Execute(() =>
            {
                this.ScheduledTaskQueue.Enqueue(task);
            });
        }
        public void Schedule(IRunnable action, TimeSpan delay)
        {
            Schedule(new ScheduledRunnable(action, DateUtils.GetTimeStamp(DateTime.Now.Add(delay))));
        }

        public void Schedule(Action action, TimeSpan delay)
        {
            Schedule(new ActionRunnable(action), delay);
        }

        //public void Schedule(Func<Task> action, TimeSpan delay)
        //{
        //    Schedule(new FuncRunnable(action), delay);
        //}

        public void Dispose()
        {
            this.Stop();
        }

        public void Stop()
        {
            if (this._isStart == false) return;
            lock (this)
            {
                if (this._isStart)
                {
                    this._isStart = false;
                }
            }

        }
        #endregion
    }
}
