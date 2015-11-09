﻿using IceWallet.Core;
using IceWallet.IO;
using IceWallet.IO.Caching;
using IceWallet.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IceWallet.Network
{
    public class LocalNode : IDisposable
    {
        public event EventHandler<Inventory> NewInventory;

        public const uint PROTOCOL_VERSION = 70002;
        internal const int CONNECTED_MAX = 50;
        private const int PENDING_MAX = CONNECTED_MAX;
        private const int UNCONNECTED_MAX = 5000;
        public const int DEFAULT_PORT = 8333;

        private static readonly string[] SeedList =
        {
            "seed.bitcoin.sipa.be",
            "dnsseed.bluematt.me",
            "dnsseed.bitcoin.dashjr.org",
            "seed.bitcoinstats.com",
            "bitseed.xf2.org"
        };

        internal readonly RelayCache RelayCache = new RelayCache(100);

        private static readonly HashSet<IPEndPoint> unconnectedPeers = new HashSet<IPEndPoint>();
        private static readonly HashSet<IPEndPoint> badPeers = new HashSet<IPEndPoint>();
        internal readonly HashSet<RemoteNode> pendingPeers = new HashSet<RemoteNode>();
        internal readonly Dictionary<IPEndPoint, RemoteNode> connectedPeers = new Dictionary<IPEndPoint, RemoteNode>();

        internal readonly IPEndPoint LocalEndpoint;
        private readonly TcpListener listener;
        private Thread connectThread;
        private int started = 0;
        private int disposed = 0;

        public bool GlobalMissionsEnabled { get; set; } = true;
        public int RemoteNodeCount => connectedPeers.Count;
        public bool ServiceEnabled { get; set; } = true;

        public LocalNode(int port = DEFAULT_PORT)
        {
            IPAddress ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(p => p.AddressFamily == AddressFamily.InterNetwork).MapToIPv6();
            this.LocalEndpoint = new IPEndPoint(ip, port);
            this.listener = new TcpListener(IPAddress.Any, port);
            this.connectThread = new Thread(ConnectToPeersLoop)
            {
                IsBackground = true,
                Name = "LocalNode.ConnectToPeersLoop"
            };
        }

        public async Task ConnectToPeerAsync(string hostNameOrAddress)
        {
            IPHostEntry entry;
            try
            {
                entry = await Dns.GetHostEntryAsync(hostNameOrAddress);
            }
            catch (SocketException)
            {
                return;
            }
            IPAddress ipAddress = entry.AddressList.FirstOrDefault()?.MapToIPv6();
            if (ipAddress == null) return;
            await ConnectToPeerAsync(new IPEndPoint(ipAddress, DEFAULT_PORT));
        }

        public async Task ConnectToPeerAsync(IPEndPoint remoteEndpoint)
        {
            if (remoteEndpoint.Equals(LocalEndpoint)) return;
            RemoteNode remoteNode;
            lock (unconnectedPeers)
            {
                unconnectedPeers.Remove(remoteEndpoint);
            }
            lock (pendingPeers)
            {
                lock (connectedPeers)
                {
                    if (pendingPeers.Any(p => p.RemoteEndpoint == remoteEndpoint) || connectedPeers.ContainsKey(remoteEndpoint))
                        return;
                }
                remoteNode = new RemoteNode(this, remoteEndpoint);
                pendingPeers.Add(remoteNode);
                remoteNode.Disconnected += RemoteNode_Disconnected;
                remoteNode.PeersReceived += RemoteNode_PeersReceived;
                remoteNode.BlockReceived += RemoteNode_BlockReceived;
                remoteNode.TransactionReceived += RemoteNode_TransactionReceived;
            }
            await remoteNode.ConnectAsync();
        }

        private void ConnectToPeersLoop()
        {
            while (disposed == 0)
            {
                int connectedCount = connectedPeers.Count;
                int pendingCount = pendingPeers.Count;
                int unconnectedCount = unconnectedPeers.Count;
                int maxConnections = Math.Max(CONNECTED_MAX + CONNECTED_MAX / 5, PENDING_MAX);
                if (connectedCount < CONNECTED_MAX && pendingCount < PENDING_MAX && (connectedCount + pendingCount) < maxConnections)
                {
                    Task[] tasks;
                    if (unconnectedCount > 0)
                    {
                        IPEndPoint[] endpoints;
                        lock (unconnectedPeers)
                        {
                            endpoints = unconnectedPeers.Take(maxConnections - (connectedCount + pendingCount)).ToArray();
                        }
                        tasks = endpoints.Select(p => ConnectToPeerAsync(p)).ToArray();
                    }
                    else if (connectedCount > 0)
                    {
                        lock (connectedPeers)
                        {
                            tasks = connectedPeers.Values.ToArray().Select(p => p.RequestPeersAsync()).ToArray();
                        }
                    }
                    else
                    {
                        tasks = SeedList.Select(p => ConnectToPeerAsync(p)).ToArray();
                    }
                    Task.WaitAll(tasks);
                }
                for (int i = 0; i < 50 && disposed == 0; i++)
                {
                    Thread.Sleep(100);
                }
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) == 0)
            {
                if (started > 0)
                {
                    listener.Stop();
                    connectThread.Join();
                    lock (unconnectedPeers)
                    {
                        if (unconnectedPeers.Count < UNCONNECTED_MAX)
                        {
                            lock (connectedPeers)
                            {
                                unconnectedPeers.UnionWith(connectedPeers.Keys.Take(UNCONNECTED_MAX - unconnectedPeers.Count));
                            }
                        }
                    }
                    RemoteNode[] nodes;
                    lock (connectedPeers)
                    {
                        nodes = connectedPeers.Values.ToArray();
                    }
                    Task.WaitAll(nodes.Select(p => Task.Run(() => p.Disconnect(false))).ToArray());
                }
            }
        }

        public RemoteNode[] GetRemoteNodes()
        {
            lock (connectedPeers)
            {
                return connectedPeers.Values.ToArray();
            }
        }

        public static void LoadState(Stream stream)
        {
            unconnectedPeers.Clear();
            using (BinaryReader reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    IPAddress address = new IPAddress(reader.ReadBytes(4));
                    int port = reader.ReadUInt16();
                    unconnectedPeers.Add(new IPEndPoint(address.MapToIPv6(), port));
                }
            }
        }

        public async Task<bool> RelayAsync(Inventory data)
        {
            bool result = await RelayInternalAsync(data);
            if (data is Block)
            {
                if (Blockchain.Default != null && !Blockchain.Default.ContainsBlock(data.Hash) && Blockchain.Default.AddBlock(data as Block))
                {
                    if (NewInventory != null) NewInventory(this, data);
                }
            }
            else if (data is Transaction)
            {
                if (Blockchain.Default != null && !Blockchain.Default.ContainsTransaction(data.Hash) && Blockchain.Default.AddTransaction(data as Transaction))
                {
                    if (NewInventory != null) NewInventory(this, data);
                }
            }
            return result;
        }

        private async Task<bool> RelayInternalAsync(Inventory data)
        {
            if (connectedPeers.Count == 0) return false;
            RemoteNode[] remoteNodes;
            lock (connectedPeers)
            {
                if (connectedPeers.Count == 0) return false;
                remoteNodes = connectedPeers.Values.ToArray();
            }
            if (remoteNodes.Length == 0) return false;
            RelayCache.Add(data);
            await Task.WhenAny(remoteNodes.Select(p => p.RelayAsync(data)));
            return true;
        }

        private void RemoteNode_BlockReceived(object sender, Block block)
        {
            if (Blockchain.Default == null) return;
            if (Blockchain.Default.ContainsBlock(block.Hash)) return;
            if (!Blockchain.Default.AddBlock(block)) return;
            RelayInternalAsync(block).Void();
            if (NewInventory != null) NewInventory(this, block);
        }

        private void RemoteNode_Disconnected(object sender, bool error)
        {
            RemoteNode remoteNode = (RemoteNode)sender;
            remoteNode.Disconnected -= RemoteNode_Disconnected;
            remoteNode.PeersReceived -= RemoteNode_PeersReceived;
            remoteNode.BlockReceived -= RemoteNode_BlockReceived;
            remoteNode.TransactionReceived -= RemoteNode_TransactionReceived;
            if (error)
            {
                lock (badPeers)
                {
                    badPeers.Add(remoteNode.RemoteEndpoint);
                }
            }
            lock (unconnectedPeers)
            {
                lock (pendingPeers)
                {
                    lock (connectedPeers)
                    {
                        unconnectedPeers.Remove(remoteNode.RemoteEndpoint);
                        pendingPeers.Remove(remoteNode);
                        if (remoteNode.RemoteEndpoint != null)
                            connectedPeers.Remove(remoteNode.RemoteEndpoint);
                    }
                }
            }
        }

        private void RemoteNode_PeersReceived(object sender, IPEndPoint[] peers)
        {
            lock (unconnectedPeers)
            {
                if (unconnectedPeers.Count < UNCONNECTED_MAX)
                {
                    lock (badPeers)
                    {
                        lock (pendingPeers)
                        {
                            lock (connectedPeers)
                            {
                                unconnectedPeers.UnionWith(peers);
                                unconnectedPeers.ExceptWith(badPeers);
                                unconnectedPeers.ExceptWith(pendingPeers.Select(p => p.RemoteEndpoint));
                                unconnectedPeers.ExceptWith(connectedPeers.Keys);
                            }
                        }
                    }
                }
            }
        }

        private void RemoteNode_TransactionReceived(object sender, Transaction tx)
        {
            if (Blockchain.Default == null) return;
            if (Blockchain.Default.ContainsTransaction(tx.Hash)) return;
            if (!Blockchain.Default.AddTransaction(tx)) return;
            RelayInternalAsync(tx).Void();
            if (NewInventory != null) NewInventory(this, tx);
        }

        public static void SaveState(Stream stream)
        {
            IPEndPoint[] peers;
            lock (unconnectedPeers)
            {
                peers = unconnectedPeers.Take(UNCONNECTED_MAX).ToArray();
            }
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                writer.Write(peers.Length);
                foreach (IPEndPoint endpoint in peers)
                {
                    writer.Write(endpoint.Address.MapToIPv4().GetAddressBytes());
                    writer.Write((ushort)endpoint.Port);
                }
            }
        }

        public async void Start()
        {
            if (Interlocked.Exchange(ref started, 1) == 0)
            {
                connectThread.Start();
                listener.Start();
                while (disposed == 0)
                {
                    TcpClient tcp;
                    try
                    {
                        tcp = await listener.AcceptTcpClientAsync();
                    }
                    catch (ObjectDisposedException)
                    {
                        continue;
                    }
                    RemoteNode remoteNode = new RemoteNode(this, tcp);
                    lock (pendingPeers)
                    {
                        pendingPeers.Add(remoteNode);
                    }
                    remoteNode.StartProtocol();
                }
            }
        }
    }
}
