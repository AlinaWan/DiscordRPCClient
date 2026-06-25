using System.Windows;
using System.Windows.Controls;

namespace DiscordRPC.UI
{
    internal static class ScrollbarStyle
    {
        public static void Apply(Grid root)
        {
            var dict = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/DiscordRPCManager;component/UI/Scrollbar.xaml")
            };

            root.Resources.MergedDictionaries.Add(dict);
        }
    }
}