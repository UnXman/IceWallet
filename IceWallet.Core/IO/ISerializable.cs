using System.IO;

namespace IceWallet.IO
{
    public interface ISerializable
    {
        void Deserialize(BinaryReader reader);
        void Serialize(BinaryWriter writer);
    }
}
