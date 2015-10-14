using IceWallet.Core;
using IceWallet.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IceWallet.Network.Payloads
{
    internal class HeadersPayload : ISerializable
    {
        public Block[] Headers;

        public static HeadersPayload Create(IEnumerable<Block> headers)
        {
            return new HeadersPayload
            {
                Headers = headers.Select(p => p.Header).ToArray()
            };
        }

        void ISerializable.Deserialize(BinaryReader reader)
        {
            this.Headers = reader.ReadSerializableArray<Block>();
            if (Headers.Any(p => !p.IsHeader))
                throw new FormatException();
        }

        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(Headers);
        }
    }
}
