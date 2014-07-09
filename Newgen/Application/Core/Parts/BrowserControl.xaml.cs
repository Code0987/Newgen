using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Newgen {
    /// <summary>
    /// Interaction logic for BrowserControl.xaml
    /// </summary>
    public partial class BrowserControl : Border {

        /// <summary>
        /// Gets the browser.
        /// </summary>
        /// <value>The browser.</value>
        /// <remarks>...</remarks>
        public Browser Browser { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserControl" /> class.
        /// </summary>
        /// <param name="browser">The browser.</param>
        /// <remarks>...</remarks>
        public BrowserControl(Browser browser) {
            Browser = browser;

            InitializeComponent();

            if (browser.Provider != null)
                Child = browser.Provider as FrameworkElement;
        }
    }
}
