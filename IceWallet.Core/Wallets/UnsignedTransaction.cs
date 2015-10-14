using IceWallet.Core;
using IceWallet.Core.Scripts;
using IceWallet.Cryptography;
using IceWallet.IO;
using System;
using System.IO;

namespace IceWallet.Wallets
{
    public class UnsignedTransaction
    {
        public uint Version;
        public UnspentCoin[] Inputs;
        public TransactionOutput[] Outputs;
        public uint LockTime;

        public static UnsignedTransaction Create(UnspentCoin[] inputs, params TransactionOutput[] outputs)
        {
            if (inputs == null)
                throw new ArgumentNullException(nameof(inputs));
            if (inputs.Length == 0 || outputs.Length == 0)
                throw new ArgumentException();
            if (inputs.Sum(p => p.Value) < outputs.Sum(p => p.Value))
                throw new ArgumentException();
            return new UnsignedTransaction
            {
                Version = 1,
                Inputs = inputs,
                Outputs = outputs,
                LockTime = 0
            };
        }

        internal byte[] GetHashForSigning(HashType hashType, uint index)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(Version);
                writer.WriteVarInt(Inputs.Length);
                for (uint i = 0; i < Inputs.Length; i++)
                {
                    writer.Write(Inputs[i].TxId);
                    writer.Write(Inputs[i].Index);
                    if (i == index)
                    {
                        writer.WriteVarInt(Inputs[i].ScriptPubKey.Length);
                        writer.Write(Inputs[i].ScriptPubKey);
                    }
                    else
                    {
                        writer.WriteVarInt(0);
                    }
                    writer.Write(uint.MaxValue);
                }
                writer.Write(Outputs);
                writer.Write(LockTime);
                writer.Write((int)hashType);
                writer.Flush();
                return ms.ToArray().Sha256().Sha256();
            }
        }
    }
}
