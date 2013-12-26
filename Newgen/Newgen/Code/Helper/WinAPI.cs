using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Newgen.Native
{
    internal static class WinAPI
    {
        internal const int GwlExstyle = -20;

        internal const int GwlStyle = -16;

        internal const uint WM_SETICON = 0x0080;
        internal const uint WM_CLOSE = 0x0010;

        internal const int WS_EX_DLGMODALFRAME = 0x0001;
        internal const int SWP_NOSIZE = 0x0001;
        internal const int SWP_NOMOVE = 0x0002;
        internal const int SWP_NOZORDER = 0x0004;
        internal const int SWP_FRAMECHANGED = 0x0020;
        internal const int SWP_NOACTIVATE = 0x0010;

        internal const int WM_KEYDOWN = 0x0100;
        internal const int WM_KEYUP = 0x0101;
        internal const int WM_CHAR = 0x0102;

        internal const int WsExToolwindow = 0x00000080;

        internal const int Flip3D = 8;

        [StructLayout(LayoutKind.Sequential)]
        internal struct Margins
        {
            internal int cxLeftWidth;      // width of left border that retains its size
            internal int cxRightWidth;     // width of right border that retains its size
            internal int cyTopHeight;      // height of top border that retains its size
            internal int cyBottomHeight;   // height of bottom border that retains its size
        };

        internal struct BbStruct //Blur Behind Structure
        {
            internal BbFlags Flags;
            internal bool Enable;
            internal IntPtr Region;
            internal bool TransitionOnMaximized;
        }

        [Flags]
        internal enum BbFlags : byte //Blur Behind Flags
        {
            DwmBbEnable = 1,
            DwmBbBlurregion = 2,
            DwmBbTransitiononmaximized = 4,
        };

        internal enum Flip3DPolicy
        {
            Default = 0,
            ExcludeBelow,
            ExcludeAbove
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SHFILEINFO
        {
            internal IntPtr hIcon;
            internal int iIcon;
            internal uint dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            internal string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            internal string szTypeName;
        }

        internal const uint SHGFI_ICON = 0x000000100;     // get icon
        internal const uint SHGFI_DISPLAYNAME = 0x000000200;     // get display name
        internal const uint SHGFI_LARGEICON = 0x000000000;     // get large icon
        internal const uint SHGFI_SMALLICON = 0x000000001;     // get small icon

        // Structure contain information about low-level keyboard input event
        [StructLayout(LayoutKind.Sequential)]
        internal struct KBDLLHOOKSTRUCT
        {
            internal int vkCode;
            internal int scanCode;
            internal int flags;
            internal int time;
            internal IntPtr extra;
        }

        //System level functions to be used for hook and unhook keyboard input
        internal delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SetWindowsHookEx(int id, LowLevelKeyboardProc callback, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool UnhookWindowsHookEx(IntPtr hook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wp, IntPtr lp);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr GetModuleHandle(string name);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern short GetAsyncKeyState(int vkCode);

        [DllImport("DwmApi.dll")]
        internal static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins pMarInset);

        [DllImport("dwmapi.dll")]
        internal static extern int DwmEnableBlurBehindWindow(IntPtr hWnd, ref BbStruct blurBehind);

        [DllImport("dwmapi.dll")]
        internal static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        internal static extern int DwmIsCompositionEnabled(out bool enabled);

        [DllImport("user32.dll")]
        internal static extern int GetWindowLong(IntPtr window, int index);

        [DllImport("user32.dll")]
        internal static extern int SetWindowLong(IntPtr window, int index, uint value);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int xradius, int yradius);

        [DllImport("user32.dll")]
        internal static extern IntPtr FindWindowEx(IntPtr parentHwnd, IntPtr childAfterHwnd, IntPtr className, string windowText);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateEllipticRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("kernel32")]
        internal static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern bool FreeConsole();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("shell32.dll")]
        internal static extern IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

        [DllImport("user32.dll")]
        internal static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("shell32")]
        internal static extern int SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        internal static extern int ExitWindowsEx(int uFlags, int dwReason);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsIconic(IntPtr hWnd);

        internal static void RemoveWindowIcon(IntPtr hwnd)
        {
            // Change the extended window style to not show a window icon
            int extendedStyle = GetWindowLong(hwnd, GwlExstyle);
            SetWindowLong(hwnd, GwlExstyle, (uint)extendedStyle | WS_EX_DLGMODALFRAME);

            // Update the window's non-client area to reflect the changes
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE |
                  SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowPlacement")]
        internal static extern bool InternalGetWindowPlacement(IntPtr hWnd, ref WindowPlacement lpwndpl);

        internal static bool GetWindowPlacement(IntPtr hWnd, out WindowPlacement placement)
        {
            placement = new WindowPlacement();
            placement.Length = Marshal.SizeOf(typeof(WindowPlacement));
            return InternalGetWindowPlacement(hWnd, ref placement);
        }

        [DllImport("user32.dll")]
        internal static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCmd uCmd);

        [DllImport("user32.dll")]
        internal static extern IntPtr FindWindow(string classname, string title);

        [DllImport("user32.dll")]
        internal static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        [DllImport("dwmapi.dll")]
        internal static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr source, out IntPtr hthumbnail);

        [DllImport("dwmapi.dll")]
        internal static extern int DwmUnregisterThumbnail(IntPtr HThumbnail);

        [DllImport("dwmapi.dll")]
        internal static extern int DwmUpdateThumbnailProperties(IntPtr HThumbnail, ref ThumbnailProperties props);

        [DllImport("dwmapi.dll")]
        internal static extern int DwmQueryThumbnailSourceSize(IntPtr HThumbnail, out Size size);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, WindowShowStyle nCmdShow);

        internal enum GWL
        {
            GWL_WNDPROC = (-4),
            GWL_HINSTANCE = (-6),
            GWL_HWNDPARENT = (-8),
            GWL_STYLE = (-16),
            GWL_EXSTYLE = (-20),
            GWL_USERDATA = (-21),
            GWL_ID = (-12)
        }

        internal static IntPtr GetWindowLongPtr(HandleRef hWnd, GWL nIndex)
        {
            if(IntPtr.Size == 4)
            {
                return GetWindowLong32(hWnd, nIndex);
            }
            return GetWindowLongPtr64(hWnd, nIndex);
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
        private static extern IntPtr GetWindowLong32(HandleRef hWnd, GWL nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
        private static extern IntPtr GetWindowLongPtr64(HandleRef hWnd, GWL nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        internal static extern IntPtr GetWindowLongPtr(IntPtr hWnd, GWL nIndex);

        [DllImport("user32.dll")]
        internal static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int GetWindowTextLength(IntPtr hWnd);

        internal static string GetText(IntPtr hWnd)
        {
            // Allocate correct string length first
            int length = GetWindowTextLength(hWnd);
            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        internal enum WindowShowStyle : uint
        {
            Hide = 0,
            ShowNormal = 1,
            ShowMinimized = 2,
            ShowMaximized = 3,
            Maximize = 3,
            ShowNormalNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActivate = 7,
            ShowNoActivate = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimized = 11
        }

        internal struct Point
        {
            internal int x;
            internal int y;
        }

        internal struct Size
        {
            internal int Width, Height;
        }

        internal struct WindowPlacement
        {
            internal int Length;
            internal int Flags;
            internal int ShowCmd;
            internal Point MinPosition;
            internal Point MaxPosition;
            internal Rect NormalPosition;
        }

        internal struct ThumbnailProperties
        {
            internal ThumbnailFlags Flags;
            internal Rect Destination;
            internal Rect Source;
            internal Byte Opacity;
            internal bool Visible;
            internal bool SourceClientAreaOnly;
        }

        internal struct Rect
        {
            internal Rect(int x, int y, int x1, int y1)
            {
                this.Left = x;
                this.Top = y;
                this.Right = x1;
                this.Bottom = y1;
            }

            internal int Left, Top, Right, Bottom;
        }

        [Flags]
        internal enum ThumbnailFlags : int
        {
            RectDetination = 1,
            RectSource = 2,
            Opacity = 4,
            Visible = 8,
            SourceClientAreaOnly = 16
        }

        internal enum GetWindowCmd : uint
        {
            First = 0,
            Last = 1,
            Next = 2,
            Prev = 3,
            Owner = 4,
            Child = 5,
            EnabledPopup = 6
        }

        [Flags]
        internal enum WindowStyles : uint
        {
            WS_OVERLAPPED = 0x00000000,
            WS_POPUP = 0x80000000,
            WS_CHILD = 0x40000000,
            WS_MINIMIZE = 0x20000000,
            WS_VISIBLE = 0x10000000,
            WS_DISABLED = 0x08000000,
            WS_CLIPSIBLINGS = 0x04000000,
            WS_CLIPCHILDREN = 0x02000000,
            WS_MAXIMIZE = 0x01000000,
            WS_BORDER = 0x00800000,
            WS_DLGFRAME = 0x00400000,
            WS_VSCROLL = 0x00200000,
            WS_HSCROLL = 0x00100000,
            WS_SYSMENU = 0x00080000,
            WS_THICKFRAME = 0x00040000,
            WS_GROUP = 0x00020000,
            WS_TABSTOP = 0x00010000,

            WS_MINIMIZEBOX = 0x00020000,
            WS_MAXIMIZEBOX = 0x00010000,

            WS_CAPTION = WS_BORDER | WS_DLGFRAME,
            WS_TILED = WS_OVERLAPPED,
            WS_ICONIC = WS_MINIMIZE,
            WS_SIZEBOX = WS_THICKFRAME,
            WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,

            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_CHILDWINDOW = WS_CHILD,
        }

        internal struct LASTINPUTINFO
        {
            internal uint cbSize;

            internal uint dwTime;
        }

        [DllImport("User32.dll")]
        internal static extern bool LockWorkStation();

        [DllImport("User32.dll")]
        internal static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("Kernel32.dll")]
        internal static extern uint GetLastError();

        internal static uint GetIdleTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            GetLastInputInfo(ref lastInPut);

            return ((uint)Environment.TickCount - lastInPut.dwTime);
        }

        internal static long GetTickCount()
        {
            return Environment.TickCount;
        }

        internal static long GetLastInputTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            if(!GetLastInputInfo(ref lastInPut))
            {
                throw new Exception(GetLastError().ToString());
            }

            return lastInPut.dwTime;
        }

        [DllImport("kernel32.dll")]
        public static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

        public static void FlushMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if(Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int GetWindowThreadProcessId(IntPtr handle, out uint processId);

        internal static string GetProcessPath(IntPtr hwnd)
        {
            uint pid = 0;
            GetWindowThreadProcessId(hwnd, out pid);
            Process proc = Process.GetProcessById((int)pid);
            return proc.MainModule.FileName.ToString();
        }

        internal static Process GetProcess(IntPtr hwnd)
        {
            uint pid = 0;
            GetWindowThreadProcessId(hwnd, out pid);
            return Process.GetProcessById((int)pid);
        }

        [DllImport("dwmapi.dll")]
        internal static extern int DwmQueryThumbnailSourceSize(IntPtr thumb, out PSIZE size);

        [StructLayout(LayoutKind.Sequential)]
        internal struct PSIZE
        {
            public int x;
            public int y;
        }

        [DllImport("dwmapi.dll")]
        internal static extern int DwmUpdateThumbnailProperties(IntPtr hThumb, ref DWM_THUMBNAIL_PROPERTIES props);

        [StructLayout(LayoutKind.Sequential)]
        internal struct DWM_THUMBNAIL_PROPERTIES
        {
            public int dwFlags;
            public RECT rcDestination;
            public RECT rcSource;
            public byte opacity;
            public bool fVisible;
            public bool fSourceClientAreaOnly;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        internal static readonly int DWM_TNP_VISIBLE = 0x8;
        internal static readonly int DWM_TNP_RECTDESTINATION = 0x1;
        internal static readonly int DWM_TNP_SOURCECLIENTAREAONLY = 0x00000010;

        [DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
        internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll", SetLastError = true)]
        internal static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateBitmap(int nWidth, int nHeight, uint cPlanes, uint cBitsPerPel, IntPtr lpvBits);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        internal enum TernaryRasterOperations : uint
        {
            /// <summary>dest = source</summary>
            SRCCOPY = 0x00CC0020,

            /// <summary>dest = source OR dest</summary>
            SRCPAINT = 0x00EE0086,

            /// <summary>dest = source AND dest</summary>
            SRCAND = 0x008800C6,

            /// <summary>dest = source XOR dest</summary>
            SRCINVERT = 0x00660046,

            /// <summary>dest = source AND (NOT dest)</summary>
            SRCERASE = 0x00440328,

            /// <summary>dest = (NOT source)</summary>
            NOTSRCCOPY = 0x00330008,

            /// <summary>dest = (NOT src) AND (NOT dest)</summary>
            NOTSRCERASE = 0x001100A6,

            /// <summary>dest = (source AND pattern)</summary>
            MERGECOPY = 0x00C000CA,

            /// <summary>dest = (NOT source) OR dest</summary>
            MERGEPAINT = 0x00BB0226,

            /// <summary>dest = pattern</summary>
            PATCOPY = 0x00F00021,

            /// <summary>dest = DPSnoo</summary>
            PATPAINT = 0x00FB0A09,

            /// <summary>dest = pattern XOR dest</summary>
            PATINVERT = 0x005A0049,

            /// <summary>dest = (NOT dest)</summary>
            DSTINVERT = 0x00550009,

            /// <summary>dest = BLACK</summary>
            BLACKNESS = 0x00000042,

            /// <summary>dest = WHITE</summary>
            WHITENESS = 0x00FF0062
        }

        [DllImport("gdi32.dll")]
        internal static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        public static BitmapSource Capture(System.Windows.Rect area)
        {
            IntPtr screenDC = WinAPI.GetDC(IntPtr.Zero);
            IntPtr memDC = WinAPI.CreateCompatibleDC(screenDC);
            IntPtr hBitmap = WinAPI.CreateCompatibleBitmap(screenDC, (int)SystemParameters.VirtualScreenWidth, (int)SystemParameters.VirtualScreenHeight);
            WinAPI.SelectObject(memDC, hBitmap); // Select bitmap from compatible bitmap to memDC

            // TODO: BitBlt may fail horribly
            WinAPI.BitBlt(memDC, 0, 0, (int)area.Width, (int)area.Height, screenDC, (int)area.X, (int)area.Y, WinAPI.TernaryRasterOperations.SRCCOPY);
            BitmapSource bsource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            WinAPI.DeleteObject(hBitmap);
            WinAPI.ReleaseDC(IntPtr.Zero, screenDC);
            WinAPI.ReleaseDC(IntPtr.Zero, memDC);
            return bsource;
        }

        [DllImport("user32")]
        private static extern IntPtr IsWindowEnabled(IntPtr hWnd);

        [DllImport("user32")]
        private static extern IntPtr IsZoomed(IntPtr hWnd);

        [Flags]
        public enum ESTRRET
        {
            STRRET_WSTR = 0,
            STRRET_OFFSET = 1,
            STRRET_CSTR = 2
        }

        [Flags]
        public enum ESHCONTF
        {
            SHCONTF_FOLDERS = 32,
            SHCONTF_NONFOLDERS = 64,
            SHCONTF_INCLUDEHIDDEN = 128,
        }

        [Flags]
        public enum ESHGDN
        {
            SHGDN_NORMAL = 0,
            SHGDN_INFOLDER = 1,
            SHGDN_FORADDRESSBAR = 16384,
            SHGDN_FORPARSING = 32768
        }

        [Flags]
        public enum ESFGAO
        {
            SFGAO_CANCOPY = 1,
            SFGAO_CANMOVE = 2,
            SFGAO_CANLINK = 4,
            SFGAO_CANRENAME = 16,
            SFGAO_CANDELETE = 32,
            SFGAO_HASPROPSHEET = 64,
            SFGAO_DROPTARGET = 256,
            SFGAO_CAPABILITYMASK = 375,
            SFGAO_LINK = 65536,
            SFGAO_SHARE = 131072,
            SFGAO_READONLY = 262144,
            SFGAO_GHOSTED = 524288,
            SFGAO_DISPLAYATTRMASK = 983040,
            SFGAO_FILESYSANCESTOR = 268435456,
            SFGAO_FOLDER = 536870912,
            SFGAO_FILESYSTEM = 1073741824,
            SFGAO_HASSUBFOLDER = -2147483648,
            SFGAO_CONTENTSMASK = -2147483648,
            SFGAO_VALIDATE = 16777216,
            SFGAO_REMOVABLE = 33554432,
            SFGAO_COMPRESSED = 67108864,
        }

        public enum EIEIFLAG
        {
            IEIFLAG_ASYNC = 1,
            IEIFLAG_CACHE = 2,
            IEIFLAG_ASPECT = 4,
            IEIFLAG_OFFLINE = 8,
            IEIFLAG_GLEAM = 16,
            IEIFLAG_SCREEN = 32,
            IEIFLAG_ORIGSIZE = 64,
            IEIFLAG_NOSTAMP = 128,
            IEIFLAG_NOBORDER = 256,
            IEIFLAG_QUALITY = 512
        }

        [StructLayout(LayoutKind.Sequential, Pack=4, Size=0, CharSet=CharSet.Auto)]
        public struct STRRET_CSTR
        {
            public ESTRRET uType;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst=520)]
            public byte[] cStr;
        }

        [StructLayout(LayoutKind.Explicit, CharSet=CharSet.Auto)]
        public struct STRRET_ANY
        {
            [FieldOffset(0)]
            public ESTRRET uType;

            [FieldOffset(4)]
            public IntPtr pOLEString;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct SIZE
        {
            public int cx;
            public int cy;
        }

        [ComImport(), Guid("00000000-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IUnknown
        {
            [PreserveSig()]
            IntPtr QueryInterface(ref Guid riid, ref IntPtr pVoid);

            [PreserveSig()]
            IntPtr AddRef();

            [PreserveSig()]
            IntPtr Release();
        }

        [ComImportAttribute()]
        [GuidAttribute("00000002-0000-0000-C000-000000000046")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IMalloc
        {
            [PreserveSig()]
            IntPtr Alloc(int cb);

            [PreserveSig()]
            IntPtr Realloc(IntPtr pv, int cb);

            [PreserveSig()]
            void Free(IntPtr pv);

            [PreserveSig()]
            int GetSize(IntPtr pv);

            [PreserveSig()]
            int DidAlloc(IntPtr pv);

            [PreserveSig()]
            void HeapMinimize();
        }

        [ComImportAttribute()]
        [GuidAttribute("000214F2-0000-0000-C000-000000000046")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IEnumIDList
        {
            [PreserveSig()]
            int Next(int celt, ref IntPtr rgelt, ref int pceltFetched);

            void Skip(int celt);

            void Reset();

            void Clone(ref IEnumIDList ppenum);
        }

        [ComImportAttribute()]
        [GuidAttribute("000214E6-0000-0000-C000-000000000046")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IShellFolder
        {
            void ParseDisplayName(IntPtr hwndOwner, IntPtr pbcReserved,
              [MarshalAs(UnmanagedType.LPWStr)]string lpszDisplayName,
              ref int pchEaten, ref IntPtr ppidl, ref int pdwAttributes);

            void EnumObjects(IntPtr hwndOwner,
              [MarshalAs(UnmanagedType.U4)]ESHCONTF grfFlags,
              ref IEnumIDList ppenumIDList);

            void BindToObject(IntPtr pidl, IntPtr pbcReserved, ref Guid riid,
              ref IShellFolder ppvOut);

            void BindToStorage(IntPtr pidl, IntPtr pbcReserved, ref Guid riid, IntPtr ppvObj);

            [PreserveSig()]
            int CompareIDs(IntPtr lParam, IntPtr pidl1, IntPtr pidl2);

            void CreateViewObject(IntPtr hwndOwner, ref Guid riid,
              IntPtr ppvOut);

            void GetAttributesOf(int cidl, IntPtr apidl,
              [MarshalAs(UnmanagedType.U4)]ref ESFGAO rgfInOut);

            void GetUIObjectOf(IntPtr hwndOwner, int cidl, ref IntPtr apidl, ref Guid riid, ref int prgfInOut, ref IUnknown ppvOut);

            void GetDisplayNameOf(IntPtr pidl,
              [MarshalAs(UnmanagedType.U4)]ESHGDN uFlags,
              ref STRRET_CSTR lpName);

            void SetNameOf(IntPtr hwndOwner, IntPtr pidl,
              [MarshalAs(UnmanagedType.LPWStr)]string lpszName,
              [MarshalAs(UnmanagedType.U4)] ESHCONTF uFlags,
              ref IntPtr ppidlOut);
        }

        [ComImportAttribute(), GuidAttribute("BB2E617C-0920-11d1-9A0B-00C04FC2D6C1"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IExtractImage
        {
            void GetLocation([Out(), MarshalAs(UnmanagedType.LPWStr)]
      StringBuilder pszPathBuffer, int cch, ref int pdwPriority, ref SIZE prgSize, int dwRecClrDepth, ref int pdwFlags);

            void Extract(ref IntPtr phBmpThumbnail);
        }

        [DllImport("shell32", CharSet=CharSet.Auto)]
        internal extern static int SHGetMalloc(ref IMalloc ppMalloc);

        [DllImport("shell32", CharSet=CharSet.Auto)]
        internal extern static int SHGetDesktopFolder(ref IShellFolder ppshf);

        [DllImport("shell32", CharSet=CharSet.Auto)]
        internal extern static int SHGetPathFromIDList(IntPtr pidl, StringBuilder pszPath);

        [DllImport("gdi32", CharSet=CharSet.Auto)]
        internal extern static int DeleteObject(IntPtr hObject);

        [DllImport("Shell32", CharSet = CharSet.Auto)]
        public static unsafe extern int ExtractIconEx(
          string lpszFile,
          int nIconIndex,
          IntPtr[] phIconLarge,
          IntPtr[] phIconSmall,
          int nIcons);

        [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        public static unsafe extern int DestroyIcon(IntPtr hIcon);

        public static bool IsMaximized(IntPtr hWnd)
        {
            if(IsZoomed(hWnd) == (IntPtr)0)
                return false;
            else
                return true;
        }

        public static bool IsEnabled(IntPtr hWnd)
        {
            if(IsWindowEnabled(hWnd) == (IntPtr)0)
                return false;
            else
                return true;
        }

        internal static bool IsGlassAvailable()
        {
            return (System.Environment.OSVersion.Version.Major >= 6 && System.Environment.OSVersion.Version.Build >= 5600) && File.Exists(System.Environment.SystemDirectory + @"\dwmapi.dll");
        }

        internal static bool IsGlassEnabled()
        {
            bool result;
            WinAPI.DwmIsCompositionEnabled(out result);
            return result;
        }

        internal static void RemoveFromAeroPeek(IntPtr hwnd)
        {
            if(IsGlassAvailable() && System.Environment.OSVersion.Version.Major == 6 &&
                System.Environment.OSVersion.Version.Minor == 1)
            {
                var attrValue = 1; // True
                WinAPI.DwmSetWindowAttribute(hwnd, 12, ref attrValue, sizeof(int));
            }
        }

        internal static void RemoveFromAltTab(IntPtr hwnd)
        {
            var windowStyle = (uint)WinAPI.GetWindowLong(hwnd, WinAPI.GwlExstyle);
            WinAPI.SetWindowLong(hwnd, WinAPI.GwlExstyle, windowStyle | WinAPI.WsExToolwindow);
        }

        internal static void ExtendGlassFrame(IntPtr hwnd, ref WinAPI.Margins margins)
        {
            if(IsGlassAvailable())
                WinAPI.DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }

        internal static void RemoveFromFlip3D(IntPtr hwnd)
        {
            if(IsGlassAvailable())
            {
                var attrValue = (int)WinAPI.Flip3DPolicy.ExcludeBelow; // True
                WinAPI.DwmSetWindowAttribute(hwnd, WinAPI.Flip3D, ref attrValue, sizeof(int));
            }
        }

        internal static void MakeGlassRegion(ref IntPtr handle, IntPtr rgn)
        {
            if(IsGlassAvailable() && rgn != IntPtr.Zero)
            {
                var bb = new WinAPI.BbStruct
                {
                    Enable = true,
                    Flags = WinAPI.BbFlags.DwmBbEnable | WinAPI.BbFlags.DwmBbBlurregion,
                    Region = rgn
                };
                WinAPI.DwmEnableBlurBehindWindow(handle, ref bb);
            }
        }

        internal static void RemoveGlassRegion(ref IntPtr handle)
        {
            if(IsGlassAvailable())
            {
                var bb = new WinAPI.BbStruct
                {
                    Enable = false,
                    Flags = WinAPI.BbFlags.DwmBbEnable | WinAPI.BbFlags.DwmBbBlurregion,
                    Region = IntPtr.Zero
                };
                WinAPI.DwmEnableBlurBehindWindow(handle, ref bb);
            }
        }

        internal static void RemoveFromDWM(Window w)
        {
            RemoveFromDWM(new WindowInteropHelper(w).Handle);
        }

        internal static void RemoveFromDWM(IntPtr handle)
        {
            RemoveFromAeroPeek(handle);
            RemoveFromAltTab(handle);
            RemoveFromFlip3D(handle);
        }
    }
}