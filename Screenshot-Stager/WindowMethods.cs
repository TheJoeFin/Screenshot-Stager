using System.Runtime.InteropServices;
using System.Windows;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;

namespace Screenshot_Stager;

public static partial class WindowMethods
{
    internal static IDictionary<HWND, string> GetOpenWindows()
    {
        HWND shellWindow = GetShellWindow();
        Dictionary<HWND, string> windows = [];

        EnumWindows(delegate (HWND hWnd, LPARAM lParam) 
        {
            if (hWnd == shellWindow) return true;
            if (!IsWindowVisible(hWnd)) return true;

            int length = GetWindowTextLength(hWnd);
            if (length == 0) return true;

            unsafe
            {
                fixed (char* title = stackalloc char[length + 1])
                {
                    PWSTR windowText = new(title);
                    _ = GetWindowText(hWnd, windowText, length + 1);

                    bool isAltTab = IsWindowInAltTab(hWnd, windowText.ToString());
                    if (!isAltTab) return true;

                    windows[hWnd] = windowText.ToString();
                }
            }
            return true;

        }, 0);

        return windows;
    }

    internal static double GetScaleForHwnd(HWND hWnd)
    {
        return 96.0 / GetDpiForWindow(hWnd);
    }

    internal static void ChangeSize(HWND hWnd, int x, int y, int width, int height, bool topMost = true)
    {
        _ = ShowWindow(hWnd, SHOW_WINDOW_CMD.SW_NORMAL);
        HWND topMostFlag = topMost ? HWND_TOPMOST : HWND_NOTOPMOST;
        _ = SetWindowPos(hWnd, topMostFlag, x, y, width, height, SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
    }

    private delegate bool EnumWindowsProc(HWND hWnd, LPARAM lParam);

    internal static readonly HWND HWND_TOPMOST = new(-1);
    internal static readonly HWND HWND_NOTOPMOST = new(-2);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_APPWINDOW = 0x00040000;
    private const int WS_EX_NOREDIRECTIONBITMAP = 0x00200000;

    private static bool IsWindowInAltTab(HWND hWnd, string windowName)
    {
        //// Check if the window is visible
        //if (!IsWindowVisible(hWnd))
        //    return false;

        // Check if the window is a top-level window
        IntPtr root = GetAncestor(hWnd, GET_ANCESTOR_FLAGS.GA_ROOT);
        if (root != hWnd)
            return false;

        // Get the extended window styles
        int exStyle = GetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
        string exStyleHexString = exStyle.ToString("X");

        // Exclude tool windows
        if ((exStyle & WS_EX_TOOLWINDOW) != 0)
            return false;

        // UWP apps break here
        if (exStyle is WS_EX_NOREDIRECTIONBITMAP)
            return false;

        // Include app windows or those without the tool window style
        // if ((exStyle & WS_EX_APPWINDOW) == 0 && (exStyle & WS_EX_TOOLWINDOW) == 0)
        //     return false;

        return true;
    }

    internal static Rect GetMonitorRect(HWND handle)
    {
        // get monitor for the window
        HMONITOR hMonitor = MonitorFromWindow(handle, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        MONITORINFO monitorInfo = new() { cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFO)) };

        // apply offsets to x and y based on monitor
        if (GetMonitorInfo(hMonitor, ref monitorInfo))
        {
             return new Rect(
                monitorInfo.rcMonitor.left,
                monitorInfo.rcMonitor.top,
                monitorInfo.rcMonitor.right - monitorInfo.rcMonitor.left,
                monitorInfo.rcMonitor.bottom - monitorInfo.rcMonitor.top);
        }

        return new();
    }
}
