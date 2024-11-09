using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using HWND = System.IntPtr;

namespace Screenshot_Stager;

/// <summary>Contains functionality to get all the open windows.</summary>
public static partial class WindowMethods
{
    /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
    /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
    public static IDictionary<HWND, string> GetOpenWindows()
    {
        HWND shellWindow = GetShellWindow();
        Dictionary<HWND, string> windows = [];

        EnumWindows(delegate (HWND hWnd, int lParam)
        {
            if (hWnd == shellWindow) return true;
            if (!IsWindowVisible(hWnd)) return true;

            int length = GetWindowTextLength(hWnd);
            if (length == 0) return true;

            StringBuilder builder = new(length);
            _ = GetWindowText(hWnd, builder, length + 1);

            bool isAltTab = IsWindowInAltTab(hWnd, builder.ToString());
            if (!isAltTab) return true;

            windows[hWnd] = builder.ToString();
            return true;

        }, 0);

        return windows;
    }

    public static double GetScaleForHwnd(HWND hWnd)
    {
        return 96.0 / GetDpiForWindow(hWnd);
    }

    public static void ChangeSize(HWND hWnd, int x, int y, int width, int height, bool topMost = true)
    {
        _ = ShowWindow(hWnd, SW_SHOWNORMAL);
        IntPtr topMostFlag = topMost ? HWND_TOPMOST : HWND_NOTOPMOST;
        _ = SetWindowPos(hWnd, topMostFlag, x, y, width, height, SWP_NOACTIVATE);
    }

    private delegate bool EnumWindowsProc(HWND hWnd, int lParam);

    [DllImport("USER32.DLL")]
    private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

    [DllImport("USER32.DLL")]
    private static extern int GetWindowText(HWND hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("USER32.DLL")]
    private static extern int GetWindowTextLength(HWND hWnd);

    [DllImport("USER32.DLL")]
    private static extern bool IsWindowVisible(HWND hWnd);

    [DllImport("USER32.DLL")]
    private static extern IntPtr GetShellWindow();

    [DllImport("USER32.DLL")]
    private static extern bool SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    internal static readonly IntPtr HWND_TOPMOST = new(-1);
    internal static readonly IntPtr HWND_NOTOPMOST = new(-2);
    internal const uint SWP_NOSIZE = 0x0001;
    internal const uint SWP_NOMOVE = 0x0002;
    internal const uint SWP_NOACTIVATE = 0x0010;
    internal const uint SWP_SHOWWINDOW = 0x0040;
    internal const int SW_SHOWNORMAL = 1;

    [DllImport("USER32.DLL")]
    private static extern uint GetDpiForWindow(HWND hWnd);

    // The DWM_WINDOW_CORNER_PREFERENCE enum for DwmSetWindowAttribute's third parameter, which tells the function
    // what value of the enum to set.
    // Copied from dwmapi.h
    public enum DWM_WINDOW_CORNER_PREFERENCE
    {
        DWMWCP_DEFAULT = 0,
        DWMWCP_DONOTROUND = 1,
        DWMWCP_ROUND = 2,
        DWMWCP_ROUNDSMALL = 3
    }

    // Import dwmapi.dll and define DwmSetWindowAttribute in C# corresponding to the native function.
    [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    internal static extern void DwmSetWindowAttribute(IntPtr hwnd,
                                                     DWMWINDOWATTRIBUTE attribute,
                                                     ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
                                                     uint cbAttribute);

    public enum DWMWINDOWATTRIBUTE
    {
        DWMWA_WINDOW_CORNER_PREFERENCE = 33
    }

    [DllImport("USER32.DLL")]
    private static extern bool ShowWindow(HWND hWnd, int nCmdShow);

    [DllImport("USER32.DLL")]
    public static extern bool GetWindowRect(HWND hWnd, out RECT lpRect);

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

    [DllImport("user32.dll")]
    private static extern IntPtr GetAncestor(IntPtr hWnd, uint gaFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    private const int GWL_EXSTYLE = -20;
    private const int GA_ROOT = 2;

    public static bool IsWindowInAltTab(IntPtr hWnd, string windowName)
    {
        //// Check if the window is visible
        //if (!IsWindowVisible(hWnd))
        //    return false;

        // Check if the window is a top-level window
        IntPtr root = GetAncestor(hWnd, GA_ROOT);
        if (root != hWnd)
            return false;

        // Get the extended window styles
        int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
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
}
