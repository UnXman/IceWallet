using IceWallet.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IceWallet.Network.Payloads
{
    internal class GetBlocksPayload : ISerializable
    {
        public uint Version;
        public UInt256[] HashStart;
        public UInt256 HashStop;

        public static GetBlocksPayload Create(IEnumerable<UInt256> hash_start, UInt256 hash_stop = null)
        {
            return new GetBlocksPayload
            {
                Version = LocalNode.PROTOCOL_VERSION,
                HashStart = hash_start.ToArray(),
                HashStop = hash_stop ?? UInt256.Zero
            };
        }

        void ISerializable.Deserialize(BinaryReader reader)
        {
            this.Version = reader.ReadUInt32();
            this.HashStart = reader.ReadSerializableArray<UInt256>();
            this.HashStop = reader.ReadSerializable<UInt256>();
        }

        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(HashStart);
            writer.Write(HashStop);
        }
    }
}
