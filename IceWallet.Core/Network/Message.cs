﻿using IceWallet.Cryptography;
using IceWallet.IO;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace IceWallet.Network
{
    internal class Message : ISerializable
    {
        public const uint Magic = 0xD9B4BEF9;
        public string Command;
        public uint Checksum;
        public byte[] Payload;

        public static Message Create(string command, ISerializable payload = null)
        {
            return Create(command, payload == null ? new byte[0] : payload.ToArray());
        }

        public static Message Create(string command, byte[] payload)
        {
            return new Message
            {
                Command = command,
                Checksum = GetChecksum(payload),
                Payload = payload
            };
        }

        void ISerializable.Deserialize(BinaryReader reader)
        {
            if (reader.ReadUInt32() != Magic)
                throw new FormatException();
            this.Command = reader.ReadFixedString(12);
            uint length = reader.ReadUInt32();
            if (length > 0x02000000)
                throw new FormatException();
            this.Checksum = reader.ReadUInt32();
            this.Payload = reader.ReadBytes((int)length);
            if (GetChecksum(Payload) != Checksum)
                throw new FormatException();
        }

        public static async Task<Message> DeserializeFromStreamAsync(Stream stream, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[sizeof(uint) + 12 + sizeof(uint) + sizeof(uint)];
            await ReadAsync(stream, buffer, 0, buffer.Length, cancellationToken);
            Message message = new Message();
            using (MemoryStream ms = new MemoryStream(buffer, false))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                if (reader.ReadUInt32() != Magic)
                    throw new FormatException();
                message.Command = reader.ReadFixedString(12);
                uint length = reader.ReadUInt32();
                if (length > 0x02000000)
                    throw new FormatException();
                message.Checksum = reader.ReadUInt32();
                message.Payload = new byte[length];
            }
            await ReadAsync(stream, message.Payload, 0, message.Payload.Length, cancellationToken);
            if (GetChecksum(message.Payload) != message.Checksum)
                throw new FormatException();
            return message;
        }

        private static uint GetChecksum(byte[] value)
        {
            return BitConverter.ToUInt32(value.Sha256().Sha256(), 0);
        }

        private static async Task ReadAsync(Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            while (count > 0)
            {
                int total = await stream.ReadAsync(buffer, offset, count, cancellationToken);
                if (total == 0) throw new IOException();
                offset += total;
                count -= total;
            }
        }

        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(Magic);
            writer.WriteFixedString(Command, 12);
            writer.Write(Payload.Length);
            writer.Write(Checksum);
            writer.Write(Payload);
        }
    }
}
