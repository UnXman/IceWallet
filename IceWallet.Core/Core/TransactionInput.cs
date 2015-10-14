using IceWallet.IO;
using System.IO;

namespace IceWallet.Core
{
    public class TransactionInput : ISerializable
    {
        public UInt256 PrevHash;
        public uint PrevIndex;
        public byte[] Script;
        public uint Sequence;

        void ISerializable.Deserialize(BinaryReader reader)
        {
            this.PrevHash = reader.ReadSerializable<UInt256>();
            this.PrevIndex = reader.ReadUInt32();
            this.Script = reader.ReadBytes((int)reader.ReadVarInt());
            this.Sequence = reader.ReadUInt32();
        }

        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(PrevHash);
            writer.Write(PrevIndex);
            writer.WriteVarInt(Script.Length); writer.Write(Script);
            writer.Write(Sequence);
        }
    }
}
