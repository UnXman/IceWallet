using System.Collections.ObjectModel;

namespace IceWallet.Wallets
{
    public class WalletEntryCollection : KeyedCollection<UInt160, WalletEntry>
    {
        protected override UInt160 GetKeyForItem(WalletEntry entry)
        {
            return entry.PublicKeyHash;
        }
    }
}
