using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.SocketCore.Foundation
{
    public class MyThread
    {
        static int maxThreadId;

        [ThreadStatic]
        public static MyThread _currentThread;
        public static MyThread CurrentThread => _currentThread ?? (_currentThread = new MyThread());

        public int Id { get; }
        Action _action;
        Action<object> _actionWithParameter;
        object _startupParameter;
        bool _open = false;
        public MyThread()
        {
            this.Id = GetNewThreadId();
        }
        public MyThread(Action action)
        {
            _action = action;
            this.Id = GetNewThreadId();
        }

        public MyThread(Action<object> action)
        {
            _actionWithParameter = action;
            this.Id = GetNewThreadId();
        }
        public void Start()
        {
            if (_open) return;
            _open = true;
            CreateLongRunningTask();
        }
        public void Start(object parameter)
        {
            this._startupParameter = parameter;
            this.Start();
        }
        void CreateLongRunningTask()
        {
            Task.Factory.StartNew(() =>
            {
                _currentThread = this;
                if (_action != null)
                {
                    _action();
                }
                else if (_actionWithParameter != null)
                {
                    _actionWithParameter(_startupParameter);
                }
            }, TaskCreationOptions.LongRunning);
        }

        static int GetNewThreadId() => Interlocked.Increment(ref maxThreadId);
        public static void Sleep(int millisecondsTimeout) => Task.Delay(millisecondsTimeout).Wait();
    }
}
