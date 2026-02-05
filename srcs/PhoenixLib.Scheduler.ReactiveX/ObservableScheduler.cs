using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using PhoenixLib.Logging;

namespace PhoenixLib.Scheduler.ReactiveX
{
    public class ObservableScheduler : IScheduler
    {
        public IDisposable Schedule(TimeSpan delay, Action tmp)
        {
            return Observable.Timer(delay).Subscribe(s =>
            {
                try
                {
                    tmp();
                }
                catch (Exception e)
                {
                    Log.Error("[SCHEDULER]", e);
                    throw;
                }
            });
        }

        public IDisposable Schedule(TimeSpan delay, Func<Task> tmp)
        {
            return Observable.Timer(delay).Subscribe(async s =>
            {
                try
                {
                    await tmp();
                }
                catch (Exception e)
                {
                    Log.Error("[SCHEDULER]", e);
                    throw;
                }
            });
        }

        public IDisposable Schedule(TimeSpan delay, Action<object> action)
        {
            return Observable.Timer(delay).Subscribe(async s =>
            {
                try
                {
                    action(null);
                }
                catch (Exception e)
                {
                    Log.Error("[SCHEDULER]", e);
                    throw;
                }
            });
        }
    }
}