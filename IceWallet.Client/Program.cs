using IceWallet.Client.Properties;
using IceWallet.Core;
using IceWallet.Implementations.Blockchains.LevelDB;
using IceWallet.Network;
using System;
using System.IO;
using System.Windows.Forms;

namespace IceWallet.Client
{
    static class Program
    {
        public static LocalNode LocalNode;
        public static MainForm MainForm;

        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            WindowsFormsSynchronizationContext.AutoInstall = false;
            MainForm = new MainForm();
            const string path = "nodes.dat";
            if (File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    LocalNode.LoadState(fs);
                }
            }
            using (LevelDBBlockchain blockchain = new LevelDBBlockchain(Settings.Default.ChainPath))
            using (LocalNode = new LocalNode())
            {
                Blockchain.RegisterBlockchain(blockchain);
                LocalNode.Start();
                Application.Run(MainForm);
            }
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                LocalNode.SaveState(fs);
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
#if DEBUG
            Exception ex = (Exception)e.ExceptionObject;
            File.WriteAllText("error.log", $"{ex.Message}\r\n{ex.StackTrace}");
#endif
        }
    }
}
