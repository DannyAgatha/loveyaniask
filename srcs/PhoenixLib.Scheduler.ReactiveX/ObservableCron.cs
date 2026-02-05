using System;
using System.Threading.Tasks;

namespace PhoenixLib.Scheduler.ReactiveX
{
    public class ObservableCron : ICron
    {
        public IDisposable Schedule(TimeSpan interval, Action callback) => new TaskWorkerDisposable(interval, callback);

        public IDisposable Schedule(TimeSpan interval, Func<Task> callback) => new FuncTaskWorkerDisposable(interval, callback);
    }
}