using IceWallet.IO;
using System;
using System.IO;
using System.Net;
using System.Reflection;

namespace IceWallet.Network.Payloads
{
    internal class VersionPayload : ISerializable
    {
        public const uint MIN_PEER_PROTO_VERSION = 209;
        private static readonly string USER_AGENT = string.Format("/IceWallet:{0}/", Assembly.GetExecutingAssembly().GetName().Version.ToString(3));

        public uint Version;
        public ulong Services;
        public ulong Timestamp;
        public NetworkAddress AddressReceiver;
        public NetworkAddress AddressFrom;
        public ulong Nonce;
        public string UserAgent;
        public uint StartHeight;
        public bool Relay;

        public static VersionPayload Create(IPEndPoint local, IPEndPoint remote, uint start_height)
        {
            Random rand = new Random();
            byte[] nonce = new byte[sizeof(ulong)];
            rand.NextBytes(nonce);
            return new VersionPayload
            {
                Version = LocalNode.PROTOCOL_VERSION,
                Services = NetworkAddress.NODE_NETWORK,
                Timestamp = DateTime.Now.ToTimestamp(),
                AddressReceiver = NetworkAddress.Create(remote),
                AddressFrom = NetworkAddress.Create(local),
                Nonce = BitConverter.ToUInt64(nonce, 0),
                UserAgent = USER_AGENT,
                StartHeight = start_height,
                Relay = true
            };
        }

        void ISerializable.Deserialize(BinaryReader reader)
        {
            this.Version = reader.ReadUInt32();
            if (Version < MIN_PEER_PROTO_VERSION)
                throw new FormatException();
            this.Services = reader.ReadUInt64();
            this.Timestamp = reader.ReadUInt64();
            this.AddressReceiver = reader.ReadSerializable<NetworkAddress>();
            this.AddressFrom = reader.ReadSerializable<NetworkAddress>();
            this.Nonce = reader.ReadUInt64();
            this.UserAgent = reader.ReadVarString();
            this.StartHeight = reader.ReadUInt32();
            this.Relay = true;
            if (Version >= 70001)
            {
                try
                {
                    this.Relay = reader.ReadBoolean();
                }
                catch (EndOfStreamException) { }
            }
        }

        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(Services);
            writer.Write(Timestamp);
            writer.Write(AddressReceiver);
            writer.Write(AddressFrom);
            writer.Write(Nonce);
            writer.WriteVarString(UserAgent);
            writer.Write(StartHeight);
            writer.Write(Relay);
        }
    }
}
