using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace CadEye_WebVersion.Infrastructure.Utils
{
    public static class SystemIconProvider
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHGetStockIconInfo(uint siid, uint uFlags, ref SHSTOCKICONINFO psii);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHSTOCKICONINFO
        {
            public uint cbSize;
            public IntPtr hIcon;
            public int iSysImageIndex;
            public int iIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szPath;
        }

        private const uint SHGSI_ICON = 0x000000100;
        private const uint SIID_FOLDER = 3; // 기본 폴더

        public static ImageSource FolderIcon()
        {
            SHSTOCKICONINFO sii = new SHSTOCKICONINFO();
            sii.cbSize = (uint)Marshal.SizeOf(typeof(SHSTOCKICONINFO));
            SHGetStockIconInfo(SIID_FOLDER, SHGSI_ICON, ref sii);

            BitmapSource bmp = Imaging.CreateBitmapSourceFromHIcon(
                sii.hIcon,
                Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(32, 32));

            bmp.Freeze();
            return bmp;
        }
    }
}
