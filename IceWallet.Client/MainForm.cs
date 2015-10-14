using IceWallet.Core;
using IceWallet.Wallets;
using System;
using System.Linq;
using System.Windows.Forms;

namespace IceWallet.Client
{
    internal partial class MainForm : Form
    {
        private MyWallet wallet;
        private string wallet_path;
        private string password;

        public MainForm()
        {
            InitializeComponent();
        }

        private void OpenWallet(MyWallet wallet)
        {
            this.wallet = wallet;
            listView1.Items.Clear();
            listView1.Items.AddRange(wallet.GetAddresses().Select(p => new ListViewItem(new[] { p, "" }) { Name = p }).ToArray());
            RefreshWallet();
            创建新地址NToolStripMenuItem.Enabled = true;
            导入私钥IToolStripMenuItem.Enabled = true;
            button1.Enabled = true;
        }

        private void RefreshWallet()
        {
            decimal sum = 0;
            foreach (ListViewItem item in listView1.Items)
            {
                decimal balance = 0; //TODO: find unspent
                item.SubItems[1].Text = balance.ToString();
                sum += balance;
            }
            lbl_balance.Text = $"{sum} BTC";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lbl_height.Text = $"{Blockchain.Default.Height}/{Blockchain.Default.HeaderHeight}";
            lbl_count_node.Text = Program.LocalNode.RemoteNodeCount.ToString();
        }

        private void 新建钱包NToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (CreateWalletDialog dialog = new CreateWalletDialog())
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;
                MyWallet wallet = MyWallet.Create();
                wallet.Save(dialog.WalletPath, dialog.Password);
                wallet_path = dialog.WalletPath;
                password = dialog.Password;
                OpenWallet(wallet);
            }
        }

        private void 打开钱包OToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenWalletDialog dialog = new OpenWalletDialog())
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;
                MyWallet wallet;
                try
                {
                    wallet = MyWallet.Load(dialog.WalletPath, dialog.Password);
                }
                catch
                {
                    MessageBox.Show("密码错误");
                    return;
                }
                wallet_path = dialog.WalletPath;
                password = dialog.Password;
                OpenWallet(wallet);
            }
        }

        private void 退出XToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void 创建新地址NToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WalletEntry entry = wallet.CreateEntry();
            wallet.Save(wallet_path, password);
            listView1.Items.Add(new ListViewItem(new[] { entry.Address, "" }) { Name = entry.Address });
            RefreshWallet();
            listView1.SelectedIndices.Clear();
            listView1.Items[entry.Address].Selected = true;
        }

        private void 导入私钥IToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (ImportPrivateKeyDialog dialog = new ImportPrivateKeyDialog())
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;
                WalletEntry entry = wallet.Import(dialog.WIF);
                wallet.Save(wallet_path, password);
                listView1.Items.Add(new ListViewItem(new[] { entry.Address, "" }) { Name = entry.Address });
                RefreshWallet();
                listView1.SelectedIndices.Clear();
                listView1.Items[entry.Address].Selected = true;
            }
        }

        private void 查看私钥VToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool p2sh;
            WalletEntry entry = wallet.FindEntry(Wallet.ToScriptHash(listView1.SelectedItems[0].Text, out p2sh));
            using (ViewPrivateKeyDialog dialog = new ViewPrivateKeyDialog(entry))
            {
                dialog.ShowDialog();
            }
        }

        private void 复制到剪贴板CToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(listView1.SelectedItems[0].Text);
        }

        private void 删除DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("删除地址后，这些地址中的比特币将永久性地丢失，确认要继续吗？", "删除地址确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;
            string[] addresses = listView1.SelectedItems.OfType<ListViewItem>().Select(p => p.Name).ToArray();
            foreach (string address in addresses)
            {
                bool p2sh;
                listView1.Items.RemoveByKey(address);
                wallet.DeleteEntry(Wallet.ToScriptHash(address, out p2sh));
            }
            wallet.Save(wallet_path, password);
            RefreshWallet();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            删除DToolStripMenuItem.Enabled = listView1.SelectedIndices.Count > 0;
            查看私钥VToolStripMenuItem.Enabled = listView1.SelectedIndices.Count == 1;
            复制到剪贴板CToolStripMenuItem.Enabled = listView1.SelectedIndices.Count == 1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RefreshWallet();
        }
    }
}
