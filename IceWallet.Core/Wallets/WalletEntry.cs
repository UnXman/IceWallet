using IceWallet.Cryptography;
using IceWallet.Cryptography.ECC;
using System;
using System.Security.Cryptography;

namespace IceWallet.Wallets
{
    public class WalletEntry : IEquatable<WalletEntry>
    {
        public byte[] PrivateKey;
        public ECPoint PublicKey;
        public UInt160 PublicKeyHash;
        public bool Compressed;

        private string _address;
        public virtual string Address
        {
            get
            {
                if (_address == null)
                {
                    _address = Wallet.ToAddress(PublicKeyHash, false);
                }
                return _address;
            }
        }

        public WalletEntry(byte[] privateKey, ECPoint publicKey, UInt160 publicKeyHash, bool compressed)
        {
            this.PrivateKey = privateKey;
            this.PublicKey = publicKey;
            this.PublicKeyHash = publicKeyHash;
            this.Compressed = compressed;
            ProtectedMemory.Protect(privateKey, MemoryProtectionScope.SameProcess);
        }

        public static WalletEntry Create(byte[] privateKey, bool compressed = true)
        {
            ECPoint publicKey = ECCurve.Secp256k1.G * privateKey;
            UInt160 publicKeyHash = new UInt160(publicKey.EncodePoint(compressed).Sha256().RIPEMD160());
            return new WalletEntry(privateKey, publicKey, publicKeyHash, compressed);
        }

        public IDisposable Decrypt()
        {
            return new ProtectedMemoryContext(PrivateKey, MemoryProtectionScope.SameProcess);
        }

        public bool Equals(WalletEntry other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(null, other)) return false;
            return PublicKeyHash == other.PublicKeyHash;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as WalletEntry);
        }

        public string Export()
        {
            using (Decrypt())
            {
                return Wallet.GetWIFFromPrivateKey(PrivateKey, Compressed);
            }
        }

        public override int GetHashCode()
        {
            return BitConverter.ToInt32(PublicKeyHash.ToArray(), 0);
        }

        public override string ToString()
        {
            return Address;
        }
    }
}
