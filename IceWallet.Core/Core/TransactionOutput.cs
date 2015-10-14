using IceWallet.Core.Scripts;
using IceWallet.IO;
using IceWallet.Wallets;
using System;
using System.IO;

namespace IceWallet.Core
{
    public class TransactionOutput : ISerializable
    {
        public Fixed8 Value;
        public byte[] Script;

        private string _address = null;
        public string Address
        {
            get
            {
                if (_address == null)
                {
                    _address = Transaction.GetAddressFromScriptPubKey(Script);
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
                    _scriptHash = Transaction.GetScriptHashFromScriptPubKey(Script);
                }
                return _scriptHash;
            }
        }

        public static TransactionOutput Create(Fixed8 value, string address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            bool p2sh;
            UInt160 hash = Wallet.ToScriptHash(address, out p2sh);
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                if (p2sh)
                {
                    sb.Add(ScriptOp.OP_HASH160);
                    sb.Push(hash.ToArray());
                    sb.Add(ScriptOp.OP_EQUAL);
                }
                else
                {
                    sb.Add(ScriptOp.OP_DUP);
                    sb.Add(ScriptOp.OP_HASH160);
                    sb.Push(hash.ToArray());
                    sb.Add(ScriptOp.OP_EQUALVERIFY);
                    sb.Add(ScriptOp.OP_CHECKSIG);
                }
                return new TransactionOutput
                {
                    Value = value,
                    Script = sb.ToArray()
                };
            }
        }

        void ISerializable.Deserialize(BinaryReader reader)
        {
            this.Value = reader.ReadSerializable<Fixed8>();
            this.Script = reader.ReadBytes((int)reader.ReadVarInt());
        }

        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(Value);
            writer.WriteVarInt(Script.Length); writer.Write(Script);
        }
    }
}
