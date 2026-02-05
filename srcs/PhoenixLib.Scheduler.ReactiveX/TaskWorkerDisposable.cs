using System;
using System.Threading;
using System.Threading.Tasks;

namespace PhoenixLib.Scheduler.ReactiveX
{
    public class TaskWorkerDisposable : IDisposable
    {
        private readonly Action _action;
        private readonly CancellationTokenSource _cts;
        private readonly TimeSpan _interval;

        public TaskWorkerDisposable(TimeSpan interval, Action action)
        {
            _interval = interval;
            _action = action;
            _cts = new CancellationTokenSource();
            Work();
        }

        public void Dispose()
        {
            _cts.Cancel();
        }

        private async Task Work()
        {
            while (!_cts.IsCancellationRequested)
            {
                _action();
                await Task.Delay(_interval);
            }
        }
    }
}