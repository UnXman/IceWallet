using IceWallet.Core;
using IceWallet.IO;
using System;
using System.IO;

namespace IceWallet.Wallets
{
    public class UnspentCoin : IEquatable<UnspentCoin>, ISerializable
    {
        public UInt256 TxId;
        public uint Index;
        public Fixed8 Value;
        public byte[] ScriptPubKey;

        private string _address = null;
        public string Address
        {
            get
            {
                if (_address == null)
                {
                    _address = Transaction.GetAddressFromScriptPubKey(ScriptPubKey);
                }
                return _address;
            }
        }

        private UInt160 _scriptHash = null;
        public UInt160 ScriptHash
        {
            get
            {
                if (_scriptHash == null)
                {
                    _scriptHash = Transaction.GetScriptHashFromScriptPubKey(ScriptPubKey);
                }
                return _scriptHash;
            }
        }

        void ISerializable.Deserialize(BinaryReader reader)
        {
            this.TxId = reader.ReadSerializable<UInt256>();
            this.Index = reader.ReadUInt32();
            this.Value = reader.ReadSerializable<Fixed8>();
            this.ScriptPubKey = reader.ReadBytes((int)reader.ReadVarInt());
        }

        public bool Equals(UnspentCoin other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Index == other.Index && TxId == other.TxId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (!(obj is UnspentCoin))
                return false;
            return Equals((UnspentCoin)obj);
        }

        public override int GetHashCode()
        {
            return TxId.GetHashCode() + Index.GetHashCode();
        }

        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(TxId);
            writer.Write(Index);
            writer.Write(Value);
            writer.WriteVarInt(ScriptPubKey.Length); writer.Write(ScriptPubKey);
        }
    }
}
