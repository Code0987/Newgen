using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ionic.Zip;

/// <summary>
/// The Core namespace.
/// </summary>
/// <remarks>...</remarks>
namespace Newgen.Packages {

    /// <summary>
    /// Class WidgetManager.
    /// </summary>
    /// <remarks>...</remarks>
    public class PackageManager {

        /// <summary>
        /// The post remove mark
        /// </summary>
        public const string PostRemoveFilename = "Package.Remove";

        /// <summary>
        /// The post update cache folder
        /// </summary>
        public const string PostUpdateCacheFoldername = "Package.Update[Content]";

        /// <summary>
        /// The post update mark
        /// </summary>
        public const string PostUpdateFilename = "Package.Update";

        /// <summary>
        /// The current
        /// </summary>
        private static PackageManager current;

        /// <summary>
        /// The cache
        /// </summary>
        private List<Package> packages;

        /// <summary>
        /// Occurs when [loaded].
        /// </summary>
        /// <remarks>...</remarks>
        public event Action<Package> Loaded;

        /// <summary>
        /// Occurs when [unloaded].
        /// </summary>
        /// <remarks>...</remarks>
        public event Action<Package> Unloaded;

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>The current.</value>
        /// <remarks>...</remarks>
        public static PackageManager Current {
            get { return current ?? (current = new PackageManager()); }
        }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        /// <remarks>...</remarks>
        public string Location { get; set; }

