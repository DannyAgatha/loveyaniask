namespace PhoenixLib.ServiceBus.MQTT
{
    public class MqttConfiguration
    {
        public MqttConfiguration(string address, string clientName, int? port = null)
        {
            Address = address;
            ClientName = clientName;
            Port = port;
        }

        public string Address { get; }
        public int? Port { get; }
        public string ClientName { get; }
    }
}