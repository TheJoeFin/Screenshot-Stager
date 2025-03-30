using System.Runtime.InteropServices;

namespace Screenshot_Stager;
public static partial class NativeMethods
{
    [LibraryImport("user32.dll")]
    public static partial BOOL ShowWindow(HWND hWnd, int nCmdShow);

    [DllImport("USER32.DLL")]
    public static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

    [LibraryImport("user32.dll")]
    public static partial BOOL IsWindowVisible(HWND hWnd);

    [LibraryImport("user32.dll")]
    public static partial int GetWindowText(HWND hWnd, LPWSTR lpString, int nMaxCount);

    [LibraryImport("user32.dll")]
    public static partial int GetWindowTextLength(HWND hWnd);

    [LibraryImport("user32.dll")]
    public static partial HWND GetShellWindow();

    [LibraryImport("user32.dll")]
    public static partial BOOL SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

    [LibraryImport("user32.dll")]
    public static partial HWND GetAncestor(HWND hwnd, GetAncestorFlags gaFlags);

    [LibraryImport("user32.dll")]
    public static partial long GetWindowLong(HWND hWnd, int nIndex);

    [LibraryImport("user32.dll")]
    public static partial HMONITOR MonitorFromWindow(HWND hwnd, MonitorDefaultTo dwFlags);

    [LibraryImport("user32.dll")]
    public static partial BOOL GetMonitorInfoW(HMONITOR hMonitor, ref MONITORINFO lpmi);

    [LibraryImport("user32.dll")]
    public static partial UINT GetDpiForWindow(HWND hwnd);

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool DeleteObject(IntPtr hObject);

    #region Delegates

    // Define the delegate used for EnumWindows callback
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate BOOL WNDENUMPROC(HWND hwnd, LPARAM lParam);

    public delegate bool EnumWindowsProc(HWND hWnd, int lParam);

    #endregion

    #region Structs

    // MONITORINFO structure used by GetMonitorInfoW
    [StructLayout(LayoutKind.Sequential)]
    public struct MONITORINFO
    {
        public uint cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    // RECT structure for storing rectangle coordinates
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    // Handle to a monitor
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct HMONITOR
    {
        public readonly IntPtr Value;
        public HMONITOR(IntPtr value) => Value = value;
    }

    // LPARAM used for passing data to callbacks
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct LPARAM
    {
        public readonly IntPtr Value;
        public LPARAM(IntPtr value) => Value = value;
        public LPARAM(int value) => Value = new IntPtr(value);
        public static implicit operator LPARAM(int value) => new(value);
        public static implicit operator LPARAM(IntPtr value) => new(value);
    }

    // LPWSTR used for string parameters
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct LPWSTR
    {
        private readonly IntPtr value;

        public LPWSTR(IntPtr value) => this.value = value;
        public  unsafe LPWSTR(char* ptr) => value = new IntPtr(ptr);

        public override string ToString()
        {
            if (value == IntPtr.Zero)
                return string.Empty;

            return Marshal.PtrToStringUni(value) ?? string.Empty;
        }
    }

    // UINT structure for DPI values
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct UINT
    {
        public readonly uint Value;
        public UINT(uint value) => Value = value;
        public static implicit operator uint(UINT value) => value.Value;
        public static implicit operator UINT(uint value) => new(value);
    }

    #endregion

    #region Enums

    // GetAncestorFlags enum for GetAncestor function
    public enum GetAncestorFlags : uint
    {
        GA_PARENT = 1,
        GA_ROOT = 2,
        GA_ROOTOWNER = 3
    }

    // MonitorDefaultTo enum for MonitorFromWindow function
    public enum MonitorDefaultTo : uint
    {
        MONITOR_DEFAULTTONULL = 0,
        MONITOR_DEFAULTTOPRIMARY = 1,
        MONITOR_DEFAULTTONEAREST = 2
    }

