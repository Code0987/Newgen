﻿using System;
using System.Windows;
using Newgen.Base;

namespace TemplateWidget
{
    /// <summary>
    /// Widget Definition.
    /// </summary>
    public class Widget : NewgenWidget
    {
        public const string WIDGET_NAME     = "$safeprojectname$";

        internal static string SettingsFile = E.CreateSettingsPathForWidget(WIDGET_NAME);
        internal static Settings Settings;

        /// <summary>
        /// Name of the Widget, required to display in Newgen.
        /// </summary>
        public override string Name { get { return WIDGET_NAME; } }

        /// <summary>
        /// Path to widget icon. Return null if there is no icon.
        /// </summary>
        public override Uri IconPath { get { return new Uri("/" + WIDGET_NAME + ";component/Resources/icon.png", UriKind.Relative); } }

        private Tile widgetControl;

        /// <summary>
        /// Widget control.
        /// </summary>
        public override FrameworkElement WidgetControl { get { return widgetControl; } }

        /// <summary>
        /// Number of horizontal cells occupied by widget. (1 or 2, more is possible but not recommended).
        /// </summary>
        public override int ColumnSpan { get { return 1; } }

        /// <summary>
        /// Widget initialization (e.g.variables initialization, reading settings, loading resources) must done be here.
        /// </summary>
        //! This method is called whenever the widget is loaded by Newgen.
        public override void Load()
        {
            Settings      = (Settings)XmlSerializable.Load(typeof(Settings), SettingsFile) ?? new Settings();
            widgetControl = new Tile();

            widgetControl.Load();
        }

        /// <summary>
        /// Releasing resources and settings saving must be here.
        /// This method is called when the widget is unloaded by Newgen OR
        ///	user removes widget from Newgen tiles OR when user closes Newgen.
        /// </summary>
        public override void Unload()
        {
            Settings.Save(SettingsFile);

            widgetControl.Unload();
        }
    }
}