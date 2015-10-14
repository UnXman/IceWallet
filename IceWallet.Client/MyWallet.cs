using IceWallet.Core.Scripts;
using IceWallet.Cryptography;
using IceWallet.Cryptography.ECC;
using IceWallet.IO;
using IceWallet.Wallets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace IceWallet.Client
{
    internal class MyWallet : Wallet
    {
        private WalletEntryCollection accounts = new WalletEntryCollection();

        private MyWallet() { }

        public static MyWallet Create()
        {
            MyWallet wallet = new MyWallet();
            wallet.CreateEntry();
            return wallet;
        }

        public WalletEntry CreateEntry()
        {
            WalletEntry entry = WalletEntry.Create(CreatePrivateKey());
            accounts.Add(entry);
            return entry;
        }

        public void DeleteEntry(UInt160 scriptHash)
        {
            accounts.Remove(scriptHash);
        }

        public override WalletEntry FindEntry(UInt160 scriptHash)
        {
            if (!accounts.Contains(scriptHash)) return null;
            return accounts[scriptHash];
        }

        public override IEnumerable<UnspentCoin> FindUnspentCoins(bool includeUnmaturedCoins)
        {
            throw new NotSupportedException();
        }

        public override IEnumerable<string> GetAddresses()
        {
            return accounts.Select(p => p.Address);
        }

        public WalletEntry Import(string wif)
        {
            bool compressed;
            byte[] privateKey = GetPrivateKeyFromWIF(wif, out compressed);
            WalletEntry entry = WalletEntry.Create(privateKey, compressed);
            accounts.Add(entry);
            return entry;
        }

        public static MyWallet Load(string path, string password)
        {
            MyWallet wallet = new MyWallet();
            byte[] aes_key = Encoding.UTF8.GetBytes(password).Sha256().Sha256();
            byte[] aes_iv = new byte[16];
            using (AesManaged aes = new AesManaged())
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                fs.Read(aes_iv, 0, aes_iv.Length);
                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes_key, aes_iv))
                using (CryptoStream cs = new CryptoStream(fs, decryptor, CryptoStreamMode.Read))
                using (BinaryReader reader = new BinaryReader(cs))
                {
                    int count = (int)reader.ReadVarInt();
                    for (int i = 0; i < count; i++)
                    {
                        byte[] privateKey = reader.ReadBytes(32);
                        bool compressed = reader.ReadBoolean();
                        ECPoint publicKey = ECCurve.Secp256k1.G * privateKey;
                        UInt160 publicKeyHash = publicKey.EncodePoint(compressed).ToScriptHash();
                        wallet.accounts.Add(new WalletEntry(privateKey, publicKey, publicKeyHash, compressed));
                    }
                }
            }
            Array.Clear(aes_key, 0, aes_key.Length);
            return wallet;
        }

        public void Save(string path, string password)
        {
            byte[] aes_key = Encoding.UTF8.GetBytes(password).Sha256().Sha256();
            using (AesManaged aes = new AesManaged())
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                fs.Write(aes.IV, 0, aes.IV.Length);
                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes_key, aes.IV))
                using (CryptoStream cs = new CryptoStream(fs, encryptor, CryptoStreamMode.Write))
                using (BinaryWriter w = new BinaryWriter(cs))
                {
                    w.WriteVarInt(accounts.Count);
                    foreach (WalletEntry entry in accounts)
                    {
                        using (entry.Decrypt())
                        {
                            w.Write(entry.PrivateKey);
                        }
                        w.Write(entry.Compressed);
                    }
                }
            }
            Array.Clear(aes_key, 0, aes_key.Length);
        }
    }
}
