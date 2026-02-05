using System;
using System.Threading;
using System.Threading.Tasks;

namespace PhoenixLib.Scheduler.ReactiveX
{
    public class FuncTaskWorkerDisposable : IDisposable
    {
        private readonly Func<Task> _action;
        private readonly CancellationTokenSource _cts;
        private readonly TimeSpan _interval;

        public FuncTaskWorkerDisposable(TimeSpan interval, Func<Task> action)
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
                await _action();
                await Task.Delay(_interval);
            }
        }
    }
}