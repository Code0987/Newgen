using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Newgen.Controls;
using Newgen.Native;

namespace Newgen.Windows
{
    public partial class SwitchBar : ToolbarWindow
    {
        private SwitchAppPreview preview = null;

        public SwitchBar()
            : base()
        {
            this.Location = ToolbarLocation.Left;
            InitializeComponent();
        }

        private Point laspos = new Point();

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if(!this.IsOpened)
            {
                OpenToolbar();
            }
            try
            {
                if(preview != null)
                {
                    if(!preview.IsMulti)
                    {
                        Point mp = Mouse.GetPosition(this);
                        preview.Top += mp.Y - laspos.Y;
                        preview.Left += mp.X - laspos.X;
                        laspos = mp;
                    }
                }
            }
            catch { }
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            this.CloseToolbar();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(this);

            if(preview == null)
            {
                preview = new SwitchAppPreview();
            }

            if(!preview.IsLoaded)
            {
                preview = null;
                preview = new SwitchAppPreview();
            }

            if(e.ClickCount == 1 && e.LeftButton == MouseButtonState.Pressed)
            {
                preview.Release();
                SetNextWindow();
                preview.WindowStartupLocation = WindowStartupLocation.Manual;

                Point mp = Mouse.GetPosition(this);
                preview.Left = -preview.ActualWidth;
                preview.Top = -preview.ActualHeight;
                preview.Top += mp.Y;
                preview.Left += mp.X;
                laspos = mp;

                preview.TextTitle.MaxWidth = 220;

                preview.IsMulti = false;
                preview.Show();
                preview.Refresh();
                return;
            }
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(this);

            if(preview == null)
            {
                preview = new SwitchAppPreview();
            }

            if(!preview.IsLoaded)
            {
                preview = null;
                preview = new SwitchAppPreview();
            }

            preview.Release();
            SetNextWindows();

            preview.IsMulti = true;
            preview.Show();
            preview.Refresh();
            return;
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Topmost = true;
            if(!this.IsOpened)
            {
                OpenToolbar();
            }
            Mouse.Capture(null);
            try
            {
                if(!preview.IsMulti)
                {
                    preview.Switch();
                    preview.Release();
                    preview.Close();
                    preview = null;

                    foreach(Window window in App.Current.Windows)
                    {
                        if(window.GetType() == typeof(SwitchAppPreview))
                            window.Close();
                    }
                }
            }
            catch { }
        }

        private void SetNextWindow()
        {
            //IntPtr handle = ((System.Windows.Interop.HwndSource)System.Windows.Interop.HwndSource.FromVisual(this)).Handle;
            //IntPtr current = WinAPI.GetWindow(handle, WinAPI.GetWindowCmd.First);
            //List<IntPtr> list = new List<IntPtr>();

            //do
            //{
            //    int GWL_STYLE = -16;
            //    uint normalWnd = 0x10000000 | 0x00800000 | 0x00080000;
            //    uint popupWnd = 0x10000000 | 0x80000000 | 0x00080000;
            //    var windowLong = WinAPI.GetWindowLong(current, GWL_STYLE);
            //    var text = WinAPI.GetText(current);
            //    if (((normalWnd & windowLong) == normalWnd || (popupWnd & windowLong) == popupWnd) && !string.IsNullOrEmpty(text))
            //    {
            //        list.Add(current);
            //    }

            //    current = WinAPI.GetWindow(current, WinAPI.GetWindowCmd.Next);

            //    if (current == handle)
            //        current = WinAPI.GetWindow(current, WinAPI.GetWindowCmd.Next);
            //}
            //while (current != IntPtr.Zero);
            //try
            //{
            //    preview.PreviewWindow(list[list.Count - 1]);
            //}
            //catch { }
            List<IntPtr> list = new List<IntPtr>();

            // Get the desktop window handle
            IntPtr nDeshWndHandle = WinAPI.GetDesktopWindow();

            //get the first child window
            IntPtr nChildHandle = WinAPI.GetWindow(nDeshWndHandle, WinAPI.GetWindowCmd.Child);
            while(nChildHandle != IntPtr.Zero)
            {
                IntPtr LHParent = WinAPI.GetWindowLongPtr(nChildHandle, WinAPI.GWL.GWL_HWNDPARENT); //GWL_HWNDPARENT = -8
                IntPtr LEXSTYLE = WinAPI.GetWindowLongPtr(nChildHandle, WinAPI.GWL.GWL_EXSTYLE);//GWL_EXSTYLE = -20 && GWL_STYLE = -16
                if(WinAPI.IsWindowVisible(nChildHandle) && ((LHParent == IntPtr.Zero) || (LHParent == nDeshWndHandle)) && (((int)LEXSTYLE & 0x00000080) == 0 || ((int)LEXSTYLE & 0x40000) != 0))
                {
                    list.Add((IntPtr)nChildHandle);
                }
                nChildHandle = WinAPI.GetWindow(nChildHandle, WinAPI.GetWindowCmd.Next); //GW_HWNDNEXT  = 2
            }
            try
            {
                preview.PreviewWindow(list[list.Count - 1]);
            }
            catch { }
        }

        private void SetNextWindows()
        {
            IntPtr handle = ((System.Windows.Interop.HwndSource)System.Windows.Interop.HwndSource.FromVisual(this)).Handle;
            IntPtr current = WinAPI.GetWindow(handle, WinAPI.GetWindowCmd.First);

            do
            {
                int GWL_STYLE = -16;
                uint normalWnd = 0x10000000 | 0x00800000 | 0x00080000;
                uint popupWnd = 0x10000000 | 0x80000000 | 0x00080000;
                var windowLong = WinAPI.GetWindowLong(current, GWL_STYLE);
                var text = WinAPI.GetText(current);
                if(((normalWnd & windowLong) == normalWnd || (popupWnd & windowLong) == popupWnd) && !string.IsNullOrEmpty(text) && current != handle)
                {
                    preview.PreviewWindow(current);
                }

                current = WinAPI.GetWindow(current, WinAPI.GetWindowCmd.Next);

                // if (current == handle)
                //      current = WinAPI.GetWindow(current, WinAPI.GetWindowCmd.Next);
            }
            while(current != IntPtr.Zero);
        }
    }
}