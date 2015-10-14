using IceWallet.Wallets;
using System.Windows.Forms;

namespace IceWallet.Client
{
    internal partial class ViewPrivateKeyDialog : Form
    {
        public ViewPrivateKeyDialog(WalletEntry entry)
        {
            InitializeComponent();
            textBox3.Text = entry.Address;
            using (entry.Decrypt())
            {
                textBox1.Text = entry.PrivateKey.ToHexString();
            }
            textBox2.Text = entry.Export();
        }
    }
}
