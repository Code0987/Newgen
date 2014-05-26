﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using libns;
using libns.Media.Imaging;
using libns.Native;
using libns.Threading;
using Microsoft.Win32;
using Newgen.Packages;

namespace Newgen {

    /// <summary>
    /// Class Screen.
    /// </summary>
    /// <remarks>...</remarks>
    public partial class Screen : Window {

        /// <summary>
        /// The start bar
        /// </summary>
        internal StartBar StartBar;

        /// <summary>
        /// The task bar
        /// </summary>
        internal TaskBar TaskBar;

        /// <summary>
        /// The thumbnails bar
        /// </summary>
        internal ThumbnailsBar ThumbnailsBar;

        /// <summary>
        /// The tiles bar
        /// </summary>
        internal TilesBar TilesBar;

        /// <summary>
        /// Initializes a new instance of the <see cref="Screen" /> class.
        /// </summary>
        /// <remarks>...</remarks>
        public Screen() {
            this.InitializeComponent();

            if (E.BackgroundColor.A == 255)
                AllowsTransparency = false;
        }

        /// <summary>
        /// Zs the order helper.
        /// </summary>
        /// <param name="settrue">if set to <c>true</c> [settrue].</param>
        /// <remarks>...</remarks>
        internal void ZOrderHelper(bool settrue = true) {
            if (settrue) {
                if (ThumbnailsBar != null) {
                    ThumbnailsBar.Topmost = false;
                    ThumbnailsBar.Topmost = true;
                }
                else if (TaskBar != null) {
                    TaskBar.Topmost = false;
                    TaskBar.Topmost = true;
                }
                if (StartBar != null) {
                    StartBar.Topmost = false;
                    StartBar.Topmost = true;
                }
            }
            else {
                if (ThumbnailsBar != null) {
                    ThumbnailsBar.Topmost = true;
                    ThumbnailsBar.Topmost = false;
                }
                else if (TaskBar != null) {
                    TaskBar.Topmost = true;
                    TaskBar.Topmost = false;
                }
                if (StartBar != null) {
                    StartBar.Topmost = true;
                    StartBar.Topmost = false;
                }
            }
        }

        /// <summary>
        /// Loads the toolbars.
        /// </summary>
        /// <remarks>...</remarks>
        private void LoadToolbars() {
            StartBar = new StartBar();
            if (Settings.Current.UseThumbailsBar)
                ThumbnailsBar = new ThumbnailsBar();
            else
                TaskBar = new TaskBar();

            if (Settings.Current.UseThumbailsBar)
                ThumbnailsBar.OpenToolbar();
            else
                TaskBar.OpenToolbar();
            StartBar.OpenToolbar();

            TilesBar = new TilesBar();
            TilesBar.OpenToolbar();

            Helper.Delay(() => {
                if (Settings.Current.UseThumbailsBar)
                    ThumbnailsBar.CloseToolbar();
                else
                    TaskBar.CloseToolbar();
                StartBar.CloseToolbar();

                TilesBar.CloseToolbar();
            }, 250);
        }

        /// <summary>
        /// Handles the <see cref="E:Closing" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="System.ComponentModel.CancelEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            E.Messenger.RemoveListener(new WindowInteropHelper(this).Handle);
        }

        /// <summary>
        /// Handles the <see cref="E:Loaded" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnLoaded(object sender, RoutedEventArgs e) {
            this.UserBadgeControl.Reload();
            Helper.Animate(this.Header, OpacityProperty, 250, 1);

            LoadToolbars();

            // Find all packages
            Helper.Delay(PackageManager.Current.Scan, 3500);
        }

        /// <summary>
        /// Handles the <see cref="E:MouseLeftButtonUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (ThumbnailsBar != null) {
                ThumbnailsBar.CloseToolbar();
            }
            else if (TaskBar != null) {
                TaskBar.CloseToolbar();
            }
            if (StartBar != null) {
                StartBar.CloseToolbar();
            }
        }

        /// <summary>
        /// Handles the <see cref="E:MouseMove" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMouseMove(object sender, MouseEventArgs e) {
        }

        /// <summary>
        /// Handles the <see cref="E:SourceInitialized" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnSourceInitialized(object sender, EventArgs e) {
            var handle = new WindowInteropHelper(this).Handle;

            WinAPI.RemoveFromDWM(handle);

            E.Messenger.AddListener(handle);

            try {
                IntPtr taskbar = WinAPI.FindWindow("Shell_TrayWnd", "");
                IntPtr hwndOrb = WinAPI.FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr)0xC017, null);
                Width = SystemParameters.PrimaryScreenWidth;

                if (Settings.Current.ShowTaskbarAlways) {
                    Height = SystemParameters.WorkArea.Height;
                    Top = SystemParameters.WorkArea.Top;
                    WinAPI.ShowWindow(taskbar, WindowShowStyle.Show);
                    WinAPI.ShowWindow(hwndOrb, WindowShowStyle.Show);
                }
                else if (Settings.Current.ShowTaskbar) {
                    Height = SystemParameters.PrimaryScreenHeight;
                    Top = 0;
                    WinAPI.ShowWindow(taskbar, WindowShowStyle.Show);
                    WinAPI.ShowWindow(hwndOrb, WindowShowStyle.Show);
                }
                else {
                    Height = SystemParameters.PrimaryScreenHeight;
                    Top = 0;
                    WinAPI.ShowWindow(taskbar, WindowShowStyle.Hide);
                    WinAPI.ShowWindow(hwndOrb, WindowShowStyle.Hide);
                }
            }
            catch { }

            this.Left = 0;
        }

        /// <summary>
        /// Handles the <see cref="E:TitleTextMouseUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void OnTitleTextMouseUp(object sender, MouseButtonEventArgs e) {
            TilesControl.ScrollToLeftEnd();
        }
    }
}