using IceWallet.Core.Scripts;
using IceWallet.Cryptography;
using IceWallet.IO;
using IceWallet.Network;
using IceWallet.Wallets;
using System;
using System.IO;
using System.Linq;

namespace IceWallet.Core
{
    public class Transaction : Inventory
    {
        public uint Version;
        public TransactionInput[] Inputs;
        public TransactionOutput[] Outputs;
        public uint LockTime;

        public override InventoryType InventoryType => InventoryType.MSG_TX;

        public override void Deserialize(BinaryReader reader)
        {
            Version = reader.ReadUInt32();
            Inputs = reader.ReadSerializableArray<TransactionInput>();
            for (int i = 1; i < Inputs.Length; i++)
                for (int j = 0; j < i; j++)
                    if (Inputs[i].PrevHash == Inputs[j].PrevHash && Inputs[i].PrevIndex == Inputs[j].PrevIndex)
                        throw new FormatException();
            Outputs = reader.ReadSerializableArray<TransactionOutput>();
            LockTime = reader.ReadUInt32();
        }

        internal static string GetAddressFromScriptPubKey(byte[] script)
        {
            UInt160 hash = GetScriptHashFromScriptPubKey(script);
            if (hash == null) return null;
            bool p2sh = script.Length == 23;
            return Wallet.ToAddress(hash, p2sh);
        }

        internal byte[] GetHashForVerification(HashType hashType, uint index, byte[] scriptPubKey)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(Version);
                writer.WriteVarInt(Inputs.Length);
                for (uint i = 0; i < Inputs.Length; i++)
                {
                    writer.Write(Inputs[i].PrevHash);
                    writer.Write(Inputs[i].PrevIndex);
                    if (i == index)
                    {
                        writer.WriteVarInt(scriptPubKey.Length);
                        writer.Write(scriptPubKey);
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

        internal static UInt160 GetScriptHashFromScriptPubKey(byte[] script)
        {
            if (script.Length == 23)
            {
                return new UInt160(script.Skip(2).Take(20).ToArray());
            }
            else if (script.Length == 25)
            {
                return new UInt160(script.Skip(3).Take(20).ToArray());
            }
            else if (script.Length == 35 || script.Length == 67)
            {
                return script.Skip(1).Take(script.Length - 2).ToArray().ToScriptHash();
            }
            else //Non-Standard Transactions
            {
                return null;
            }
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(Inputs);
            writer.Write(Outputs);
            writer.Write(LockTime);
        }
    }
}
