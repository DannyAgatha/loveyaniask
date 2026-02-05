using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet; 
using MQTTnet.Diagnostics.Logger; 
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus.Protocol;
using PhoenixLib.ServiceBus.Routing;

namespace PhoenixLib.ServiceBus.MQTT;

internal class ConsoleLogger : IMqttNetLogger
{
    private readonly object _consoleSyncRoot = new();

    public bool IsEnabled => true;

    public void Publish(MqttNetLogLevel logLevel, string source, string message, object[]? parameters, Exception? exception)
    {
        ConsoleColor foregroundColor = ConsoleColor.White;
        switch (logLevel)
        {
            case MqttNetLogLevel.Verbose:
                foregroundColor = ConsoleColor.White;
                break;

            case MqttNetLogLevel.Info:
                foregroundColor = ConsoleColor.Green;
                break;

            case MqttNetLogLevel.Warning:
                foregroundColor = ConsoleColor.DarkYellow;
                break;

            case MqttNetLogLevel.Error:
                foregroundColor = ConsoleColor.Red;
                break;
        }

        if (parameters?.Length > 0)
        {
            message = string.Format(message, parameters);
        }

        lock (_consoleSyncRoot)
        {
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(message);

            if (exception != null)
            {
                Console.WriteLine(exception);
            }
        }
    }
}

internal sealed class MqttMessagingService : IMessagingService
{
    private static readonly MethodInfo HandleMessageMethod =
        typeof(MqttMessagingService).GetMethod(nameof(HandleMessageReceived), BindingFlags.Instance | BindingFlags.NonPublic);
    
    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;
    private readonly IMessageSerializer _packetSerializer;
    private readonly IServiceProvider _provider;
    private readonly HashSet<string> _queues;
    private readonly IMessageRouter _router;
    private readonly IServiceBusInstance _serviceBusInstance;
    private TaskCompletionSource<bool> _clientConnectionReady;

    public MqttMessagingService(MqttConfiguration conf, IMessageRouter router, IServiceProvider provider, 
        IServiceBusInstance serviceBusInstance, IMessageSerializer packetSerializer)
    {
        _router = router;
        _provider = provider;
        _serviceBusInstance = serviceBusInstance;
        _packetSerializer = packetSerializer;

        var logger = new ConsoleLogger();
    
        // Usa MqttClientFactory en lugar de MqttFactory
        var factory = new MqttClientFactory(); 
        _client = factory.CreateMqttClient();

        _queues = new HashSet<string>();

        // Configura opciones estándar del cliente
        _options = new MqttClientOptionsBuilder()
            .WithClientId($"{conf.ClientName}-{_serviceBusInstance.Id}")
            .WithTcpServer(conf.Address, conf.Port)
            .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500) 
            .Build();

        // Manejo manual de eventos
        _client.ApplicationMessageReceivedAsync += async e => await Client_OnMessage(e);
        _client.ConnectedAsync += async e => await WaitReadyAsync(e);
    }
    
    private bool IsInitialized => _client.IsConnected;

    public async Task SendAsync<T>(T eventToSend) where T : IMessage
    {
        if (!IsInitialized)
        {
            await StartAsync();
        }

        Log.Debug($"[SERVICE_BUS][PUBLISHER] Sending<{typeof(T)}>...");
        IRoutingInformation routingInfo = GetRoutingInformation<T>();
        
        MqttApplicationMessage mqttMessage = _packetSerializer.ToMessage(eventToSend);
        
        mqttMessage.Topic = routingInfo.Topic;
        
        await _client.PublishAsync(mqttMessage);
        Log.Debug($"[SERVICE_BUS][PUBLISHER] Message sent to topic: {routingInfo.Topic}");
    }

    public async Task StartAsync()
    {
        if (IsInitialized)
        {
            return;
        }

        Log.Debug("[SERVICE_BUS][SUBSCRIBER] Starting...");
        _clientConnectionReady = new TaskCompletionSource<bool>(TimeSpan.FromSeconds(10));
        await _client.ConnectAsync(_options); 
        await _clientConnectionReady.Task;

        if (_clientConnectionReady.Task.IsCanceled)
        {
            throw new Exception("Could not connect to MQTT broker within 10 seconds");
        }

        Log.Debug("[SERVICE_BUS][SUBSCRIBER] Started !");
        await SubscribeRegisteredEventsAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _client.DisconnectAsync();
        _client.Dispose();
    }
    
    private async Task Client_OnMessage(MqttApplicationMessageReceivedEventArgs e)
    {
        (object message, Type objType) = _packetSerializer.FromMessage(e.ApplicationMessage);

        if (message == null || objType == null)
        {
            return;
        }

        try
        {
            MethodInfo method = HandleMessageMethod.MakeGenericMethod(objType);
            var task = (Task)method.Invoke(this, new[] { message });
            if (task != null)
            {
                await task;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Client_OnMessage<{objType.Name}", ex);
            throw;
        }
    }

    private async Task HandleMessageReceived<T>(T message)
    {
        try
        {
            IEnumerable<IMessageConsumer<T>> tmp = _provider.GetServices<IMessageConsumer<T>>();
            var cts = new CancellationTokenSource();
            foreach (IMessageConsumer<T> subscriber in tmp)
            {
                await subscriber.HandleAsync(message, cts.Token);
            }
        }
        catch (Exception e)
        {
            Log.Error($"HandleMessageReceived<{typeof(T).Name}", e);
            throw;
        }
    }

    private async Task TrySubscribeAsync(IRoutingInformation infos)
    {
        if (_queues.Contains(infos.Topic))
        {
            return;
        }

        // ✔️ Construye opciones de suscripción
        MqttClientSubscribeOptions subscribeOptions = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(f => f.WithTopic(infos.Topic))
            .Build();

        await _client.SubscribeAsync(subscribeOptions);
        _queues.Add(infos.Topic);
        Log.Debug($"[SERVICE_BUS][SUBSCRIBER] Subscribed to topic: {infos.Topic}");
    }

    private IRoutingInformation GetRoutingInformation<T>() => GetRoutingInformation(typeof(T));

    private IRoutingInformation GetRoutingInformation(Type type)
    {
        IRoutingInformation routingInfos = _router.GetRoutingInformation(type);
        if (string.IsNullOrEmpty(routingInfos.Topic))
        {
            throw new ArgumentException("routing information couldn't be retrieved");
        }

        return routingInfos;
    }

    private async Task SubscribeRegisteredEventsAsync()
    {
        IEnumerable<ISubscribedMessage> subs = _provider.GetServices<ISubscribedMessage>();

        foreach (ISubscribedMessage sub in subs)
        {
            IRoutingInformation routingInfo = GetRoutingInformation(sub.Type);
            await TrySubscribeAsync(routingInfo);
        }
    }

    private async Task WaitReadyAsync(MqttClientConnectedEventArgs e)
    {
        if (e.ConnectResult.ResultCode == MqttClientConnectResultCode.Success)
        {
            if (!_clientConnectionReady.Task.IsCompleted)
            {
                _clientConnectionReady.SetResult(true);
            }
        }
    }
}