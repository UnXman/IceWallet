using IceWallet.Cryptography;
using IceWallet.IO;
using IceWallet.Network;
using System;
using System.IO;
using System.Linq;

namespace IceWallet.Core
{
    public class Block : Inventory, IEquatable<Block>
    {
        public uint Version;
        public UInt256 PrevBlock;
        public UInt256 MerkleRoot;
        public uint Timestamp;
        public uint Bits;
        public uint Nonce;
        public Transaction[] Transactions;

        public Block Header
        {
            get
            {
                if (IsHeader) return this;
                return new Block
                {
                    Version = Version,
                    PrevBlock = PrevBlock,
                    MerkleRoot = MerkleRoot,
                    Timestamp = Timestamp,
                    Bits = Bits,
                    Nonce = Nonce,
                    Transactions = new Transaction[0]
                };
            }
        }
        public override InventoryType InventoryType => InventoryType.MSG_BLOCK;
        public bool IsHeader => Transactions.Length == 0;

        public override void Deserialize(BinaryReader reader)
        {
            this.Version = reader.ReadUInt32();
            this.PrevBlock = reader.ReadSerializable<UInt256>();
            this.MerkleRoot = reader.ReadSerializable<UInt256>();
            this.Timestamp = reader.ReadUInt32();
            this.Bits = reader.ReadUInt32();
            this.Nonce = reader.ReadUInt32();
            if (Hash > Bits.SetCompact().ToUInt256())
                throw new FormatException();
            this.Transactions = reader.ReadSerializableArray<Transaction>();
            if (Transactions.Length > 0 && MerkleTree.ComputeRoot(Transactions.Select(p => p.Hash).ToArray()) != MerkleRoot)
                throw new FormatException();
        }

        public bool Equals(Block other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(null, other)) return false;
            return Hash.Equals(other.Hash);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Block);
        }

        public static Block FromTrimmedData(byte[] data, int index, Func<UInt256, Transaction> txSelector = null)
        {
            Block block = new Block();
            using (MemoryStream ms = new MemoryStream(data, index, data.Length - index, false))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                block.Version = reader.ReadUInt32();
                block.PrevBlock = reader.ReadSerializable<UInt256>();
                block.MerkleRoot = reader.ReadSerializable<UInt256>();
                block.Timestamp = reader.ReadUInt32();
                block.Bits = reader.ReadUInt32();
                block.Nonce = reader.ReadUInt32();
                if (txSelector == null)
                {
                    block.Transactions = new Transaction[0];
                }
                else
                {
                    block.Transactions = new Transaction[reader.ReadVarInt()];
                    for (int i = 0; i < block.Transactions.Length; i++)
                    {
                        block.Transactions[i] = txSelector(reader.ReadSerializable<UInt256>());
                    }
                }
            }
            return block;
        }

        public override int GetHashCode()
        {
            return Hash.GetHashCode();
        }

        protected override byte[] GetHashData()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(Version);
                writer.Write(PrevBlock);
                writer.Write(MerkleRoot);
                writer.Write(Timestamp);
                writer.Write(Bits);
                writer.Write(Nonce);
                return ms.ToArray();
            }
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(PrevBlock);
            writer.Write(MerkleRoot);
            writer.Write(Timestamp);
            writer.Write(Bits);
            writer.Write(Nonce);
            writer.Write(Transactions);
        }

        public byte[] Trim()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(Version);
                writer.Write(PrevBlock);
                writer.Write(MerkleRoot);
                writer.Write(Timestamp);
                writer.Write(Bits);
                writer.Write(Nonce);
                writer.Write(Transactions.Select(p => p.Hash).ToArray());
                writer.Flush();
                return ms.ToArray();
            }
        }
    }
}
