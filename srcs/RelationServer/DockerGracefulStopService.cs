using System;
using System.Runtime.Loader;
using System.Threading;
using PhoenixLib.Logging;

namespace RelationServer
{
    public class DockerGracefulStopService : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ManualResetEventSlim _stoppedEvent;

        public DockerGracefulStopService()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _stoppedEvent = new ManualResetEventSlim();

            // SIGINT
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Log.Error("[GRACEFUL_SHUTDOWN] SIGINT received", new Exception("[GRACEFUL_SHUTDOWN] SIGINT received"));
                GracefulStop(_cancellationTokenSource, _stoppedEvent);
            };

            // SIGTERM
            AssemblyLoadContext.Default.Unloading += context =>
            {
                Log.Error("[GRACEFUL_SHUTDOWN] SIGTERM received", new Exception("[GRACEFUL_SHUTDOWN] SIGTERM received"));
                GracefulStop(_cancellationTokenSource, _stoppedEvent);
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Log.Error("UnhandledException", args.ExceptionObject as Exception);
                GracefulStop(_cancellationTokenSource, _stoppedEvent);
            };
        }

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        public void Dispose()
        {
            _stoppedEvent.Set();
        }

        private static void GracefulStop(CancellationTokenSource cancellationTokenSource, ManualResetEventSlim stoppedEvent)
        {
            Log.Info("DockerGracefulStopService Stopping service");
            cancellationTokenSource.Cancel();
            stoppedEvent.Wait();
            Log.Info("DockerGracefulStopService Stop finished");
        }
    }
}