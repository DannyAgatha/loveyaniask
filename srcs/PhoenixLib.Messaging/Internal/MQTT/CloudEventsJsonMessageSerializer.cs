using CloudNative.CloudEvents;
using MQTTnet;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus.Routing;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using CloudNative.CloudEvents.Core;
using CloudNative.CloudEvents.NewtonsoftJson;
using MQTTnet.Packets;

namespace PhoenixLib.ServiceBus.Protocol;

public static class MqttExtensions
{
    /// <summary>
    /// Converts this MQTT message into a CloudEvent object.
    /// </summary>
    /// <param name="message">The MQTT message to convert. Must not be null.</param>
    /// <param name="formatter">The event formatter to use to parse the CloudEvent. Must not be null.</param>
    /// <param name="extensionAttributes">The extension attributes to use when parsing the CloudEvent. May be null.</param>
    /// <returns>A reference to a validated CloudEvent instance.</returns>
    public static CloudEvent ToCloudEvent(this MqttApplicationMessage message,
        CloudEventFormatter formatter, params CloudEventAttribute[]? extensionAttributes) =>
        ToCloudEvent(message, formatter, (IEnumerable<CloudEventAttribute>?)extensionAttributes);

    /// <summary>
    /// Converts this MQTT message into a CloudEvent object.
    /// </summary>
    /// <param name="message">The MQTT message to convert. Must not be null.</param>
    /// <param name="formatter">The event formatter to use to parse the CloudEvent. Must not be null.</param>
    /// <param name="extensionAttributes">The extension attributes to use when parsing the CloudEvent. May be null.</param>
    /// <returns>A reference to a validated CloudEvent instance.</returns>
    public static CloudEvent ToCloudEvent(this MqttApplicationMessage message,
        CloudEventFormatter formatter, IEnumerable<CloudEventAttribute>? extensionAttributes)
    {
        Validation.CheckNotNull(formatter, nameof(formatter));
        Validation.CheckNotNull(message, nameof(message));
        
        byte[] payloadBytes = message.Payload.ToArray();
        
        string? contentTypeString = DetermineContentType(message);
        
        ContentType? contentType = contentTypeString != null ? new ContentType(contentTypeString) : null;
        
        return formatter.DecodeStructuredModeMessage(new ReadOnlyMemory<byte>(payloadBytes), contentType, extensionAttributes);
    }

    /// <summary>
    /// Determines the appropriate contentType for the MQTT message.
    /// </summary>
    /// <param name="message">The MQTT message.</param>
    /// <returns>The contentType, or null if it cannot be determined.</returns>
    private static string? DetermineContentType(MqttApplicationMessage message)
    {
        MqttUserProperty contentTypeProperty = message.UserProperties?.FirstOrDefault(p => p.Name.Equals("ContentType", StringComparison.OrdinalIgnoreCase));
        if (contentTypeProperty != null && !string.IsNullOrEmpty(contentTypeProperty.Value))
        {
            return contentTypeProperty.Value;
        }
        return IsJsonPayload(message.Payload.ToArray()) ? "application/json" : "application/octet-stream";
    }

