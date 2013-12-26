using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Newgen.Base;

namespace Newgen.Core
{
    public class NewgenAdWidget : NewgenWidget
    {
        private Action onClick;
        private Image image;
        private int colSpan;

        public override string Name { get { return null; } }

        public override FrameworkElement WidgetControl
        {
            get { return image; }
        }

        public override Uri IconPath
        {
            get { return new Uri("/Newgen;component/Resources/Ad.png", UriKind.Relative); }
        }

        public override int ColumnSpan
        {
            get { return colSpan; }
        }

        public NewgenAdWidget()
        {
            colSpan = 1;
            image = new Image()
            {
                Stretch = System.Windows.Media.Stretch.Fill,
                Source = new BitmapImage(new Uri("/Newgen;component/Resources/Ad.png", UriKind.Relative))
            };

            image.MouseLeftButtonUp += image_MouseLeftButtonUp;
        }

        ~NewgenAdWidget()
        {
            image.MouseLeftButtonUp -= image_MouseLeftButtonUp;
        }

        private void image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(onClick != null)
                onClick();
        }

        public override void Load(string path)
        {
            switch(path)
            {
                case ID_UpdateAd:
                    onClick = () => { App.ShowOptions(true, true); };
                    image.Source = new BitmapImage(new Uri("/Newgen;component/Resources/UpdateAd.png", UriKind.Relative));
                    colSpan = 1;
                    break;

                case ID_FreeVersionAd:
                    onClick = () => { Newgen.Base.MessagingHelper.SendMessageToNewgen("URL", Resources.Resources.Url_BuyLicense); };
                    image.Source = new BitmapImage(new Uri("/Newgen;component/Resources/FreeVersionAd.png", UriKind.Relative));
                    colSpan = 2;
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        public const string ID_Ad = "Ad::AdWidget";
        public const string ID_UpdateAd = "Update" + ID_Ad;
        public const string ID_FreeVersionAd = "FreeVersion" + ID_Ad;

        public static void LoadAds()
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1500);
                if(Windows.Settings.IsUpdateAvailable())
                {
                    if(!App.WidgetManager.IsWidgetLoadedById(ID_UpdateAd))
                        App.Current.Dispatcher.BeginInvoke(new Action(() => App.WidgetManager.LoadWidget(App.WidgetManager.CreateGeneratedWidget(ID_UpdateAd, null))));
                }
                else
                {
                    App.Current.Dispatcher.BeginInvoke(new Action(() => App.WidgetManager.UnloadWidgetById(ID_UpdateAd)));
                }
            });
            if(!App.IsProMode)
                if(!App.WidgetManager.IsWidgetLoadedById(ID_FreeVersionAd))
                    App.Settings.LoadedWidgets.Add(new TileScreenWidgetInfo() { Id = ID_FreeVersionAd, Column = 0, Row = 2 });
        }
    }
}