using IceWallet.Core;
using IceWallet.IO;
using IceWallet.IO.Caching;
using LevelDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace IceWallet.Implementations.Blockchains.LevelDB
{
    public class LevelDBBlockchain : Blockchain
    {
        private DB db;
        private Thread thread_persistence;
        private Tree<UInt256, Block> header_chain = new Tree<UInt256, Block>(GenesisBlock.Hash, GenesisBlock);
        private List<UInt256> header_index = new List<UInt256>();
        private Dictionary<UInt256, Block> block_cache = new Dictionary<UInt256, Block>();
        private UInt256 current_block_hash = GenesisBlock.Hash;
        private UInt256 current_header_hash = GenesisBlock.Hash;
        private uint current_block_height = 0;
        private uint stored_header_count = 0;
        private bool disposed = false;

        public override UInt256 CurrentBlockHash => current_block_hash;
        public override UInt256 CurrentHeaderHash => current_header_hash;
        public override uint HeaderHeight => header_chain.Nodes[current_header_hash].Height;
        public override uint Height => current_block_height;
        public override bool IsReadOnly => false;
        public bool VerifyBlocks { get; set; } = false;

        public LevelDBBlockchain(string path)
        {
            header_index.Add(GenesisBlock.Hash);
            Slice value;
            db = DB.Open(path);
            if (db.TryGet(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.CFG_Initialized), out value) && value.ToBoolean())
            {
                ReadOptions options = new ReadOptions { FillCache = false };
                value = db.Get(options, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentBlock));
                this.current_block_hash = new UInt256(value.ToArray().Take(32).ToArray());
                this.current_block_height = BitConverter.ToUInt32(value.ToArray(), 32);
                foreach (Block header in db.Find(options, SliceBuilder.Begin(DataEntryPrefix.DATA_HeaderList), (k, v) =>
                {
                    using (MemoryStream ms = new MemoryStream(v.ToArray(), false))
                    using (BinaryReader r = new BinaryReader(ms))
                    {
                        return new
                        {
                            Index = BitConverter.ToUInt32(k.ToArray(), 1),
                            Headers = r.ReadSerializableArray<Block>()
                        };
                    }
                }).OrderBy(p => p.Index).SelectMany(p => p.Headers).ToArray())
                {
                    if (header.Hash != GenesisBlock.Hash)
                    {
                        header_chain.Add(header.Hash, header, header.PrevBlock);
                        header_index.Add(header.Hash);
                    }
                    stored_header_count++;
                }
                if (stored_header_count == 0)
                {
                    Dictionary<UInt256, Block> table = db.Find(options, SliceBuilder.Begin(DataEntryPrefix.DATA_Block), (k, v) => Block.FromTrimmedData(v.ToArray(), 0)).ToDictionary(p => p.PrevBlock);
                    for (UInt256 hash = GenesisBlock.Hash; hash != current_block_hash;)
                    {
                        Block header = table[hash];
                        header_chain.Add(header.Hash, header, header.PrevBlock);
                        header_index.Add(header.Hash);
                        hash = header.Hash;
                    }
                }
                else if (current_block_height >= stored_header_count)
                {
                    List<Block> list = new List<Block>();
                    for (UInt256 hash = current_block_hash; hash != header_index[(int)stored_header_count - 1];)
                    {
                        Block header = Block.FromTrimmedData(db.Get(options, SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(hash)).ToArray(), 0);
                        list.Add(header);
                        header_index.Insert((int)stored_header_count, hash);
                        hash = header.PrevBlock;
                    }
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        header_chain.Add(list[i].Hash, list[i], list[i].PrevBlock);
                    }
                }
                this.current_header_hash = header_index[header_index.Count - 1];
            }
            else
            {
                WriteBatch batch = new WriteBatch();
                ReadOptions options = new ReadOptions { FillCache = false };
                using (Iterator it = db.NewIterator(options))
                {
                    for (it.SeekToFirst(); it.Valid(); it.Next())
                    {
                        batch.Delete(it.Key());
                    }
                }
                batch.Put(SliceBuilder.Begin(DataEntryPrefix.CFG_Version), 0);
                db.Write(WriteOptions.Default, batch);
                Persist(GenesisBlock);
                db.Put(WriteOptions.Default, SliceBuilder.Begin(DataEntryPrefix.CFG_Initialized), true);
            }
            thread_persistence = new Thread(PersistBlocks);
            thread_persistence.Name = "LevelDBBlockchain.PersistBlocks";
            thread_persistence.Start();
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        protected internal override bool AddBlock(Block block)
        {
            lock (block_cache)
            {
                if (!block_cache.ContainsKey(block.Hash))
                {
                    block_cache.Add(block.Hash, block);
                }
            }
            lock (header_chain)
            {
                if (!header_chain.Nodes.ContainsKey(block.PrevBlock)) return false;
                if (!header_chain.Nodes.ContainsKey(block.Hash))
                {
                    if (VerifyBlocks && !Verify(block)) return false;
                    header_chain.Add(block.Hash, block.Header, block.PrevBlock);
                    OnAddHeader(block);
                }
            }
            return true;
        }

        protected internal override void AddHeaders(IEnumerable<Block> headers)
        {
            lock (header_chain)
            {
                foreach (Block header in headers)
                {
                    if (!header_chain.Nodes.ContainsKey(header.PrevBlock)) break;
                    if (header_chain.Nodes.ContainsKey(header.Hash)) continue;
                    if (VerifyBlocks && !Verify(header)) break;
                    header_chain.Add(header.Hash, header, header.PrevBlock);
                    OnAddHeader(header);
                }
            }
        }

        public override bool ContainsBlock(UInt256 hash)
        {
            if (!header_chain.Nodes.ContainsKey(hash)) return false;
            TreeNode<Block> node = header_chain.Nodes[hash];
            TreeNode<Block> i = header_chain.Nodes[current_block_hash];
            if (i.Height < node.Height) return false;
            while (i.Height > node.Height)
                i = i.Parent;
            return i == node;
        }

        public override bool ContainsTransaction(UInt256 hash)
        {
            if (base.ContainsTransaction(hash)) return true;
            Slice value;
            return db.TryGet(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.DATA_Transaction).Add(hash), out value);
        }

        public override bool ContainsUnspent(UInt256 hash, uint index)
        {
            if (base.ContainsUnspent(hash, index)) return true;
            Slice value;
            if (!db.TryGet(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.IX_Unspent).Add(hash), out value))
                return false;
            return value.ToArray().GetUInt32Array().Contains(index);
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Dispose();
        }

        public override void Dispose()
        {
            disposed = true;
            AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
            thread_persistence.Join();
            if (db != null)
            {
                db.Dispose();
                db = null;
            }
        }

        public override Block GetBlock(uint height)
        {
            Block block = base.GetBlock(height);
            if (block != null) return block;
            if (current_block_height < height) return null;
            lock (header_chain)
            {
                if (header_index.Count <= height) return null;
                return GetBlockInternal(header_index[(int)height], ReadOptions.Default);
            }
        }

        public override Block GetBlock(UInt256 hash)
        {
            Block block = base.GetBlock(hash);
            if (block == null)
            {
                block = GetBlockInternal(hash, ReadOptions.Default);
            }
            return block;
        }

        private Block GetBlockInternal(UInt256 hash, ReadOptions options)
        {
            Slice value;
            if (!db.TryGet(options, SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(hash), out value))
                return null;
            return Block.FromTrimmedData(value.ToArray(), 0, p => GetTransaction(p, options));
        }

        public override int GetBlockHeight(UInt256 hash)
        {
            if (!header_chain.Nodes.ContainsKey(hash)) return -1;
            return (int)header_chain.Nodes[hash].Height;
        }

        public override Block GetHeader(UInt256 hash)
        {
            if (!header_chain.Nodes.ContainsKey(hash)) return null;
            return header_chain[hash];
        }

        public override UInt256[] GetLeafHeaderHashes()
        {
            lock (header_chain)
            {
                return header_chain.Leaves.Select(p => p.Item.Hash).ToArray();
            }
        }

        public override Block GetNextBlock(UInt256 hash)
        {
            return GetBlockInternal(GetNextBlockHash(hash), ReadOptions.Default);
        }

        public override UInt256 GetNextBlockHash(UInt256 hash)
        {
            lock (header_chain)
            {
                if (!header_chain.Nodes.ContainsKey(hash)) return null;
                uint height = header_chain.Nodes[hash].Height;
                if (hash != header_index[(int)height]) return null;
                if (header_index.Count <= height + 1) return null;
                return header_chain[header_index[(int)height + 1]].Hash;
            }
        }

        public override Transaction GetTransaction(UInt256 hash)
        {
            Transaction tx = base.GetTransaction(hash);
            if (tx == null)
            {
                tx = GetTransaction(hash, ReadOptions.Default);
            }
            return tx;
        }

        private Transaction GetTransaction(UInt256 hash, ReadOptions options)
        {
            Slice value;
            if (!db.TryGet(options, SliceBuilder.Begin(DataEntryPrefix.DATA_Transaction).Add(hash), out value))
                return null;
            return value.ToArray().AsSerializable<Transaction>();
        }

        public override TransactionOutput GetUnspent(UInt256 hash, uint index)
        {
            TransactionOutput unspent = base.GetUnspent(hash, index);
            if (unspent != null) return unspent;
            ReadOptions options = new ReadOptions();
            using (options.Snapshot = db.GetSnapshot())
            {
                Slice value;
                if (!db.TryGet(options, SliceBuilder.Begin(DataEntryPrefix.IX_Unspent).Add(hash), out value))
                    return null;
                if (!value.ToArray().GetUInt32Array().Contains(index))
                    return null;
                return GetTransaction(hash, options).Outputs[index];
            }
        }

        public override bool IsDoubleSpend(Transaction tx)
        {
            lock (MemoryPool)
            {
                if (MemoryPool.Values.SelectMany(p => p.Inputs).Intersect(tx.Inputs).Count() > 0)
                    return true;
            }
            ReadOptions options = new ReadOptions();
            using (options.Snapshot = db.GetSnapshot())
            {
                foreach (var group in tx.Inputs.GroupBy(p => p.PrevHash))
                {
                    Slice value;
                    if (!db.TryGet(options, SliceBuilder.Begin(DataEntryPrefix.IX_Unspent).Add(group.Key), out value))
                        return true;
                    HashSet<uint> unspents = new HashSet<uint>(value.ToArray().GetUInt32Array());
                    if (group.Any(p => !unspents.Contains(p.PrevIndex)))
                        return true;
                }
            }
            return false;
        }

        private void OnAddHeader(Block header)
        {
            if (header.PrevBlock == current_header_hash)
            {
                current_header_hash = header.Hash;
                header_index.Add(header.Hash);
                uint height = header_chain.Nodes[current_header_hash].Height;
                if (height % 2000 == 0)
                {
                    WriteBatch batch = new WriteBatch();
                    while (height - 2000 > stored_header_count)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        using (BinaryWriter w = new BinaryWriter(ms))
                        {
                            w.Write(header_index.Skip((int)stored_header_count).Take(2000).Select(p => header_chain[p]).ToArray());
                            w.Flush();
                            batch.Put(SliceBuilder.Begin(DataEntryPrefix.DATA_HeaderList).Add(stored_header_count), ms.ToArray());
                        }
                        stored_header_count += 2000;
                    }
                    db.Write(WriteOptions.Default, batch);
                }
            }
            else
            {
                TreeNode<Block> main = header_chain.Leaves.OrderBy(p => p.Item.Bits).ThenByDescending(p => p.Height).ThenBy(p => p.Item.Hash).First();
                if (main.Item.Hash != current_header_hash)
                {
                    TreeNode<Block> fork = header_chain.Nodes[current_header_hash];
                    current_header_hash = main.Item.Hash;
                    TreeNode<Block> common = header_chain.FindCommonNode(main, fork);
                    header_index.RemoveRange((int)common.Height + 1, header_index.Count - (int)common.Height - 1);
                    for (TreeNode<Block> i = main; i != common; i = i.Parent)
                    {
                        header_index.Insert((int)common.Height + 1, i.Item.Hash);
                    }
                    if (header_chain.Nodes[current_block_hash].Height > common.Height)
                    {
                        Rollback(common.Item.Hash);
                    }
                }
            }
        }

        private void Persist(Block block)
        {
            Dictionary<UInt256, HashSet<uint>> unspents = new Dictionary<UInt256, HashSet<uint>>();
            WriteBatch batch = new WriteBatch();
            batch.Put(SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(block.Hash), block.Trim());
            foreach (Transaction tx in block.Transactions)
            {
                batch.Put(SliceBuilder.Begin(DataEntryPrefix.DATA_Transaction).Add(tx.Hash), tx.ToArray());
                unspents.Add(tx.Hash, new HashSet<uint>());
                for (uint index = 0; index < tx.Outputs.Length; index++)
                {
                    if (tx.Outputs[index].Value > Fixed8.Zero)
                        unspents[tx.Hash].Add(index);
                }
            }
            foreach (TransactionInput input in block.Transactions.Skip(1).SelectMany(p => p.Inputs))
            {
                if (!unspents.ContainsKey(input.PrevHash))
                {
                    Slice value = new byte[0];
                    db.TryGet(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.IX_Unspent).Add(input.PrevHash), out value);
                    unspents.Add(input.PrevHash, new HashSet<uint>(value.ToArray().GetUInt32Array()));
                }
                unspents[input.PrevHash].Remove(input.PrevIndex);
            }
            foreach (var unspent in unspents)
            {
                if (unspent.Value.Count == 0)
                {
                    batch.Delete(SliceBuilder.Begin(DataEntryPrefix.IX_Unspent).Add(unspent.Key));
                }
                else
                {
                    batch.Put(SliceBuilder.Begin(DataEntryPrefix.IX_Unspent).Add(unspent.Key), unspent.Value.ToByteArray());
                }
            }
            current_block_hash = block.Hash;
            current_block_height = block.Hash == GenesisBlock.Hash ? 0 : current_block_height + 1;
            batch.Put(SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentBlock), SliceBuilder.Begin().Add(block.Hash).Add(current_block_height));
            db.Write(WriteOptions.Default, batch);
        }

        private void PersistBlocks()
        {
            while (!disposed)
            {
                while (!disposed)
                {
                    UInt256 hash;
                    lock (header_chain)
                    {
                        TreeNode<Block> node = header_chain.Nodes[current_block_hash];
                        if (header_index.Count <= node.Height + 1) break;
                        hash = header_index[(int)node.Height + 1];
                    }
                    Block block;
                    lock (block_cache)
                    {
                        if (!block_cache.ContainsKey(hash)) break;
                        block = block_cache[hash];
                    }
                    Persist(block);
                    OnPersistCompleted(block);
                    lock (block_cache)
                    {
                        block_cache.Remove(hash);
                    }
                }
                for (int i = 0; i < 50 && !disposed; i++)
                {
                    Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// 将区块链的状态回滚到指定的位置
        /// </summary>
        /// <param name="hash">
        /// 要回滚到的区块的散列值
        /// </param>
        private void Rollback(UInt256 hash)
        {
            if (hash == current_block_hash) return;
            List<Block> blocks = new List<Block>();
            UInt256 current = current_block_hash;
            while (current != hash)
            {
                if (current == GenesisBlock.Hash)
                    throw new InvalidOperationException();
                Block block = GetBlockInternal(current, ReadOptions.Default);
                blocks.Add(block);
                current = block.PrevBlock;
            }
            WriteBatch batch = new WriteBatch();
            foreach (Block block in blocks)
            {
                batch.Delete(SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(block.Hash));
                foreach (Transaction tx in block.Transactions)
                {
                    batch.Delete(SliceBuilder.Begin(DataEntryPrefix.DATA_Transaction).Add(tx.Hash));
                    batch.Delete(SliceBuilder.Begin(DataEntryPrefix.IX_Unspent).Add(tx.Hash));
                }
            }
            HashSet<UInt256> tx_hashes = new HashSet<UInt256>(blocks.SelectMany(p => p.Transactions).Select(p => p.Hash));
            foreach (var group in blocks.SelectMany(p => p.Transactions.Skip(1)).SelectMany(p => p.Inputs).GroupBy(p => p.PrevHash).Where(g => !tx_hashes.Contains(g.Key)))
            {
                Transaction tx = GetTransaction(group.Key, ReadOptions.Default);
                Slice value = new byte[0];
                db.TryGet(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.IX_Unspent).Add(tx.Hash), out value);
                IEnumerable<uint> indexes = value.ToArray().GetUInt32Array().Union(group.Select(p => p.PrevIndex));
                batch.Put(SliceBuilder.Begin(DataEntryPrefix.IX_Unspent).Add(tx.Hash), indexes.ToByteArray());
            }
            current_block_hash = current;
            current_block_height -= (uint)blocks.Count;
            batch.Put(SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentBlock), SliceBuilder.Begin().Add(current_block_hash).Add(current_block_height));
            db.Write(WriteOptions.Default, batch);
        }

        private bool Verify(Block block)
        {
            return block.Bits == GetNextWorkRequired(block.PrevBlock, header_chain.Nodes[block.PrevBlock].Height);
        }
    }
}
