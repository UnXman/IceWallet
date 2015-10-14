using IceWallet.Core;
using IceWallet.Core.Scripts;
using IceWallet.Cryptography;
using IceWallet.Cryptography.ECC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using ECDsa = IceWallet.Cryptography.ECC.ECDsa;

namespace IceWallet.Wallets
{
    public abstract class Wallet
    {
        public const byte CoinVersion = 0x00;
        public const byte P2SHVersion = 0x05;

        public static byte[] CreatePrivateKey()
        {
            byte[] key = new byte[32];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetNonZeroBytes(key);
            }
            return key;
        }

        public abstract WalletEntry FindEntry(UInt160 scriptHash);

        public abstract IEnumerable<UnspentCoin> FindUnspentCoins(bool includeUnmaturedCoins);

        private static UnspentCoin[] FindUnspentCoins(IEnumerable<UnspentCoin> coins, Fixed8 total)
        {
            UnspentCoin[] coins_array = coins.ToArray();
            UnspentCoin coin = coins_array.FirstOrDefault(p => p.Value == total);
            if (coin != null) return new UnspentCoin[] { coin };
            coin = coins_array.OrderBy(p => p.Value).FirstOrDefault(p => p.Value > total);
            if (coin != null) return new UnspentCoin[] { coin };
            Fixed8 sum = coins_array.Sum(p => p.Value);
            if (sum < total) return null;
            if (sum == total) return coins_array;
            return coins_array.OrderByDescending(p => p.Value).TakeWhile(p =>
            {
                if (total <= Fixed8.Zero) return false;
                total -= p.Value;
                return true;
            }).ToArray();
        }

        public abstract IEnumerable<string> GetAddresses();

        public virtual Fixed8 GetBalance()
        {
            return FindUnspentCoins(true).Sum(p => p.Value);
        }

        public virtual string GetChangeAddress()
        {
            return GetAddresses().FirstOrDefault();
        }

        public static byte[] GetPrivateKeyFromWIF(string wif, out bool compressed)
        {
            if (wif == null)
                throw new ArgumentNullException();
            byte[] data = Base58.Decode(wif);
            if (data.Length != 37 && data.Length != 38)
                throw new FormatException();
            if (data[0] != CoinVersion + 0x80)
                throw new FormatException();
            if (data.Length == 38 && data[33] != 0x01)
                throw new FormatException();
            byte[] checksum = data.Take(data.Length - 4).Sha256().Sha256().Take(4).ToArray();
            if (!data.Skip(data.Length - 4).SequenceEqual(checksum))
                throw new FormatException();
            compressed = data.Length == 38;
            return data.Skip(1).Take(32).ToArray();
        }

        public static string GetWIFFromPrivateKey(byte[] privateKey, bool compressed)
        {
            byte[] data;
            if (compressed)
            {
                data = new byte[38];
            }
            else
            {
                data = new byte[37];
            }
            data[0] = CoinVersion + 0x80;
            Buffer.BlockCopy(privateKey, 0, data, 1, 32);
            if (compressed)
            {
                data[33] = 0x01;
            }
            byte[] checksum = data.Take(data.Length - 4).Sha256().Sha256().Take(4).ToArray();
            Buffer.BlockCopy(checksum, 0, data, data.Length - 4, 4);
            return Base58.Encode(data);
        }

        public Transaction SignTransaction(UnsignedTransaction utx)
        {
            const HashType hashType = HashType.SIGHASH_ALL;
            Transaction tx = new Transaction
            {
                Version = utx.Version,
                Inputs = new TransactionInput[utx.Inputs.Length],
                Outputs = utx.Outputs,
                LockTime = utx.LockTime
            };
            for (uint i = 0; i < utx.Inputs.Length; i++)
            {
                tx.Inputs[i] = new TransactionInput
                {
                    PrevHash = utx.Inputs[i].TxId,
                    PrevIndex = utx.Inputs[i].Index,
                    Sequence = uint.MaxValue
                };
                if (utx.Inputs[i].ScriptHash == null) return null;
                WalletEntry entry = FindEntry(utx.Inputs[i].ScriptHash);
                if (entry == null) return null;
                BigInteger[] signature;
                using (entry.Decrypt())
                {
                    ECDsa signer = new ECDsa(entry.PrivateKey, ECCurve.Secp256k1);
                    signature = signer.GenerateSignature(utx.GetHashForSigning(hashType, i));
                }
                byte[] sigEncoded;
                using (MemoryStream ms = new MemoryStream())
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    byte[] r = signature[0].ToByteArray().Reverse().ToArray();
                    byte[] s = signature[1].ToByteArray().Reverse().ToArray();
                    writer.Write((byte)0x30);
                    writer.Write((byte)(2 + r.Length + 2 + s.Length));
                    writer.Write((byte)0x02);
                    writer.Write((byte)r.Length);
                    writer.Write(r);
                    writer.Write((byte)0x02);
                    writer.Write((byte)s.Length);
                    writer.Write(s);
                    writer.Write((byte)hashType);
                    writer.Flush();
                    sigEncoded = ms.ToArray();
                }
                using (ScriptBuilder sb = new ScriptBuilder())
                {
                    sb.Push(sigEncoded);
                    sb.Push(entry.PublicKey.EncodePoint(entry.Compressed));
                    tx.Inputs[i].Script = sb.ToArray();
                }
            }
            return tx;
        }

        public static string ToAddress(UInt160 hash, bool p2sh)
        {
            byte version = p2sh ? P2SHVersion : CoinVersion;
            byte[] data = new byte[] { version }.Concat(hash.ToArray()).ToArray();
            return Base58.Encode(data.Concat(data.Sha256().Sha256().Take(4)).ToArray());
        }

        public static UInt160 ToScriptHash(string address, out bool p2sh)
        {
            byte[] data = Base58.Decode(address);
            if (data.Length != 25)
                throw new FormatException();
            if (data[0] != CoinVersion && data[0] != P2SHVersion)
                throw new FormatException();
            if (!data.Take(21).Sha256().Sha256().Take(4).SequenceEqual(data.Skip(21)))
                throw new FormatException();
            p2sh = data[0] == P2SHVersion;
            return new UInt160(data.Skip(1).Take(20).ToArray());
        }
    }
}
