using System;
using System.Threading.Tasks;

namespace PhoenixLib.Scheduler
{
    public interface ICron
    {
        IDisposable Schedule(TimeSpan interval, Action callback);

        IDisposable Schedule(TimeSpan interval, Func<Task> callback);
    }
}