using System;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;
using Newgen.Base;

namespace Quotes
{
    /// <summary>
    /// Interaction logic for Tile.xaml
    /// </summary>
    public partial class Tile : UserControl
    {
        private Random random = new Random((int)DateTime.Now.Ticks);
        private DispatcherTimer timer = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tile"/> class.
        /// </summary>
        public Tile()
        {
            InitializeComponent();
        }

        public void Load()
        {
            //! Localization : Make widget use same language as Newgen. If you are not planning to add localization support you can remove this lines.
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(E.Language); //! {
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(E.Language); //! }

            //! Initialize all your objects here.
            timer = Helper.RunFor(UpdateQuote, -1, 15 * 60 * 1000);
            UpdateQuote();
        }

        public void Unload()
        {
            //! Free resources here to save memory.
            if(timer != null)
                timer.Stop();
            timer = null;
        }

        private void UpdateQuote()
        {
            string quote = Widget.Settings.QuotesList[random.Next(Widget.Settings.QuotesList.Count)];

            if((DateTime.Now - Widget.Settings.LastQuoteDownloadTime).TotalHours > 23.5)
            {
                try
                {
                    XmlReader reader = XmlReader.Create("http://feeds.feedburner.com/quotationspage/qotd");
                    SyndicationFeedFormatter formatter = new Rss20FeedFormatter();
                    formatter.ReadFrom(reader);
                    SyndicationItem feed = formatter.Feed.Items.FirstOrDefault();
                    if(feed != null)
                    {
                        quote = string.Format("{1}@{0}", feed.Title.Text.Trim(), feed.Summary.Text.Replace("\"", "").Trim());
                        Widget.Settings.QuotesList.Add(quote);
                    }
                    Widget.Settings.LastQuoteDownloadTime = DateTime.Now;
                }
                catch { }
            }

            try
            {
                string[] quoteparts = quote.Split('@');

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        this.TextBlock_Quote.Text = quoteparts[0];
                        this.TextBlock_Author.Text = quoteparts[1];
                    }
                    catch { }
                }));
            }
            catch { }
        }
    }
}