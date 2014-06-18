using System;
using System.Collections.Generic;
using System.IO;
using libns;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;

namespace Newgen.Packages {
    /// <summary>
    /// Class PackageSettings.
    /// </summary>
    /// <remarks>...</remarks>
    public class PackageSettings {
        /// <summary>
        /// The package metadata mark
        /// </summary>
        public static readonly string CacheFilename = "Package.Settings";

        /// <summary>
        /// Gets or sets the column.
        /// </summary>
        /// <value>The column.</value>
        /// <remarks>...</remarks>
        public int Column { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value><c>true</c> if this instance is enabled; otherwise, <c>false</c>.</value>
        /// <remarks>...</remarks>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        /// <remarks>...</remarks>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the object data.
        /// </summary>
        /// <value>The object data.</value>
        /// <remarks>...</remarks>
        public Dictionary<string, string> ObjectData { get; set; }

        /// <summary>
        /// Gets or sets the row.
        /// </summary>
        /// <value>The row.</value>
        /// <remarks>...</remarks>
        public int Row { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageSettings" /> class.
        /// </summary>
        /// <remarks>...</remarks>
        public PackageSettings() {
            ObjectData = new Dictionary<string, string>();
            Column = -1;
            Row = -1;
            Location = string.Empty;
            IsEnabled = true;
        }

        /// <summary>
        /// Creates the absolute path for.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns>System.String.</returns>
        /// <remarks>...</remarks>
        public string CreateAbsolutePathFor(string relativePath) {
            return Path.Combine(Location, relativePath);
        }

        /// <summary>
        /// Customize.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="customizer">The customizer.</param>
        /// <returns>T.</returns>
        /// <remarks>...</remarks>
        public T Customize<T>(string key, Action<T> customizer = null) {
            var value = (T)Activator.CreateInstance(typeof(T));
            try {
                value = ObjectData[key].DeserializeFromJavascript<T>();
            }
            catch (Exception ex) { Api.Logger.LogError("Unable to read previous settings !", ex); }
            if (customizer != null) {
                customizer(value);
                ObjectData[key] = value.SerializeToJavascript<T>();
            }
            return value;
        }

        /// <summary>
        /// Customizes the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>T.</returns>
        /// <remarks>...</remarks>
        public T Customize<T>(string key, T value) {
            if (value != null) {
                ObjectData[key] = value.SerializeToJavascript<T>();
            }
            return value;
        }

        /// <summary>
        /// Customizes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="key">The key.</param>
        /// <returns>T.</returns>
        /// <remarks>...</remarks>
        public T Customize<T>(T value, string key = "Customized") {
            return Customize<T>(key, value);
        }

        /// <summary>
        /// Customize.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="customizer">The customizer.</param>
        /// <param name="key">The key.</param>
        /// <returns>T.</returns>
        /// <remarks>...</remarks>
        public T Customize<T>(Action<T> customizer = null, string key = "Customized") {
            return Customize<T>(key, customizer);
        }
    }
}