using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Newgen.Base;
using Newgen.Resources;
using Newgen.Windows;

namespace Newgen.Core {

    public class NewgenAdWidget : Package { // TODO: Loads ads automatically
        public const string PackageId = "Ad";
        public const string PathFreeVersionAd = "FreeVersion" + PackageId;
        public const string PathUpdateAd = "Update" + PackageId;
        private int colSpan;
        private Image image;
        private Action onClick;

        public override int ColumnSpan {
            get { return colSpan; }
        }

        public override Uri IconPath {
            get { return new Uri("/Newgen;component/Resources/Ad.png", UriKind.Relative); }
        }

        public override string Id {
            get { return PackageId; }
        }

        public override FrameworkElement Tile {
            get { return image; }
        }

        public NewgenAdWidget() {
            colSpan = 1;
            image = new Image() {
                Stretch = System.Windows.Media.Stretch.Fill,
                Source = new BitmapImage(new Uri("/Newgen;component/Resources/Ad.png", UriKind.Relative))
            };

            image.MouseLeftButtonUp += image_MouseLeftButtonUp;
        }

        ~NewgenAdWidget() {
            image.MouseLeftButtonUp -= image_MouseLeftButtonUp;
        }

        public override void Load(dynamic proxy) {
            var widgetProxy = (PackageProxy)proxy;

            switch (widgetProxy.Path) {
            case PathUpdateAd:
                onClick = () => { SettingsHub.ShowHub(true, true); }; // TODO: Change this to store hub
                image.Source = new BitmapImage(new Uri("/Newgen;component/Resources/UpdateAd.png", UriKind.Relative));
                colSpan = 1;
                break;

            case PathFreeVersionAd:
                onClick = () => { Newgen.Base.MessagingHelper.SendMessageToBackend("URL", Definitions.Url_BuyLicense); };
                image.Source = new BitmapImage(new Uri("/Newgen;component/Resources/FreeVersionAd.png", UriKind.Relative));
                colSpan = 2;
                break;

            default:
                throw new NotSupportedException();
            }
        }

        private void image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (onClick != null)
                onClick();
        }
    }
}