using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace vmPing.Interop
{
    internal static class NativeMethods
    {
        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;
        private const int WS_MINIMIZEBOX = 0x20000;
        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_CAPTION_COLOR = 35;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);
        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        // Call from a window's SourceInitialized event, once its handle exists.
        public static void HideMinimizeAndMaximizeButtons(Window window)
        {
            IntPtr handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero)
            {
                return;
            }

            SetWindowLong(handle, GWL_STYLE, GetWindowLong(handle, GWL_STYLE) & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
        }

        // Makes the title bar dark or light. Windows 11 also takes a caption color.
        // Older versions of Windows ignore what they don't support.
        public static void SetTitleBarTheme(Window window, bool isDark, Color captionColor)
        {
            IntPtr handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero)
            {
                return;
            }

            int useDarkMode = isDark ? 1 : 0;
            if (DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int)) != 0)
            {
                // Windows 10 before version 20H1 uses an older attribute number.
                DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useDarkMode, sizeof(int));
            }

            // Colors are passed as 0x00BBGGRR.
            int caption = captionColor.R | (captionColor.G << 8) | (captionColor.B << 16);
            DwmSetWindowAttribute(handle, DWMWA_CAPTION_COLOR, ref caption, sizeof(int));
        }

        // The work area (the screen minus the taskbar) of the monitor the window is on.
        public static Rect GetWorkArea(Window window)
        {
            PresentationSource source = PresentationSource.FromVisual(window);
            IntPtr handle = new WindowInteropHelper(window).Handle;
            IntPtr monitor = MonitorFromWindow(handle, MONITOR_DEFAULTTONEAREST);
            MONITORINFO info = new MONITORINFO { cbSize = Marshal.SizeOf(typeof(MONITORINFO)) };

            // Use the primary screen if the window isn't shown or the monitor can't be read.
            if (source == null || monitor == IntPtr.Zero || !GetMonitorInfo(monitor, ref info))
            {
                return SystemParameters.WorkArea;
            }

            // Convert from pixels to the units used for window positions.
            Matrix fromPixels = source.CompositionTarget.TransformFromDevice;
            Point topLeft = fromPixels.Transform(new Point(info.rcWork.Left, info.rcWork.Top));
            Point bottomRight = fromPixels.Transform(new Point(info.rcWork.Right, info.rcWork.Bottom));
            return new Rect(topLeft, bottomRight);
        }
    }
}
