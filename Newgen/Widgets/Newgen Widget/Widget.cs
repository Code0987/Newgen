using System;
using System.Windows;

namespace NewgenWidget
{
    /// <summary>
    /// NewgenWidget
    /// </summary>
    //! Provides information about widget for Newgen and allows Newgen determine that this .dll file is a widget
    public class Widget : Newgen.Base.NewgenWidget
    {
        /// <summary>
        /// Name of the Widget, required to display in Newgen.
        /// </summary>
        public override string Name
        {
            get { return "Newgen Team"; }
        }

        /// <summary>
        /// Path to widget icon. Return null if there is no icon.
        /// </summary>
        public override Uri IconPath
        {
            get { return new Uri("/NewgenWidget;component/Resources/icon.png", UriKind.Relative); }
        }

        private WidgetControl widgetControl; //! Widget control
        /// <summary>
        /// Widget control.
        /// </summary>
        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        /// <summary>
        /// Number of horizontal cells occupied by widget. (1 or 2, more is possible but not recommended).
        /// </summary>
        public override int ColumnSpan
        {
            get { return 1; }
        }

        /// <summary>
        /// Widget initialization (e.g.variables initialization, reading settings, loading resources) must done be here.
        /// </summary>
        //! This method is called whenever the widget is loaded by Newgen.
        public override void Load()
        {
            widgetControl = new WidgetControl();

            widgetControl.Load();
        }

        /// <summary>
        /// Releasing resources and settings saving must be here.
        /// This method is called when the widget is unloaded by Newgen OR
        ///	user removes widget from Newgen tiles OR when user closes Newgen.
        /// </summary>
        public override void Unload()
        {
            widgetControl.Unload();
        }

        public override void HandleMessage(string message)
        {
            switch (message)
            {
                case "OpenHub":
                    widgetControl.InvokeHub();
                    break;
                default:

                    break;
            }
        }
    }
}