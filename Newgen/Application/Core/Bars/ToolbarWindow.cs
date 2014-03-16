using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using libns.Native;

namespace Newgen {

    /// <summary>
    /// Enum ToolbarWindowLocation.
    /// </summary>
    /// <remarks>...</remarks>
    public enum ToolbarLocation {

        /// <summary>
        /// Top.
        /// </summary>
        Top,

        /// <summary>
        /// Bottom.
        /// </summary>
        Bottom,

        /// <summary>
        /// Left.
        /// </summary>
        Left,

        /// <summary>
        /// Right.
        /// </summary>
        Right,

        /// <summary>
        /// None.
        /// </summary>
        None
    }

    /// <summary>
    /// Class ToolbarWindow.
    /// </summary>
    /// <remarks>...</remarks>
    public class ToolbarWindow : Window {
        private const int DEFAULT_ANIM_TIME = 200;

        private bool islayoutfixed;
        private ToolbarLocation location;

        /// <summary>
        /// Gets the content decorator.
        /// </summary>
        /// <value>The content decorator.</value>
        /// <remarks>...</remarks>
        public Border ContentDecorator { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is opened.
        /// </summary>
        /// <value><c>true</c> if this instance is opened; otherwise, <c>false</c>.</value>
        /// <remarks>...</remarks>
        public bool IsOpened { get; private set; }

        /// <summary>
        /// Gets the content of the main.
        /// </summary>
        /// <value>The content of the main.</value>
        /// <remarks>...</remarks>
        public Grid LayoutRoot { get; private set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        /// <remarks>...</remarks>
        public ToolbarLocation Location {
            get { return location; }
            set { location = value; InitializeComponent(); }
        }

        /// <summary>
        /// Gets the touch decorator.
        /// </summary>
        /// <value>The touch decorator.</value>
        /// <remarks>...</remarks>
        public Border TouchDecorator { get; private set; }

        /// <summary>
        /// Initializes static members of the <see cref="ToolbarWindow" /> class.
        /// </summary>
        /// <remarks>...</remarks>
        static ToolbarWindow() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolbarWindow), new FrameworkPropertyMetadata(typeof(Window)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarWindow" /> class.
        /// </summary>
        /// <remarks>...</remarks>
        public ToolbarWindow() {
            WinAPI.RemoveFromDWM(this);
            AllowsTransparency = true;
            InitializeComponent();
        }

        /// <summary>
        /// Closes the toolbar.
        /// </summary>
        /// <remarks>...</remarks>
        public virtual void CloseToolbar() {
            if (ContextMenu != null && ContextMenu.IsOpen)
                return;

            switch (Location) {
            case ToolbarLocation.Top:
                Helper.Animate(
                    this,
                    Window.TopProperty,
                    DEFAULT_ANIM_TIME,
                    -Height,
                    0.3,
                    0.7,
                    false
                    );
                break;

            case ToolbarLocation.Bottom:
                Helper.Animate(
                    this,
                    Window.TopProperty,
                    DEFAULT_ANIM_TIME,
                    SystemParameters.PrimaryScreenHeight,
                    0.3,
                    0.7,
                    false
                    );
                break;

            case ToolbarLocation.Left:
                Helper.Animate(
                    this,
                    Window.LeftProperty,
                    DEFAULT_ANIM_TIME,
                    -Width + 7.0,
                    0.3,
                    0.7,
                    false
                    );
                break;

            case ToolbarLocation.Right:
                Helper.Animate(
                    this,
                    Window.LeftProperty,
                    DEFAULT_ANIM_TIME,
                    SystemParameters.PrimaryScreenWidth - 7.0,
                    0.3,
                    0.7,
                    false
                    );
                break;

            case ToolbarLocation.None:
            default:
                break;
            }

            IsOpened = false;

            Helper.Delay(() => {
                TouchDecorator.Opacity = 0.01;
                ContentDecorator.Opacity = 0;
            }, DEFAULT_ANIM_TIME);

            App.Screen.ZOrderHelper();

            WinAPI.RemoveFromDWM(this);
        }

        /// <summary>
        /// Initializes the component.
        /// </summary>
        public void InitializeComponent() {
            Topmost = true;
            Top = 0.0;
            Left = 0.0;
            ShowInTaskbar = false;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            Background = null;

            switch (Location) {
            case ToolbarLocation.Top:
                Top = -Height;
                Left = 0.0;
                Width = SystemParameters.PrimaryScreenWidth;
                break;

            case ToolbarLocation.Bottom:
                Top = SystemParameters.WorkArea.Height;
                Left = 0.0;
                Width = SystemParameters.PrimaryScreenWidth;
                break;

            case ToolbarLocation.Left:
                if (Settings.Current.ShowTaskbarAlways) {
                    Top = SystemParameters.WorkArea.Top;
                    Height = SystemParameters.WorkArea.Height;
                }
                else if (Settings.Current.ShowTaskbar) {
                    Top = 0.0;
                    Height = SystemParameters.PrimaryScreenHeight;
                }
                else {
                    Top = 0.0;
                    Height = SystemParameters.PrimaryScreenHeight;
                }
                Left = -Width + 4;
                break;

            case ToolbarLocation.Right:
                if (Settings.Current.ShowTaskbarAlways) {
                    Top = SystemParameters.WorkArea.Top;
                    Height = SystemParameters.WorkArea.Height;
                }
                else if (Settings.Current.ShowTaskbar) {
                    Top = 0.0;
                    Height = SystemParameters.PrimaryScreenHeight;
                }
                else {
                    Top = 0.0;
                    Height = SystemParameters.PrimaryScreenHeight;
                }
                Left = SystemParameters.PrimaryScreenWidth;
                break;

            case ToolbarLocation.None:
            default:
                break;
            }
        }

        /// <summary>
        /// Opens the toolbar.
        /// </summary>
        /// <remarks>...</remarks>
        public virtual void OpenToolbar() {
            if (!IsActive) {
                Activate();
                Show();
            }

            FixLayout();

            TouchDecorator.Opacity = 1;

            ContentDecorator.Opacity = 1;
            TouchDecorator.Background = new SolidColorBrush(Settings.Current.ToolbarBackgroundColor);
            ContentDecorator.Background = new SolidColorBrush(Settings.Current.ToolbarBackgroundColor);

            switch (Location) {
            case ToolbarLocation.Top:
                Helper.Animate(
                    this,
                    Window.TopProperty,
                    DEFAULT_ANIM_TIME,
                    0.0,
                    0.7,
                    0.3,
                    false
                    );
                break;

            case ToolbarLocation.Bottom:
                Helper.Animate(
                    this,
                    Window.TopProperty,
                    DEFAULT_ANIM_TIME,
                    SystemParameters.PrimaryScreenHeight - Height,
                    0.7,
                    0.3,
                    false
                    );
                break;

            case ToolbarLocation.Left:
                Helper.Animate(
                    this,
                    Window.LeftProperty,
                    DEFAULT_ANIM_TIME,
                    0.0,
                    0.7,
                    0.3,
                    false
                    );
                break;

            case ToolbarLocation.Right:
                Helper.Animate(
                    this,
                    Window.LeftProperty,
                    DEFAULT_ANIM_TIME,
                    SystemParameters.PrimaryScreenWidth - Width,
                    0.7,
                    0.3,
                    false
                    );
                break;

            case ToolbarLocation.None:
            default:
                break;
            }

            IsOpened = true;
        }

        /// <summary>
        /// Fixes the layout.
        /// </summary>
        /// <remarks>...</remarks>
        private void FixLayout() {
            if (!islayoutfixed && Content != LayoutRoot) {
                LayoutRoot = new Grid();
                TouchDecorator = new Border();
                ContentDecorator = new Border();
                var content = (FrameworkElement)Content;
                Content = null;
                ContentDecorator.Child = content;

                switch (Location) {
                case ToolbarLocation.Top:
                    LayoutRoot.RowDefinitions.Add(new RowDefinition());
                    LayoutRoot.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(2.0) });
                    LayoutRoot.Children.Add(ContentDecorator);
                    Grid.SetRow(ContentDecorator, 0);
                    LayoutRoot.Children.Add(TouchDecorator);
                    Grid.SetRow(TouchDecorator, 1);
                    LayoutRoot.Margin = new Thickness(0, 0, 0, 5);
                    break;

                case ToolbarLocation.Bottom:
                    LayoutRoot.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(2.0) });
                    LayoutRoot.RowDefinitions.Add(new RowDefinition());
                    LayoutRoot.Children.Add(TouchDecorator);
                    Grid.SetRow(TouchDecorator, 0);
                    LayoutRoot.Children.Add(ContentDecorator);
                    Grid.SetRow(ContentDecorator, 1);
                    LayoutRoot.Margin = new Thickness(0, 5, 0, 0);
                    break;

                case ToolbarLocation.Left:
                    LayoutRoot.ColumnDefinitions.Add(new ColumnDefinition());
                    LayoutRoot.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2.0) });
                    LayoutRoot.Children.Add(ContentDecorator);
                    Grid.SetColumn(ContentDecorator, 0);
                    LayoutRoot.Children.Add(TouchDecorator);
                    Grid.SetColumn(TouchDecorator, 1);
                    LayoutRoot.Margin = new Thickness(0, 0, 5, 0);
                    break;

                case ToolbarLocation.Right:
                    LayoutRoot.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2.0) });
                    LayoutRoot.ColumnDefinitions.Add(new ColumnDefinition());
                    LayoutRoot.Children.Add(TouchDecorator);
                    Grid.SetColumn(TouchDecorator, 0);
                    LayoutRoot.Children.Add(ContentDecorator);
                    Grid.SetColumn(ContentDecorator, 1);
                    LayoutRoot.Margin = new Thickness(5, 0, 0, 0);
                    break;

                case ToolbarLocation.None:
                default:
                    LayoutRoot.Margin = new Thickness(5);
                    break;
                }

                Content = LayoutRoot;
                islayoutfixed = true;
            }
        }
    }
}