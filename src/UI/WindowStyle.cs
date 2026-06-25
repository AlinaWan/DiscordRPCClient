using DiscordRPC.Core;

namespace DiscordRPC.UI
{
    internal class WindowStyle
    {
        internal static void SetTitleBarColor(System.Windows.Window window, System.Windows.Media.Color color)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).EnsureHandle();
            int bgrColor = (color.B << 16) | (color.G << 8) | color.R;
            NativeMethods.DwmSetWindowAttribute(hwnd, 35, ref bgrColor, sizeof(int));
        }
    }
}
