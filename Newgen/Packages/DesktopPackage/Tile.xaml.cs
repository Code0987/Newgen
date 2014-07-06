﻿using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using libns;
using libns.Threading;
using libns.Media.Imaging;
using System.Windows;

namespace DesktopPackage {
    /// <summary>
    /// Interaction logic for Tile.xaml
    /// </summary>
    public partial class Tile : Border {
        /// <summary>
        /// The package
        /// </summary>
        private Package package;

        /// <summary>
        /// The wallpaper path
        /// </summary>
        private string wallpaperPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tile"/> class.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <remarks>...</remarks>
        public Tile(Package package) {
            this.package = package;

            InitializeComponent();
        }
        /// <summary>
        /// Loads this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public void Load() {
            try {
                var wpReg = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
                wallpaperPath = wpReg.GetValue("WallPaper").ToString();
                wpReg.Close();
            }
            catch /* Eat */ { /* Tasty ? */ }

            UpdatePreviewImage();
            ThreadingExtensions.RunFor(new Action(UpdatePreviewImage), -1, 2000);
        }

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public void Unload() {

        }
        /// <summary>
        /// Handles the <see cref="E:UserControlMouseLeftButtonUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnUserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                foreach (var window in Application.Current.Windows) {
                    ((Window)window).Hide();
                }
            }));
        }
        /// <summary>
        /// Updates the preview image.
        /// </summary>
        /// <remarks>...</remarks>
        private void UpdatePreviewImage() {
            if (!File.Exists(wallpaperPath))
                return;
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                PreviewImage.Source = wallpaperPath.ToBitmapSource();
            }));
        }
    }
}