    /// <summary>
    /// Checks if the payload is JSON.
    /// </summary>
    /// <param name="payload">The message payload.</param>
    /// <returns>True if the payload is JSON; otherwise, false.</returns>
    private static bool IsJsonPayload(byte[]? payload)
    {
        if (payload == null || payload.Length == 0)
        {
            return false;
        }

        try
        {
            string payloadString = System.Text.Encoding.UTF8.GetString(payload);
            JsonConvert.DeserializeObject(payloadString);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Converts a CloudEvent to <see cref="MqttApplicationMessage"/>.
    /// </summary>
    /// <param name="cloudEvent">The CloudEvent to convert. Must not be null, and must be a valid CloudEvent.</param>
    /// <param name="contentMode">Content mode. Supports both structured and binary modes.</param>
    /// <param name="formatter">The formatter to use within the conversion. Must not be null.</param>
    /// <param name="topic">The MQTT topic for the message. May be null.</param>
    /// <returns>The MQTT application message.</returns>
    public static MqttApplicationMessage ToMqttApplicationMessage(this CloudEvent cloudEvent, ContentMode contentMode, CloudEventFormatter formatter, string? topic)
    {
        ArgumentNullException.ThrowIfNull(cloudEvent, nameof(cloudEvent));
        ArgumentNullException.ThrowIfNull(formatter, nameof(formatter));

        byte[] payloadBytes;
        string contentType;

        switch (contentMode)
        {
            case ContentMode.Structured:
                ReadOnlyMemory<byte> structuredMessage = formatter.EncodeStructuredModeMessage(cloudEvent, out ContentType contentTypeHeader);
                payloadBytes = structuredMessage.ToArray(); 
                contentType = contentTypeHeader.ToString();
                break;

            case ContentMode.Binary:
                payloadBytes = formatter.EncodeBinaryModeEventData(cloudEvent).ToArray();
                contentType = formatter.GetOrInferDataContentType(cloudEvent);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(contentMode), $"Unsupported content mode: {contentMode}");
        }
        
        return new MqttApplicationMessage
        {
            Topic = topic,
            PayloadSegment = new ArraySegment<byte>(payloadBytes),
            ContentType = contentType
        };
    }
}

internal class CloudEventsJsonMessageSerializer : IMessageSerializer
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    private readonly IServiceBusInstance _busInstance;
    private readonly IMessageRouter _messageRouter;

    public CloudEventsJsonMessageSerializer(IServiceBusInstance busInstance, IMessageRouter messageRouter)
    {
        _busInstance = busInstance;
        _messageRouter = messageRouter;
    }

    public MqttApplicationMessage ToMessage<T>(T packet) where T : IMessage
    {
        IRoutingInformation routingInfos = _messageRouter.GetRoutingInformation<T>();
        MqttApplicationMessage tmp = Create(routingInfos, packet, _busInstance.Id.ToString());
        return tmp;
    }

    public (object obj, Type objType) FromMessage(MqttApplicationMessage message)
    {
        var container = message.ToCloudEvent(new JsonEventFormatter());
        if (container.Source != null && container.Source.OriginalString.Contains(_busInstance.Id.ToString()))
        {
            Log.Debug("[SERVICE_BUS][SUBSCRIBER] Message received from myself");
            // should take a look to broker's ACL
            // https://stackoverflow.com/questions/59565487/mqtt-message-subscription-all-except-me
            return (null, null);
        }

        if (container.Data == null)
        {
            Log.Debug("container.Data is null");
            return (null, null);
        }

        string eventContent = container.Data.ToString();

        IRoutingInformation routingInformation = _messageRouter.GetRoutingInformation(container.Type);

        Log.Debug($"[SERVICE_BUS][SUBSCRIBER] Message received from sender : {container.Source} topic {message.Topic}");
        object packet = JsonConvert.DeserializeObject(eventContent, routingInformation.ObjectType);
        return (packet, routingInformation.ObjectType);
    }

    private static MqttApplicationMessage Create(IRoutingInformation routingInformation, object content, string source)
    {
        // SpecVersion type source id time extension
        var cloud = new CloudEvent(CloudEventsSpecVersion.V1_0)
        {
            Type = routingInformation.EventType,
            Source = new Uri("publisher:" + source),
            Id = Guid.NewGuid().ToString(),
            Time = DateTime.UtcNow,
            DataContentType = new ContentType(MediaTypeNames.Application.Json).MediaType,
            //DataContentType = new ContentType(MediaTypeNames.Application.Json),
            Data = JsonConvert.SerializeObject(content, Settings)
        };
        return cloud.ToMqttApplicationMessage(ContentMode.Structured, new JsonEventFormatter(), routingInformation.Topic);
    }
}