using System;
using System.Threading.Tasks;

namespace PhoenixLib.Scheduler
{
    public interface IScheduler
    {
        IDisposable Schedule(TimeSpan delay, Action action);
        IDisposable Schedule(TimeSpan delay, Func<Task> action);
        IDisposable Schedule(TimeSpan delay, Action<object> action);
    }
}