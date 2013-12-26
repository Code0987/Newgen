using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;
using Newgen.Base;
using Newgen.Controls.Search;
using Newgen.Core;

namespace Newgen.Windows
{
    /// <summary>
    /// Interaction logic for SearchHub.xaml
    /// </summary>
    public partial class SearchHub : Window
    {
        // Background thread for our search
        private Thread backgroundSearchThread = null;
        internal ShellSearchFolder searchFolder = null;
        private ShellContainer selectedScope = (ShellContainer)KnownFolders.UsersFiles;

        public SearchHub()
        {
            InitializeComponent();

            HelperMethods.RunMethodAsyncThreadSafe(() =>
            {
                var sortedKnownFolders = from folder in KnownFolders.All
                                         where (folder.CanonicalName != null && folder.CanonicalName.Length > 0)
                                         orderby folder.CanonicalName
                                         select folder;

                // Add the Browse... item so users can select any arbitrary location
                StackPanel browsePanel = new StackPanel();
                browsePanel.Margin = new Thickness(5, 2, 5, 2);
                browsePanel.Orientation = Orientation.Horizontal;

                Image browseImg = new Image();
                browseImg.Source = (new StockIcons()).FolderOpen.BitmapSource;
                browseImg.Height = 25;

                TextBlock browseTextBlock = new TextBlock();
                browseTextBlock.Background = Brushes.Transparent;
                browseTextBlock.FontSize = 12;
                browseTextBlock.Margin = new Thickness(2);
                browseTextBlock.VerticalAlignment = VerticalAlignment.Center;
                browseTextBlock.Text = "Browse...";

                browsePanel.Children.Add(browseImg);
                browsePanel.Children.Add(browseTextBlock);

                SearchScopesCombo.Items.Add(browsePanel);

                foreach (ShellContainer obj in sortedKnownFolders)
                {
                    StackPanel panel = new StackPanel();
                    panel.Margin = new Thickness(5, 2, 5, 2);
                    panel.Orientation = Orientation.Horizontal;

                    Image img = new Image();
                    img.Source = obj.Thumbnail.SmallBitmapSource;
                    img.Height = 25;

                    TextBlock textBlock = new TextBlock();
                    textBlock.Background = Brushes.Transparent;
                    textBlock.FontSize = 12;
                    textBlock.Margin = new Thickness(2);
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    textBlock.Text = obj.Name;

                    panel.Children.Add(img);
                    panel.Children.Add(textBlock);

                    panel.Tag = obj;

                    HelperMethods.RunMethodAsyncThreadSafe(() =>
                    {
                        SearchScopesCombo.Items.Add(panel);
                    });

                    // Set our initial search scope.
                    // If Shell Libraries are supported, search in all the libraries,
                    // else, use user's profile (my documents, etc)
                    if (ShellLibrary.IsPlatformSupported)
                    {
                        if (obj == (ShellContainer)KnownFolders.Libraries)
                            SearchScopesCombo.SelectedItem = panel;
                    }
                    else
                    {
                        if (obj == (ShellContainer)KnownFolders.UsersFiles)
                            SearchScopesCombo.SelectedItem = panel;
                    }
                }

                SearchScopesCombo.ToolTip = "Change the scope of the search.";

                SearchScopesCombo.SelectionChanged += new SelectionChangedEventHandler(SearchScopesCombo_SelectionChanged);
            });
        }

        private void WindowKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        private void WindowSourceInitialized(object sender, EventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            Helper.Animate(this, OpacityProperty, 500, 0, 1);
        }

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Helper.Animate(this, OpacityProperty, 250, 0);
        }

        private void SearchScopesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HelperMethods.RunMethodAsyncThreadSafe(() =>
                      {
                          try
                          {
                              StackPanel previousSelection = e.RemovedItems[0] as StackPanel;

                              if (SearchScopesCombo.SelectedIndex == 0)
                              {
                                  // Show a folder selection dialog
                                  CommonOpenFileDialog cfd = new CommonOpenFileDialog();
                                  cfd.AllowNonFileSystemItems = true;
                                  cfd.IsFolderPicker = true;

                                  cfd.Title = "Select a folder as your search scope...";

                                  if (cfd.ShowDialog() == CommonFileDialogResult.Ok)
                                  {
                                      ShellContainer container = cfd.FileAsShellObject as ShellContainer;

                                      if (container != null)
                                      {
                                          #region Add it to the bottom of our combobox

                                          StackPanel panel = new StackPanel();
                                          panel.Margin = new Thickness(5, 2, 5, 2);
                                          panel.Orientation = Orientation.Horizontal;

                                          Image img = new Image();
                                          img.Source = container.Thumbnail.SmallBitmapSource;
                                          img.Height = 25;

                                          TextBlock textBlock = new TextBlock();
                                          textBlock.Background = Brushes.Transparent;
                                          textBlock.FontSize = 12;
                                          textBlock.Margin = new Thickness(4);
                                          textBlock.VerticalAlignment = VerticalAlignment.Center;
                                          textBlock.Text = container.Name;

                                          panel.Children.Add(img);
                                          panel.Children.Add(textBlock);

                                          SearchScopesCombo.Items.Add(panel);

                                          #endregion Add it to the bottom of our combobox

                                          // Set our selected scope
                                          selectedScope = container;
                                          SearchScopesCombo.SelectedItem = panel;
                                      }
                                      else
                                          SearchScopesCombo.SelectedItem = previousSelection;
                                  }
                                  else
                                      SearchScopesCombo.SelectedItem = previousSelection;
                              }
                              else if (SearchScopesCombo.SelectedItem != null && SearchScopesCombo.SelectedItem is ShellContainer)
                                  selectedScope = ((StackPanel)SearchScopesCombo.SelectedItem).Tag as ShellContainer;
                          }
                          catch { }
                      });
        }

        private void SearchTextBox_Search(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SearchBox.Text))
            {
                try
                {
                    SearchCondition searchCondition;

                    this.Cursor = Cursors.Wait;
                    SearchBox.IsEnabled = false;
                    searchCondition = SearchConditionFactory.ParseStructuredQuery(SearchBox.Text);

                    List<SearchItem> items = new List<SearchItem>();
                    ThreadStart deleg = (delegate()
                    {
                        try
                        {
                            searchFolder = new ShellSearchFolder(searchCondition, selectedScope);
                            foreach (ShellObject so in searchFolder)
                            {
                                try
                                {
                                    SearchItem item = new SearchItem();
                                    item.Name = so.Name;
                                    BitmapSource thumbnail = so.Thumbnail.MediumBitmapSource;
                                    thumbnail.Freeze();
                                    item.Thumbnail = thumbnail;
                                    item.ParsingName = so.ParsingName;
                                    items.Add(item);
                                    HelperMethods.RunMethodAsyncThreadSafe(() =>
                                    {
                                        IconsView.ItemsSource = items;
                                        if (IconsView.Items.Count > 0) IconsView.SelectedIndex = 0;
                                    });
                                }
                                catch { }
                            }
                        }
                        catch
                        {
                            searchFolder.Dispose();
                            searchFolder = null;
                        }
                    });
                    Thread thr = new Thread(deleg);
                    thr.SetApartmentState(ApartmentState.STA);
                    thr.Start();

                    this.Cursor = Cursors.Arrow;
                    SearchBox.IsEnabled = true;
                }
                catch { }
            }

            else
            {
                IconsView.ItemsSource = null;
            }
        }
    }
}