// NosEmu
// 


using System.Threading;
using System.Threading.Tasks;

namespace PhoenixLib.Events
{
    public class ConvertedEventForwarder<TSource, TDest> : IAsyncEventProcessor<TSource>
    where TSource : IAsyncEvent
    where TDest : IAsyncEvent
    {
        private readonly IConverter<TSource, TDest> _converter;
        private readonly IAsyncEventPipeline _eventPipeline;

        public ConvertedEventForwarder(IConverter<TSource, TDest> converter, IAsyncEventPipeline eventPipeline)
        {
            _converter = converter;
            _eventPipeline = eventPipeline;
        }

        public async Task HandleAsync(TSource e, CancellationToken cancellation)
        {
            TDest dest = _converter.Convert(e);
            await _eventPipeline.ProcessEventAsync(dest, cancellation);
        }
    }
}