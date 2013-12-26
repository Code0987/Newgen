using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Newgen.Base
{
    /// <summary>
    /// Animation type
    /// </summary>
    public enum AnimationType
    {
        /// <summary>
        /// Animation is controlled internally, i.e. default animation.
        /// </summary>
        Internal = 0,
        /// <summary>
        /// Animation not controlled internally, i.e. you'll have to implement your logic for animation.
        /// </summary>
        Custom = 1
    }

    /// <summary>
    /// Hub Window. This enables to use Hub feature of Newgen.
    /// It's basically a full-screen window for presenting custom data.
    /// </summary>
    public partial class HubWindow : Window
    {
        /// <summary>
        /// Gets or sets the animation type used by this window.
        /// </summary>
        /// <value>
        /// The animation.
        /// </value>
        public AnimationType Animation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        private bool IsHubActive { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HubWindow"/> class.
        /// </summary>
        public HubWindow()
        {
            this.Animation = AnimationType.Internal;
            this.IsHubActive = true;

            this.Closing += new System.ComponentModel.CancelEventHandler(HubWindow_Closing);
            this.KeyUp += new KeyEventHandler(HubWindow_KeyUp);

            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes the component.
        /// </summary>
        public void InitializeComponent()
        {
            E.CallEvent("HubOpening");

            this.Left = 0;
            this.Top = 0;
            this.Opacity = 0;
            this.Topmost = true;
            this.ShowInTaskbar = false;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;

            this.VisualBitmapScalingMode = BitmapScalingMode.HighQuality;
            this.VisualClearTypeHint = ClearTypeHint.Auto;
            this.VisualTextHintingMode = TextHintingMode.Auto;
            this.VisualTextRenderingMode = TextRenderingMode.Aliased;

            Helper.Delay(new Action(() =>
            {
                this.AnimateStart();
            }), 180);
        }

        private void HubWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) base.Close();
        }

        private void HubWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            E.CallEvent("HubClosing");

            if (this.IsHubActive)
            {
                e.Cancel = true;
                this.AnimateClose();
            }
        }

        private void AnimateStart()
        {
            this.IsHubActive = true;
            switch (this.Animation)
            {
                case AnimationType.Custom:
                    {
                        this.IsHubActive = false;
                        this.Left = 0;
                        this.Top = 0;
                        this.Width = SystemParameters.PrimaryScreenWidth;
                        this.Height = SystemParameters.PrimaryScreenHeight;
                    }
                    break;
                case AnimationType.Internal:
                default:
                    {
                        this.IsHubActive = true;
                        this.Left = -SystemParameters.PrimaryScreenWidth;
                        this.Top = 0;
                        this.Width = SystemParameters.PrimaryScreenWidth;
                        this.Height = SystemParameters.PrimaryScreenHeight;

                        DoubleAnimation leftanimation = new DoubleAnimation()
                        {
                            To = 0,
                            Duration = new Duration(TimeSpan.FromMilliseconds(180)),
                            BeginTime = TimeSpan.FromMilliseconds(50),
                            AccelerationRatio = 0.3,
                            DecelerationRatio = 0.7,
                        };
                        this.BeginAnimation(LeftProperty, leftanimation);
                        Helper.Animate(this, OpacityProperty, 3, 0, 1, 0.3, 0.7);
                    }
                    break;
            }
        }

        private void AnimateClose()
        {
            switch (this.Animation)
            {
                case AnimationType.Custom:
                    {
                        this.Topmost = false;
                        this.IsHubActive = false;
                        this.Close();
                    }
                    break;
                case AnimationType.Internal:
                default:
                    {
                        DoubleAnimation leftanimation = new DoubleAnimation()
                        {
                            To = -this.ActualWidth,
                            Duration = new Duration(TimeSpan.FromMilliseconds(180)),
                            BeginTime = TimeSpan.FromMilliseconds(1),
                            AccelerationRatio = 0.7,
                            DecelerationRatio = 0,
                        };
                        leftanimation.Completed += (a, b) =>
                        {
                            this.Left = -this.ActualWidth;

                            leftanimation = null;
                            Helper.Delay(new Action(() =>
                            {
                                IsHubActive = false; Topmost = false; Hide(); Close();
                            }), 1);
                        };
                        this.BeginAnimation(LeftProperty, leftanimation);
                    }
                    break;
            }
        }
    }
}