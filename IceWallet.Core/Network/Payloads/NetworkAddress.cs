using IceWallet.IO;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace IceWallet.Network.Payloads
{
    internal class NetworkAddress : ISerializable
    {
        public const ulong NODE_NETWORK = 1;

        public ulong Services;
        public IPEndPoint EndPoint;

        public static NetworkAddress Create(IPEndPoint endpoint)
        {
            return new NetworkAddress
            {
                Services = NODE_NETWORK,
                EndPoint = endpoint
            };
        }

        public static NetworkAddress Create(IPAddress address, int port)
        {
            return Create(new IPEndPoint(address, port));
        }

        public virtual void Deserialize(BinaryReader reader)
        {
            this.Services = reader.ReadUInt64();
            IPAddress address = new IPAddress(reader.ReadBytes(16));
            ushort port = BitConverter.ToUInt16(reader.ReadBytes(2).Reverse().ToArray(), 0);
            this.EndPoint = new IPEndPoint(address, port);
        }

        public virtual void Serialize(BinaryWriter writer)
        {
            writer.Write(Services);
            writer.Write(EndPoint.Address.GetAddressBytes());
            writer.Write(BitConverter.GetBytes((ushort)EndPoint.Port).Reverse().ToArray());
        }
    }
}