        /// <summary>
        /// Gets the cache.
        /// </summary>
        /// <value>The cache.</value>
        /// <remarks>...</remarks>
        public List<Package> Packages {
            get { return packages ?? (packages = new List<Package>()); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageManager" /> class.
        /// </summary>
        /// <remarks>...</remarks>
        public PackageManager() {
            PackageServer.Current.Start();

            Location = E.PackagesRoot;
        }

        /// <summary>
        /// Creates the absolute path for.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="relativePath">The relative path.</param>
        /// <returns>System.String.</returns>
        /// <remarks>...</remarks>
        public string CreateAbsolutePathFor(string packageId, string relativePath = "") {
            var path = Path.Combine(Location, packageId, relativePath);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// Disables the specified package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <remarks>...</remarks>
        public void Disable(Package package) {
            if (!package.Settings.IsEnabled)
                return;

            Unload(package);

            package.Settings.IsEnabled = false;
        }

        /// <summary>
        /// Enables the specified package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <remarks>...</remarks>
        public void Enable(Package package) {
            if (package.Settings.IsEnabled)
                return;

            package.Settings.IsEnabled = true;

            Load(package);
        }

        /// <summary>
        /// Unpacks the package.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="packageFile">The package file.</param>
        /// <remarks>...</remarks>
        public PackageManager Extract(string packageId, string packageFile) {
            if (IsInitialized(packageId))
                return this;

            var pf = new FileInfo(packageFile);
            if (!pf.Exists)
                return this;

            var packageFolder = CreateAbsolutePathFor(packageId);
            var packageFolderPostUpdate = CreateAbsolutePathFor(packageId, PostUpdateCacheFoldername);
            try {
                using (var zip = ZipFile.Read(packageFile)) {
                    foreach (var entry in zip) {
                        try {
                            entry.Extract(packageFolder, ExtractExistingFileAction.Throw);
                        }
                        catch {
                            entry.Extract(packageFolderPostUpdate, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                }
            }
            catch {
            }
            File.Create(CreateAbsolutePathFor(packageId, PostUpdateFilename));

            return this;
        }

        /// <summary>
        /// Gets the specified package identifier.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <returns>Package.</returns>
        /// <remarks>...</remarks>
        public Package Get(string packageId) {
            return Packages.FirstOrDefault(f => f.Metadata.Id.Equals(packageId));
        }

        /// <summary>
        /// Initializes from.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns>Package.</returns>
        /// <remarks>...</remarks>
        public Package InitializeFrom(Package package) {
            Packages.Add(package);
            return package;
        }

        /// <summary>
        /// Loads the by path.
        /// </summary>
        /// <param name="location">The path.</param>
        /// <param name="metadata">The metadata.</param>
        /// <returns>Package.</returns>
        /// <remarks>...</remarks>
        public Package InitializeFrom(string location) {

            // Scan app link
            try {
                return AppLink.AppLinkPackage.CreateFrom(location);
            }
            catch { }

            // Scan .net compiled packages
            try {
                var filePaths = Directory.GetFiles(location, "*.dll", SearchOption.TopDirectoryOnly);
                foreach (var filePath in filePaths) {

                    // Create instance
                    var package = Activator.CreateInstance(
                        Assembly
                        .LoadFrom(filePath)
                        .GetTypes()
                        .FirstOrDefault(f => typeof(Package).IsAssignableFrom(f)),
                        (object)location
                        ) as Package;

                    // Cache
                    package = InitializeFrom(package);

                    // Done !
                    return package; // Only one widgets per package !
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Determines whether the specified package identifier is enabled.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <returns><c>true</c> if the specified package identifier is enabled; otherwise, <c>false</c>.</returns>
        /// <remarks>...</remarks>
        public bool IsEnabled(string packageId) {
            var package = Get(packageId);
            return (package != null && IsEnabled(package));
        }

        /// <summary>
        /// Determines whether the specified package is enabled.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns><c>true</c> if the specified package is enabled; otherwise, <c>false</c>.</returns>
        /// <remarks>...</remarks>
        public bool IsEnabled(Package package) {
            return package.Settings.IsEnabled;
        }

        /// <summary>
        /// Determines whether the specified package identifier is cached.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <returns><c>true</c> if the specified package identifier is cached; otherwise, <c>false</c>.</returns>
        /// <remarks>...</remarks>
        public bool IsInitialized(string packageId) {
            return Packages.Any(f => f.Metadata.Id.Equals(packageId));
        }

        /// <summary>
        /// Determines whether [is update available for] [the specified package identifier].
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <returns>
        /// <c>true</c> if [is update available for] [the specified package identifier]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>...</remarks>
        public bool IsUpdateAvailableFor(string packageId) {
            var metadata = Get(packageId).Metadata;
            if (metadata == null)
                return false;
            var feed = InternalHelper.FeedsAggregator.CachedFeeds
                .OrderByDescending(f => f.LastUpdatedTime).FirstOrDefault(f => f.Id.Equals(packageId));
            if (feed == null) return false;
            return feed.LastUpdatedTime > metadata.Version;
        }
        /// <summary>
        /// Loads the specified package identifier.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <remarks>...</remarks>
        public void Load(string packageId) {
            if (!IsInitialized(packageId))
                return;

            Load(Packages.First(f => f.Metadata.Id.Equals(packageId)));
        }

        /// <summary>
        /// Loads the specified package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <remarks>...</remarks>
        public void Load(Package package) {
            if (!package.Settings.IsEnabled)
                return;

            if (!IsInitialized(package.Metadata.Id))
                InitializeFrom(package);

            package.Load();

            if (Loaded != null)
                Loaded(package);
        }

        /// <summary>
        /// Loads all.
        /// </summary>
        /// <remarks>...</remarks>
        public void LoadAll() {
            foreach (var package in Packages)
                Load(package);
        }

        /// <summary>
        /// Posts the process.
        /// </summary>
        /// <returns>PackageManager.</returns>
        /// <remarks>...</remarks>
        public void PostProcess() {

            // Scan
            var packageFolders = Directory.GetDirectories(Location);

            // Post functions
            foreach (var packageFolder in packageFolders) {
                try {

                    // Get all files
                    var filePaths = Directory.GetFiles(packageFolder, "*", SearchOption.TopDirectoryOnly);
                    foreach (var filePath in filePaths) {
                        var fi = new FileInfo(filePath);

                        // Remove if marked
                        if (fi.Name == PostRemoveFilename) {
                            Directory.Delete(packageFolder, true);
                        }

                        // Update if marked
                        else if (fi.Name == PostUpdateFilename) {
                            foreach (var f in Directory.GetFiles(packageFolder + "\\" + PostUpdateCacheFoldername)) {
                                File.Copy(f, packageFolder + "\\" + new FileInfo(f).Name, true);
                                File.Delete(f);
                            }

                            // Remove mark
                            File.Delete(fi.FullName);
                        }
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Removes the specified package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <remarks>...</remarks>
        public void Remove(Package package) {
            try {
                Unload(package);
                packages.Remove(package);

                Directory.Delete(CreateAbsolutePathFor(package.Metadata.Id), true);
            }
            catch {
                File.Create(CreateAbsolutePathFor(package.Metadata.Id, PostRemoveFilename));
            }
        }

        /// <summary>
        /// Removes the package.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <remarks>...</remarks>
        public void Remove(string packageId) {
            if (!IsInitialized(packageId))
                return;

            Remove(Get(packageId));
        }

        /// <summary>
        /// Scans this instance.
        /// </summary>
        /// <returns>Task.</returns>
        /// <remarks>...</remarks>
        public async void Scan() {
            await Task
                .Run((Action)PostProcess)
                .ContinueWith(t => {

                    // Clear cache
                    Packages.Clear();

                    // Scan
                    var packageFolders = Directory.GetDirectories(Location);

                    // Post functions
                    foreach (var packageFolder in packageFolders) {
                        var package = InitializeFrom(packageFolder);
                        Load(package);
                    }
                });
        }

        /// <summary>
        /// Toggles the enabled.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <remarks>...</remarks>
        public void ToggleEnabled(Package package) {
            if (!package.Settings.IsEnabled) {
                Enable(package);
            }
            else {
                Disable(package);
            }

        }

        /// <summary>
        /// Unloads the specified package identifier.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <remarks>...</remarks>
        public void Unload(string packageId) {
            if (!IsInitialized(packageId))
                return;

            Unload(Packages.First(f => f.Metadata.Id.Equals(packageId)));
        }

        /// <summary>
        /// Unloads the specified package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <remarks>...</remarks>
        public void Unload(Package package) {
            if (!package.Settings.IsEnabled)
                return;

            if (Unloaded != null)
                Unloaded(package);

            package.Unload();
        }

        /// <summary>
        /// Uns the load all.
        /// </summary>
        /// <remarks>...</remarks>
        public void UnloadAll() {
            foreach (var package in Packages)
                Unload(package);
        }
    }
}