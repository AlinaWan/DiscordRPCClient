using DiscordRPC.Core;
using Application = System.Windows.Application;

namespace DiscordRPC
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            Application app = new Application();
            MainWindow mainWindow = new MainWindow();

            app.Run(mainWindow);
        }
    }
}
