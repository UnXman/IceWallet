using System.IO;
using System.Net;

namespace IceWallet.Network.Payloads
{
    internal class NetworkAddressWithTime : NetworkAddress
    {
        public uint Timestamp;

        public static NetworkAddressWithTime Create(IPEndPoint endpoint, ulong services, uint timestamp)
        {
            return new NetworkAddressWithTime
            {
                Timestamp = timestamp,
                Services = services,
                EndPoint = endpoint
            };
        }

        public override void Deserialize(BinaryReader reader)
        {
            this.Timestamp = reader.ReadUInt32();
            base.Deserialize(reader);
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(Timestamp);
            base.Serialize(writer);
        }
    }
}
