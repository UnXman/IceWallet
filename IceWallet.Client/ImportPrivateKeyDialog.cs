using IceWallet.Cryptography;
using IceWallet.Wallets;
using System;
using System.Linq;
using System.Windows.Forms;

namespace IceWallet.Client
{
    internal partial class ImportPrivateKeyDialog : Form
    {
        public ImportPrivateKeyDialog()
        {
            InitializeComponent();
        }

        public string WIF
        {
            get
            {
                return textBox1.Text;
            }
            set
            {
                textBox1.Text = value;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            foreach (char c in textBox1.Text)
            {
                if (!Base58.Alphabet.Contains(c))
                {
                    button1.Enabled = false;
                    return;
                }
            }
            bool compressed;
            try
            {
                Wallet.GetPrivateKeyFromWIF(textBox1.Text, out compressed);
            }
            catch
            {
                button1.Enabled = false;
                return;
            }
            button1.Enabled = true;
        }
    }
}
