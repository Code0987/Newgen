using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using libns;
using Newgen.AppLink;
using Newgen.HtmlApp;
using PackageManager;

namespace Newgen {

    /// <summary>
    /// Abstract for package definition
    /// </summary>
    /// <remarks>Use this class to mark a assembly as package.</remarks>
    public abstract class NewgenPackage : DynamicPackage {

        /// <summary>
        /// The package type identifier
        /// </summary>
        internal static readonly string PackageTypeId = "NewgenApp";

        /// <summary>
        /// The cached logo
        /// </summary>
        private BitmapSource cachedLogo;

        /// <summary>
        /// Gets the column span.
        /// </summary>
        /// <value>The column span.</value>
        /// <remarks>It defines the vertical span of tile.</remarks>
        public virtual int ColumnSpan { get { return 1; } }

        /// <summary>
        /// Gets the row span.
        /// </summary>
        /// <value>The row span.</value>
        /// <remarks>It defines the horizontal span of tile.</remarks>
        public virtual int RowSpan { get { return 1; } }

        /// <summary>
        /// Gets the package tile control.
        /// </summary>
        /// <value>The tile.</value>
        /// <remarks>Return a valid XAML control for display.</remarks>
        public abstract FrameworkElement Tile { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewgenPackage" /> class.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="settingsStorage">The settings storage.</param>
        /// <remarks>...</remarks>
        public NewgenPackage(string location, IPackageSettingsStorage settingsStorage)
            : base(location, settingsStorage) {
            // Package type marker.
            SettingsStorage.Put(this, PackageTypeId, PackageTypeId);
        }

        /// <summary>
        /// Disables this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public void Disable() {
            if (!GetSettings().IsEnabled)
                return;

            GetSettings().IsEnabled = false;

            Stop(true);

            Api.Logger.LogInformation(string.Format("Package [{0}] disabled.", this.GetId()));
        }

        /// <summary>
        /// Enables this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public void Enable() {
            if (GetSettings().IsEnabled)
                return;

            GetSettings().IsEnabled = true;

            Start(true);

            GetSettings().IsEnabled = true;

            Api.Logger.LogInformation(string.Format("Package [{0}] enabled.", this.GetId()));
        }

        /// <summary>
        /// Gets the logo.
        /// </summary>
        /// <param name="packageLocation">The package location.</param>
        /// <returns>BitmapSource.</returns>
        /// <exception cref="System.Exception">
        /// Package location not provided, skipping 1st step !
        /// or
        /// Couldn't load the logo from file !
        /// or
        /// Couldn't load the logo from resource !
        /// or
        /// Couldn't load the logo from conventional resource !
        /// </exception>
        /// <remarks>This functions check for three locations to get logo resource -
        /// 1. From file relative to package location
        /// 2. From relative or absolute uri set through 'Logo' property
        /// 3. From package assembly using convention '"pack://application:,,,/" + Id + ";component/Resources/Logo.png"'</remarks>
        public BitmapSource GetLogo(string packageLocation = null) {
            if (cachedLogo == null) {
                var id = this.GetId();
                var logo = Settings.OfType<NewgenPackageLogoSettings>().FirstOrDefault().Value;

                try {
                    if (string.IsNullOrWhiteSpace(logo))
                        throw new Exception("Package location not provided, skipping 1st step !");
                    cachedLogo = new BitmapImage(new Uri(Path.Combine(packageLocation, logo), UriKind.RelativeOrAbsolute));
                    if (cachedLogo == null)
                        throw new Exception("Couldn't load the logo from file !");
                }
                catch /* Eat */ {
                    try {
                        cachedLogo = new BitmapImage(new Uri(logo, UriKind.RelativeOrAbsolute));
                        if (cachedLogo == null)
                            throw new Exception("Couldn't load the logo from resource !");
                    }
                    catch /* Eat */ {
                        try {
                            var resourcePath = new Uri("pack://application:,,,/" + id + "Package;component/Resources/Logo.png", UriKind.RelativeOrAbsolute);
                            cachedLogo = new BitmapImage(resourcePath);
                            if (cachedLogo == null)
                                throw new Exception("Couldn't load the logo from conventional resource !");
                            logo = resourcePath.OriginalString;
                        }
                        catch /* Eat */ {
                            cachedLogo = new BitmapImage(new Uri("pack://application:,,,/Newgen.Core;component/Resources/NWP_Icon.ico", UriKind.RelativeOrAbsolute));
                        }
                    }
                }
            }

            return cachedLogo;
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <returns>PackageManagerSettings.</returns>
        /// <remarks>...</remarks>
        public NewgenPackageSettings GetSettings() {
            return Settings
                .OfType<NewgenPackageSettingsSettings>()
                .FirstOrDefault()
                .Settings;
        }

        /// <summary>
        /// Loads settings.
        /// Load all package related settings here.
        /// </summary>
        /// <remarks>...</remarks>
        public override void Load() {
            base.Load();

            if (!Settings.OfType<NewgenPackageLogoSettings>().Any())
                Settings.Add(new NewgenPackageLogoSettings(this));

            if (!Settings.OfType<NewgenPackageSettingsSettings>().Any())
                Settings.Add(new NewgenPackageSettingsSettings(this));
        }

        /// <summary>
        /// Called whenever a message is received.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <remarks>...</remarks>
        public virtual void OnMessageReceived(EMessage message) {
        }

        /// <summary>
        /// Removes this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public void Remove() {
            Stop(true);

            // TODO: Let PM handle this.

            Api.Logger.LogInformation(string.Format("Package [{0}] removed.", this.GetId()));
        }

