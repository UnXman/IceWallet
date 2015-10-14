using System.Threading.Tasks;

namespace IceWallet.Threading
{
    internal static class Helper
    {
        public static async void Void(this Task task)
        {
            await task;
        }
    }
}
