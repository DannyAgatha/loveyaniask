// NosEmu
// 


namespace PhoenixLib.DAL.MongoDB
{
    public class MongoConfiguration
    {
        public string Endpoint { get; set; }

        public short Port { get; set; }
        public string DatabaseName { get; set; }

        public override string ToString() => $"mongodb://{Endpoint}:{Port}";
    }
}