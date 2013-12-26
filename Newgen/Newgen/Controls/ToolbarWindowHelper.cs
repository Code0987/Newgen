using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newgen.Base;
using Newgen.Native;

namespace Newgen.Controls
{
    /// <summary>
    /// Enum ToolbarWindowLocation.
    /// </summary>
    /// <remarks>...</remarks>
    public enum ToolbarLocation
    {
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
    /// ToolbarWindow.
    /// </summary>
    /// <remarks>...</remarks>
    public class ToolbarWindow : Window
    {
        private const int DEFAULT_ANIM_TIME = 200;

        private bool islayoutfixed;
        private ToolbarLocation location;

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        /// <remarks>...</remarks>
        public ToolbarLocation Location
        {
            get { return this.location; }
            set { this.location = value; this.InitializeComponent(); }
        }

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
        /// Gets the touch decorator.
        /// </summary>
        /// <value>The touch decorator.</value>
        /// <remarks>...</remarks>
        public Border TouchDecorator { get; private set; }

        /// <summary>
        /// Gets the content decorator.
        /// </summary>
        /// <value>The content decorator.</value>
        /// <remarks>...</remarks>
        public Border ContentDecorator { get; private set; }

        /// <summary>
        /// Initializes static members of the <see cref="ToolbarWindow" /> class.
        /// </summary>
        /// <remarks>...</remarks>
        static ToolbarWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolbarWindow), new FrameworkPropertyMetadata(typeof(Window)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarWindow" /> class.
        /// </summary>
        /// <remarks>...</remarks>
        public ToolbarWindow()
        {
            WinAPI.RemoveFromDWM(this);
            this.AllowsTransparency = true;
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes the component.
        /// </summary>
        public void InitializeComponent()
        {
            this.Topmost = true;
            this.Top = 0.0;
            this.Left = 0.0;
            this.ShowInTaskbar = false;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.Background = null;

            switch(this.Location)
            {
                case ToolbarLocation.Top:
                    this.Top = -this.Height;
                    this.Left = 0.0;
                    this.Width = SystemParameters.PrimaryScreenWidth;
                    break;

                case ToolbarLocation.Bottom:
                    this.Top = SystemParameters.WorkArea.Height;
                    this.Left = 0.0;
                    this.Width = SystemParameters.PrimaryScreenWidth;
                    break;

                case ToolbarLocation.Left:
                    if(App.Settings.ShowTaskbarAlways)
                    {
                        this.Top = SystemParameters.WorkArea.Top;
                        this.Height = SystemParameters.WorkArea.Height;
                    }
                    else if(App.Settings.ShowTaskbar)
                    {
                        this.Top = 0.0;
                        this.Height = SystemParameters.PrimaryScreenHeight;
                    }
                    else
                    {
                        this.Top = 0.0;
                        this.Height = SystemParameters.PrimaryScreenHeight;
                    }
                    this.Left = -this.Width+4;
                    break;

                case ToolbarLocation.Right:
                    if(App.Settings.ShowTaskbarAlways)
                    {
                        this.Top = SystemParameters.WorkArea.Top;
                        this.Height = SystemParameters.WorkArea.Height;
                    }
                    else if(App.Settings.ShowTaskbar)
                    {
                        this.Top = 0.0;
                        this.Height = SystemParameters.PrimaryScreenHeight;
                    }
                    else
                    {
                        this.Top = 0.0;
                        this.Height = SystemParameters.PrimaryScreenHeight;
                    }
                    this.Left = SystemParameters.PrimaryScreenWidth;
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
        public virtual void OpenToolbar()
        {
            if(!this.IsActive)
            {
                this.Activate();
                this.Show();
            }
            if(!this.islayoutfixed && this.Content!=this.LayoutRoot)
            {
                this.LayoutRoot = new Grid();
                this.TouchDecorator = new Border();
                this.ContentDecorator = new Border();
                FrameworkElement content = (FrameworkElement)this.Content;
                this.Content = null;
                this.ContentDecorator.Child = content;
                switch(this.Location)
                {
                    case ToolbarLocation.Top:
                        this.LayoutRoot.RowDefinitions.Add(new RowDefinition());
                        this.LayoutRoot.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(2.0) });
                        this.LayoutRoot.Children.Add(this.ContentDecorator);
                        Grid.SetColumn(this.ContentDecorator, 0);
                        this.LayoutRoot.Children.Add(this.TouchDecorator);
                        Grid.SetColumn(this.TouchDecorator, 1);
                        this.LayoutRoot.Margin = new Thickness(0, 0, 0, 5);
                        if(App.Settings.EnableOutOfBoxExperience)
                        {
                            this.TouchDecorator.Effect = new System.Windows.Media.Effects.DropShadowEffect
                            {
                                BlurRadius = 4.0,
                                ShadowDepth = 2.0,
                                Opacity = 0.75,
                                Direction = 270.0
                            };
                        }
                        break;

                    case ToolbarLocation.Bottom:
                        this.LayoutRoot.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(2.0) });
                        this.LayoutRoot.RowDefinitions.Add(new RowDefinition());
                        this.LayoutRoot.Children.Add(this.TouchDecorator);
                        Grid.SetColumn(this.TouchDecorator, 0);
                        this.LayoutRoot.Children.Add(this.ContentDecorator);
                        Grid.SetColumn(this.ContentDecorator, 1);
                        this.LayoutRoot.Margin = new Thickness(0, 5, 0, 0);
                        if(App.Settings.EnableOutOfBoxExperience)
                        {
                            this.TouchDecorator.Effect = new System.Windows.Media.Effects.DropShadowEffect
                            {
                                BlurRadius = 4.0,
                                ShadowDepth = 2.0,
                                Opacity = 0.75,
                                Direction = 90.0
                            };
                        }
                        break;

                    case ToolbarLocation.Left:
                        this.LayoutRoot.ColumnDefinitions.Add(new ColumnDefinition());
                        this.LayoutRoot.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2.0) });
                        this.LayoutRoot.Children.Add(this.ContentDecorator);
                        Grid.SetColumn(this.ContentDecorator, 0);
                        this.LayoutRoot.Children.Add(this.TouchDecorator);
                        Grid.SetColumn(this.TouchDecorator, 1);
                        this.LayoutRoot.Margin = new Thickness(0, 0, 5, 0);
                        if(App.Settings.EnableOutOfBoxExperience)
                        {
                            this.TouchDecorator.Effect = new System.Windows.Media.Effects.DropShadowEffect
                            {
                                BlurRadius = 4.0,
                                ShadowDepth = 2.0,
                                Opacity = 0.75,
                                Direction = 0.0
                            };
                        }
                        break;