        /// <summary>
        /// Starts the specified force.
        /// </summary>
        /// <param name="force">if set to <c>true</c> [force].</param>
        /// <remarks>...</remarks>
        public void Start(bool force = false) {
            OnBeforeStart();

            if (!force && !GetSettings().IsEnabled)
                return;

            OnStart();
            OnAfterStart();

            Api.Logger.LogInformation(string.Format("Package [{0}] Started.", this.GetId()));
        }

        /// <summary>
        /// Stops the specified force.
        /// </summary>
        /// <param name="force">if set to <c>true</c> [force].</param>
        /// <remarks>...</remarks>
        public void Stop(bool force = false) {
            if (!force && !GetSettings().IsEnabled)
                return;

            OnBeforeStop();
            OnStop();
            OnAfterStop();

            Api.Logger.LogInformation(string.Format("Package [{0}] Stopped.", this.GetId()));
        }

        /// <summary>
        /// Toggles the enabled.
        /// </summary>
        /// <remarks>...</remarks>
        public void ToggleEnabled() {
            if (!GetSettings().IsEnabled)
                Enable();
            else
                Disable();
        }

        /// <summary>
        /// Called when [after start].
        /// </summary>
        /// <remarks>...</remarks>
        protected virtual void OnAfterStart() {
            App.Screen.TilesControl.Place(this);
        }

        /// <summary>
        /// Called when [after stop].
        /// </summary>
        /// <remarks>...</remarks>
        protected virtual void OnAfterStop() {
            App.Screen.TilesControl.DePlace(this);

            Save();
        }

        /// <summary>
        /// Called when [before start].
        /// </summary>
        /// <remarks>...</remarks>
        protected virtual void OnBeforeStart() {
            Load();
        }

        /// <summary>
        /// Called when [before stop].
        /// </summary>
        /// <remarks>...</remarks>
        protected virtual void OnBeforeStop() {
        }

        /// <summary>
        /// Called when [start].
        /// </summary>
        /// <remarks>...</remarks>
        protected abstract void OnStart();

