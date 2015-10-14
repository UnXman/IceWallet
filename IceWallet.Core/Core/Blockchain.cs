using IceWallet.Core.Scripts;
using IceWallet.Cryptography;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace IceWallet.Core
{
    public abstract class Blockchain : IDisposable
    {
        public event EventHandler<Block> PersistCompleted;

        private const uint nTargetTimespan = 14 * 24 * 60 * 60;
        private const uint nTargetSpacing = 10 * 60;
        private const uint nInterval = nTargetTimespan / nTargetSpacing;
        private static readonly BigInteger MAX_TARGET = BigInteger.Parse("00000000ffffffffffffffffffffffffffffffffffffffffffffffffffffffff", NumberStyles.AllowHexSpecifier);
        public static readonly Block GenesisBlock = new Block
        {
            Version = 1,
            PrevBlock = UInt256.Zero,
            Timestamp = 1231006505,
            Bits = 0x1D00FFFF,
            Nonce = 2083236893,
            Transactions = new Transaction[]
            {
                new Transaction
                {
                    Version = 1,
                    Inputs = new TransactionInput[]
                    {
                        new TransactionInput
                        {
                            PrevHash = UInt256.Parse("0000000000000000000000000000000000000000000000000000000000000000"),
                            PrevIndex = uint.MaxValue,
                            Script = "04FFFF001D0104455468652054696D65732030332F4A616E2F32303039204368616E63656C6C6F72206F6E206272696E6B206F66207365636F6E64206261696C6F757420666F722062616E6B73".HexToBytes(),
                            Sequence = uint.MaxValue
                        }
                    },
                    Outputs = new TransactionOutput[]
                    {
                        new TransactionOutput
                        {
                            Value = Fixed8.FromDecimal(50),
                            Script = "4104678AFDB0FE5548271967F1A67130B7105CD6A828E03909A67962E0EA1F61DEB649F6BC3F4CEF38C4F35504E51EC112DE5C384DF7BA0B8D578A4C702B6BF11D5FAC".HexToBytes()
                        }
                    },
                    LockTime = 0
                }
            }
        };
        protected static readonly Dictionary<UInt256, Transaction> MemoryPool = new Dictionary<UInt256, Transaction>();

        public abstract UInt256 CurrentBlockHash { get; }
        public virtual UInt256 CurrentHeaderHash => CurrentBlockHash;
        public static Blockchain Default { get; private set; } = null;
        public virtual uint HeaderHeight => Height;
        public abstract uint Height { get; }
        public abstract bool IsReadOnly { get; }

        static Blockchain()
        {
            GenesisBlock.MerkleRoot = MerkleTree.ComputeRoot(GenesisBlock.Transactions.Select(p => p.Hash).ToArray());
        }

        protected internal abstract bool AddBlock(Block block);

        protected internal abstract void AddHeaders(IEnumerable<Block> headers);

        internal bool AddTransaction(Transaction tx)
        {
            lock (MemoryPool)
            {
                if (ContainsTransaction(tx.Hash)) return false;
                if (IsDoubleSpend(tx)) return false;
                if (!Verify(tx)) return false;
                MemoryPool.Add(tx.Hash, tx);
                return true;
            }
        }

        private static int BN_num_bytes(BigInteger number)
        {
            if (number == 0)
            {
                return 0;
            }
            return 1 + (int)Math.Floor(BigInteger.Log(BigInteger.Abs(number), 2)) / 8;
        }

        public virtual bool ContainsBlock(UInt256 hash)
        {
            return hash == GenesisBlock.Hash;
        }

        public virtual bool ContainsTransaction(UInt256 hash)
        {
            if (GenesisBlock.Transactions.Any(p => p.Hash == hash))
                return true;
            return MemoryPool.ContainsKey(hash);
        }

        public bool ContainsUnspent(TransactionInput input)
        {
            return ContainsUnspent(input.PrevHash, input.PrevIndex);
        }

        public virtual bool ContainsUnspent(UInt256 hash, uint index)
        {
            Transaction tx;
            if (!MemoryPool.TryGetValue(hash, out tx))
                return false;
            return index < tx.Outputs.Length;
        }

        public abstract void Dispose();

        public virtual Block GetBlock(uint height)
        {
            if (height == 0) return GenesisBlock;
            return null;
        }

        public virtual Block GetBlock(UInt256 hash)
        {
            if (hash == GenesisBlock.Hash)
                return GenesisBlock;
            return null;
        }

        public virtual int GetBlockHeight(UInt256 hash)
        {
            if (hash == GenesisBlock.Hash) return 0;
            return -1;
        }

        private static uint GetCompact(BigInteger value)
        {
            int nSize = BN_num_bytes(value);
            uint nCompact = 0;
            if (nSize <= 3)
            {
                nCompact = (uint)value << 8 * (3 - nSize);
            }
            else
            {
                nCompact = (uint)(value >> (8 * (nSize - 3)));
            }
            if ((nCompact & 0x00800000) > 0)
            {
                nCompact >>= 8;
                nSize++;
            }
            nCompact |= (uint)(nSize << 24);
            return nCompact;
        }

        public virtual Block GetHeader(UInt256 hash)
        {
            return GetBlock(hash)?.Header;
        }

        public abstract UInt256[] GetLeafHeaderHashes();

        public IEnumerable<Transaction> GetMemoryPool()
        {
            lock (MemoryPool)
            {
                foreach (var pair in MemoryPool)
                {
                    yield return pair.Value;
                }
            }
        }

        public abstract Block GetNextBlock(UInt256 hash);

        public abstract UInt256 GetNextBlockHash(UInt256 hash);

        protected uint GetNextWorkRequired(UInt256 prevHash, uint prevHeight)
        {
            Block last = GetHeader(prevHash);
            if ((prevHeight + 1) % nInterval == 0)
            {
                uint blockstogoback = nInterval - 1;
                if (prevHeight + 1 != nInterval)
                    blockstogoback = nInterval;
                Block first = GetBlock(prevHeight - blockstogoback);
                uint nActualTimespan = last.Timestamp - first.Timestamp;
                if (nActualTimespan < nTargetTimespan / 4)
                    nActualTimespan = nTargetTimespan / 4;
                if (nActualTimespan > nTargetTimespan * 4)
                    nActualTimespan = nTargetTimespan * 4;
                BigInteger target = last.Bits.SetCompact();
                target = BigInteger.Min(MAX_TARGET, target * nActualTimespan / nTargetTimespan);
                return GetCompact(target);
            }
            else
            {
                return last.Bits;
            }
        }

        public virtual Transaction GetTransaction(UInt256 hash)
        {
            Transaction tx;
            if (MemoryPool.TryGetValue(hash, out tx))
                return tx;
            return GenesisBlock.Transactions.FirstOrDefault(p => p.Hash == hash);
        }

        public virtual TransactionOutput GetUnspent(UInt256 hash, uint index)
        {
            Transaction tx;
            if (!MemoryPool.TryGetValue(hash, out tx) || index >= tx.Outputs.Length)
                return null;
            return tx.Outputs[index];
        }

        public abstract bool IsDoubleSpend(Transaction tx);

        protected void OnPersistCompleted(Block block)
        {
            lock (MemoryPool)
            {
                foreach (Transaction tx in block.Transactions)
                {
                    MemoryPool.Remove(tx.Hash);
                }
            }
            if (PersistCompleted != null) PersistCompleted(this, block);
        }

        public static void RegisterBlockchain(Blockchain blockchain)
        {
            if (blockchain == null) throw new ArgumentNullException();
            if (Default != null)
                Default.Dispose();
            Default = blockchain;
        }

        private bool Verify(Transaction tx)
        {
            Transaction[] prev_txs = tx.Inputs.Select(p => p.PrevHash).Distinct().Select(p => GetTransaction(p)).ToArray();
            if (prev_txs.Any(p => p == null)) return false;
            Dictionary<UInt256, Transaction> references = prev_txs.ToDictionary(p => p.Hash);
            if (tx.Inputs.Sum(p => references[p.PrevHash].Outputs[p.PrevIndex].Value) < tx.Outputs.Sum(p => p.Value))
                return false;
            for (uint i = 0; i < tx.Inputs.Length; i++)
            {
                byte[] scriptPubkey = references[tx.Inputs[i].PrevHash].Outputs[tx.Inputs[i].PrevIndex].Script;
                ScriptEngine engine = new ScriptEngine(tx, i, scriptPubkey);
                if (!engine.Execute()) return false;
            }
            return true;
        }
    }
}
