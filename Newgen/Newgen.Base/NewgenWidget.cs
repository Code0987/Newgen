using System;
using System.Windows;

namespace Newgen.Base
{
    /// <summary>
    /// Abstract for Newgen widget definition
    /// </summary>
    public abstract class NewgenWidget
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the widget control.
        /// </summary>
        public abstract FrameworkElement WidgetControl { get; }

        /// <summary>
        /// Gets the icon path.
        /// </summary>
        public abstract Uri IconPath { get; }

        /// <summary>
        /// Gets the column span.
        /// </summary>
        public abstract int ColumnSpan { get; }

        /// <summary>
        /// Gets the X.
        /// </summary>
        public virtual int X { get; private set; }

        /// <summary>
        /// Gets the Y.
        /// </summary>
        public virtual int Y { get; private set; }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        public virtual void Load() { }

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        public virtual void Unload() { }

        /// <summary>
        /// Loads from the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public virtual void Load(string path) { }

        /// <summary>
        /// Load.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="name">The name.</param>
        /// <param name="seed">The seed.</param>
        public virtual void Load(string id, string name, int seed) { }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public virtual void Refresh() { }

        /// <summary>
        /// Handles the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public virtual void HandleMessage(string message) { }
    }
}