        /// <summary>
        /// Called when [stop].
        /// </summary>
        /// <remarks>...</remarks>
        protected abstract void OnStop();
    }

    /// <summary>
    /// Class NewgenPackageLogoSettings.
    /// </summary>
    /// <remarks>...</remarks>
    public class NewgenPackageLogoSettings : PackageSettings {

        /// <summary>
        /// Initializes a new instance of the <see cref="NewgenPackageLogoSettings"/> class.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <remarks>...</remarks>
        public NewgenPackageLogoSettings(NewgenPackage package)
            : base(package) {
        }
    }

    /// <summary>
    /// Class NewgenPackageManagerRuntimeSettings.
    /// </summary>
    /// <remarks>...</remarks>
    public class NewgenPackageManagerRuntimeSettings : PackageManagerRuntimeSettings {

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageManagerRuntimeSettings" /> class.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <remarks>...</remarks>
        public NewgenPackageManagerRuntimeSettings(Package package)
            : base(package) {
        }

        /// <summary>
        /// Determines whether the specified location is supported.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns><c>true</c> if the specified location is supported; otherwise, <c>false</c>.</returns>
        /// <remarks>...</remarks>
        public override bool IsProxificationSupportedOn(Package package) {
            return
                package.SettingsStorage.Has(package, HtmlAppPackage.PackageTypeId)
                ||
                package.SettingsStorage.Has(package, AppLinkPackage.PackageTypeId)
                ||
                package.SettingsStorage.Has(package, NewgenPackage.PackageTypeId)
                ;
        }

        /// <summary>
        /// Creates the runtime proxy for package.
        /// Provide runtime specific settings to package here.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns>Package.</returns>
        /// <remarks>...</remarks>
        public override Package Proxify(Package package) {
            // Stop old package.
            package.Save();

            // Find internal package type.
            if (package.SettingsStorage.Has(package, HtmlAppPackage.PackageTypeId)) {
                var newPackage = new HtmlAppPackage(package.Location, package.SettingsStorage);
                return newPackage;
            }
            if (package.SettingsStorage.Has(package, AppLinkPackage.PackageTypeId)) {                
                var newPackage = new AppLinkPackage(package.Location, package.SettingsStorage);
                return newPackage;
            }
            if (package.SettingsStorage.Has(package, NewgenPackage.PackageTypeId)) {
                // 1. Find the compiled assembly.
                var packageAssemblyPath = Path.Combine(package.Location, package.GetId() + ".dll");

                // 2. Load assembly into memory.
                var packageAssembly = Assembly.LoadFrom(packageAssemblyPath);

                // 3. Find, create, load package.
                var newPackage = Activator.CreateInstance(
                    packageAssembly.GetTypes().FirstOrDefault(f => typeof(Package).IsAssignableFrom(f)),
                    (object)package.Location,
                    package.SettingsStorage
                    ) as NewgenPackage;
                if (newPackage != null)
                {
                    newPackage.ProxyProvider = Package;
                    newPackage.Load();

                    return newPackage;
                }
            }

            return base.Proxify(package);
        }
    }

    /// <summary>
    /// Class NewgenPackageSettings.
    /// </summary>
    /// <remarks>...</remarks>
    public class NewgenPackageSettings {

        /// <summary>
        /// The customized key
        /// </summary>
        public const string CustomizedKey = "Customized";

        /// <summary>
        /// The package metadata mark
        /// </summary>
        public static readonly string CacheFilename = ".pkg-settings";

        /// <summary>
        /// Gets or sets the column.
        /// </summary>
        /// <value>The column.</value>
        /// <remarks>...</remarks>
        public int Column { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value><c>true</c> if this instance is enabled; otherwise, <c>false</c> .</value>
        /// <remarks>...</remarks>
        public bool IsEnabled { get; set; }

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
        /// Initializes a new instance of the <see cref="NewgenPackageSettings"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        public NewgenPackageSettings() {
            ObjectData = new Dictionary<string, string>();
            Column = -1;
            Row = -1;
            IsEnabled = true;
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
            var value = Activator.CreateInstance<T>();
            try {
                var value_des = ObjectData[key].DeserializeFromJavascript<T>();
                if (value_des != null)
                    value = value_des;
            }
            catch (Exception ex) { Api.Logger.LogError("Unable to read previous settings !", ex); }
            if (customizer != null) {
                customizer(value);
                ObjectData[key] = value.SerializeToJavascript();
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
                ObjectData[key] = value.SerializeToJavascript();
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
        public T Customize<T>(T value, string key = CustomizedKey) {
            return Customize(key, value);
        }

        /// <summary>
        /// Customize.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="customizer">The customizer.</param>
        /// <param name="key">The key.</param>
        /// <returns>T.</returns>
        /// <remarks>...</remarks>
        public T Customize<T>(Action<T> customizer = null, string key = CustomizedKey) {
            return Customize(key, customizer);
        }
    }

    /// <summary>
    /// Class NewgenPackageSettingsSettings.
    /// </summary>
    /// <remarks>...</remarks>
    public class NewgenPackageSettingsSettings : PackageSettings {

        /// <summary>
        /// The settings
        /// </summary>
        private NewgenPackageSettings settings;

        /// <summary>
        /// Gets or sets the authors.
        /// </summary>
        /// <value>The authors.</value>
        /// <remarks>...</remarks>
        public NewgenPackageSettings Settings {
            get {
                if (settings == null)
                    settings = new NewgenPackageSettings();
                return settings;
            }
            internal set {
                settings = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageSettings" /> class.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <remarks>...</remarks>
        public NewgenPackageSettingsSettings(NewgenPackage package)
            : base(package) {
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public override void Load() {
            base.Load();

            Settings = Value.DeserializeFromJavascript<NewgenPackageSettings>();
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public override void Save() {
            Value = Settings.SerializeToJavascript();

            base.Save();
        }
    }
}