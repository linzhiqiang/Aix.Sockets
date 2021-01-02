using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.SocketCore.EventLoop
{
    public interface IRunnable
    {
        void Run();
    }

    public class ActionRunnable : IRunnable
    {
        Action _action;
        public ActionRunnable(Action action)
        {
            _action = action;
        }

        public void Run()
        {
            _action();
        }
    }
}