    // SetWindowPosFlags enum for SetWindowPos function
    [Flags]
    public enum SetWindowPosFlags : uint
    {
        SWP_NOSIZE = 0x0001,
        SWP_NOMOVE = 0x0002,
        SWP_NOZORDER = 0x0004,
        SWP_NOREDRAW = 0x0008,
        SWP_NOACTIVATE = 0x0010,
        SWP_FRAMECHANGED = 0x0020,
        SWP_SHOWWINDOW = 0x0040,
        SWP_HIDEWINDOW = 0x0080,
        SWP_NOCOPYBITS = 0x0100,
        SWP_NOOWNERZORDER = 0x0200,
        SWP_NOSENDCHANGING = 0x0400,
        SWP_DRAWFRAME = SWP_FRAMECHANGED,
        SWP_NOREPOSITION = SWP_NOOWNERZORDER,
        SWP_DEFERERASE = 0x2000,
        SWP_ASYNCWINDOWPOS = 0x4000
    }

    // Window Long pointer index constants
    public static class WINDOW_LONG_PTR_INDEX
    {
        public const int GWL_EXSTYLE = -20;
        public const int GWL_STYLE = -16;
        public const int GWL_ID = -12;
        public const int GWL_HWNDPARENT = -8;
        public const int GWL_HINSTANCE = -6;
        public const int GWL_USERDATA = -21;
        public const int GWL_WNDPROC = -4;
    }
    // ShowWindow command constants
    public static class SHOW_WINDOW_CMD
    {
        public const int SW_HIDE = 0;
        public const int SW_NORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_MAXIMIZE = 3;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOW = 5;
        public const int SW_MINIMIZE = 6;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNA = 8;
        public const int SW_RESTORE = 9;
        public const int SW_SHOWDEFAULT = 10;
        public const int SW_FORCEMINIMIZE = 11;
    }

    // Special HWND constants
    public static readonly HWND HWND_TOPMOST = new(-1);
    public static readonly HWND HWND_NOTOPMOST = new(-2);
    public static readonly HWND HWND_TOP = new(0);
    public static readonly HWND HWND_BOTTOM = new(1);

    // Monitor flags for MONITORINFO
    public static class MONITORINFO_FLAGS
    {
        public const uint MONITORINFOF_PRIMARY = 0x00000001;
    }

    // PWSTR used for wide character string parameters
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct PWSTR
    {
        private readonly IntPtr value;

        public PWSTR(IntPtr value) => this.value = value;
        public unsafe PWSTR(char* ptr) => value = new IntPtr(ptr);

        public override string ToString()
        {
            if (value == IntPtr.Zero)
                return string.Empty;

            return Marshal.PtrToStringUni(value) ?? string.Empty;
        }
    }

    // Handle to a GDI object
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct HGDIOBJ
    {
        public readonly IntPtr Value;

        public HGDIOBJ(IntPtr value) => Value = value;

        public static implicit operator IntPtr(HGDIOBJ h) => h.Value;
        public static implicit operator HGDIOBJ(IntPtr value) => new(value);

        public static bool operator ==(HGDIOBJ left, HGDIOBJ right) => left.Value == right.Value;
        public static bool operator !=(HGDIOBJ left, HGDIOBJ right) => left.Value != right.Value;

        public override bool Equals(object? obj) => obj is HGDIOBJ h && Value == h.Value;
        public override int GetHashCode() => Value.GetHashCode();
    }

    // Handle to a window
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct HWND
    {
        public readonly IntPtr Value;

        public HWND(IntPtr value) => Value = value;
        public HWND(int value) => Value = new IntPtr(value);

        public static implicit operator IntPtr(HWND hwnd) => hwnd.Value;
        public static explicit operator HWND(IntPtr value) => new HWND(value);

        public static bool operator ==(HWND left, HWND right) => left.Value == right.Value;
        public static bool operator !=(HWND left, HWND right) => left.Value != right.Value;

        public override bool Equals(object? obj) => obj is HWND hwnd && Value == hwnd.Value;
        public override int GetHashCode() => Value.GetHashCode();
    }

    // Boolean return value from functions
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct BOOL
    {
        public readonly int Value;

        public BOOL(int value) => Value = value;
        public BOOL(bool value) => Value = value ? 1 : 0;

        public static implicit operator bool(BOOL value) => value.Value != 0;
        public static implicit operator BOOL(bool value) => new BOOL(value);
        public static implicit operator int(BOOL value) => value.Value;
        public static implicit operator BOOL(int value) => new BOOL(value);

        public override string ToString() => Value != 0 ? bool.TrueString : bool.FalseString;
    }

}

#endregion
