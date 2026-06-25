using System.Runtime.InteropServices;

namespace DiscordRPC.Core
{
    internal class NativeMethods
    {
        [DllImport("dwmapi.dll")]
        internal static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int pvAttribute, int cbAttribute);
    }
}