                    case ToolbarLocation.Right:
                        this.LayoutRoot.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2.0) });
                        this.LayoutRoot.ColumnDefinitions.Add(new ColumnDefinition());
                        this.LayoutRoot.Children.Add(this.TouchDecorator);
                        Grid.SetColumn(this.TouchDecorator, 0);
                        this.LayoutRoot.Children.Add(this.ContentDecorator);
                        Grid.SetColumn(this.ContentDecorator, 1);
                        this.LayoutRoot.Margin = new Thickness(5, 0, 0, 0);
                        if(App.Settings.EnableOutOfBoxExperience)
                        {
                            this.TouchDecorator.Effect = new System.Windows.Media.Effects.DropShadowEffect
                            {
                                BlurRadius = 4.0,
                                ShadowDepth = 2.0,
                                Opacity = 0.75,
                                Direction = 180.0
                            };
                        }
                        break;

                    case ToolbarLocation.None:
                    default:
                        this.LayoutRoot.Margin = new Thickness(5);
                        break;
                }
                this.Content = this.LayoutRoot;
                this.islayoutfixed = true;
            }
            this.TouchDecorator.Opacity = 1;
            this.ContentDecorator.Opacity = 1;
            this.TouchDecorator.Background = new SolidColorBrush(App.Settings.ToolbarBackgroundColor);
            this.ContentDecorator.Background = new SolidColorBrush(App.Settings.ToolbarBackgroundColor);
            switch(this.Location)
            {
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
                        SystemParameters.PrimaryScreenHeight-this.Height,
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
                        SystemParameters.PrimaryScreenWidth-this.Width,
                        0.7,
                        0.3,
                        false
                        );
                    break;

                case ToolbarLocation.None:
                default:
                    break;
            }
            this.IsOpened = true;
        }

        /// <summary>
        /// Closes the toolbar.
        /// </summary>
        /// <remarks>...</remarks>
        public virtual void CloseToolbar()
        {
            if(this.ContextMenu!=null && this.ContextMenu.IsOpen)
                return;
            switch(this.Location)
            {
                case ToolbarLocation.Top:
                    Helper.Animate(
                        this,
                        Window.TopProperty,
                        DEFAULT_ANIM_TIME,
                        -this.Height,
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
                        -this.Width+7.0,
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
                        SystemParameters.PrimaryScreenWidth-7.0,
                        0.3,
                        0.7,
                        false
                        );
                    break;

                case ToolbarLocation.None:
                default:
                    break;
            }
            this.IsOpened = false;
            Helper.Delay(() =>
            {
                this.TouchDecorator.Opacity = 0.01;
                this.ContentDecorator.Opacity = 0;
            }, DEFAULT_ANIM_TIME);
            App.StartScreen.ZOrderHelper();
            WinAPI.RemoveFromDWM(this);
        }
    }
}