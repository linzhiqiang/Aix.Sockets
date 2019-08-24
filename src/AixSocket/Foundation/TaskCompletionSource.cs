using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AixSocket.Foundation
{
    public sealed class TaskCompletionSource : TaskCompletionSource<int>
    {
        public static readonly TaskCompletionSource Void = CreateVoidTcs();

        public TaskCompletionSource(object state)
            : base(state)
        {
        }

        public TaskCompletionSource()
        {
        }

        public bool TryComplete() => this.TrySetResult(0);

        public void Complete() => this.SetResult(0);

        // todo: support cancellation token where used
        public bool SetUncancellable() => true;

        public override string ToString() => "TaskCompletionSource[status: " + this.Task.Status.ToString() + "]";

        static TaskCompletionSource CreateVoidTcs()
        {
            var tcs = new TaskCompletionSource();
            tcs.TryComplete();
            return tcs;
        }
    }
}
