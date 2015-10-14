using IceWallet.Cryptography;
using IceWallet.IO;
using System;
using System.IO;

namespace IceWallet.Network
{
    public abstract class Inventory : ISerializable
    {
        [NonSerialized]
        private UInt256 _hash = null;

        public UInt256 Hash
        {
            get
            {
                if (_hash == null)
                {
                    _hash = new UInt256(GetHashData().Sha256().Sha256());
                }
                return _hash;
            }
        }

        public abstract InventoryType InventoryType { get; }

        public abstract void Deserialize(BinaryReader reader);

        protected virtual byte[] GetHashData()
        {
            return this.ToArray();
        }

        public abstract void Serialize(BinaryWriter writer);
    }
}
