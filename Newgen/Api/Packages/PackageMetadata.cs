using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Newgen.Packages {
    /// <summary>
    /// Package redistribution definition
    /// </summary>
    [Serializable]
    public class PackageMetadata {
        /// <summary>
        /// The package metadata mark
        /// </summary>
        public static readonly string CacheFilename = "Package.Metadata";

        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        /// <value>
        /// The author.
        /// </value>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the author website.
        /// </summary>
        /// <value>The author website.</value>
        /// <remarks>...</remarks>
        public string AuthorEMailAddress { get; set; }

        /// <summary>
        /// Gets or sets the author web.
        /// </summary>
        /// <value>
        /// The author web.
        /// </value>
        public string AuthorWebsite { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        /// <value>
        /// The ID.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the license.
        /// </summary>
        /// <value>The license.</value>
        /// <remarks>...</remarks>
        public string License { get; set; }

        /// <summary>
        /// Gets or sets the logo.
        /// </summary>
        /// <value>The logo.</value>
        /// <remarks>...</remarks>
        public string Logo { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public DateTime Version { get; set; }

        /// <summary>
        /// The cached logo
        /// </summary>
        private BitmapSource cachedLogo;

        /// <summary>
        /// Gets the logo.
        /// </summary>
        /// <returns>BitmapSource.</returns>
        /// <remarks>
        /// This functions check for three locations to get logo resource -
        /// 1. From file relative to package location
        /// 2. From relative or absolute uri set through 'Logo' property
        /// 3. From package assembly using convention '"pack://application:,,,/" + Id + ";component/Resources/Logo.png"'
        /// </remarks>
        public BitmapSource GetLogo(string packageLocation = null) {
            if (cachedLogo == null) {
                try {
                    if (string.IsNullOrWhiteSpace(Logo))
                        throw new Exception("Package location not provided, skipping 1st step !");
                    cachedLogo = new BitmapImage(new Uri(Path.Combine(packageLocation, Logo), UriKind.RelativeOrAbsolute));
                    if (cachedLogo == null)
                        throw new Exception("Couldn't load the logo from file !");
                }
                catch /* Eat */ {
                    try {
                        cachedLogo = new BitmapImage(new Uri(Logo, UriKind.RelativeOrAbsolute));
                        if (cachedLogo == null)
                            throw new Exception("Couldn't load the logo from resource !");
                    }
                    catch /* Eat */ {
                        try {
                            var resourcePath = new Uri("pack://application:,,,/" + Id + "Package;component/Resources/Logo.png", UriKind.RelativeOrAbsolute);
                            cachedLogo = new BitmapImage(resourcePath);
                            if (cachedLogo == null)
                                throw new Exception("Couldn't load the logo from conventional resource !");
                            Logo = resourcePath.OriginalString;
                        }
                        catch /* Eat */ {
                            cachedLogo = new BitmapImage(new Uri("pack://application:,,,/Newgen;component/Resources/NWP_Icon.ico", UriKind.RelativeOrAbsolute));
                        }
                    }
                }
            }

            return cachedLogo;
        }
    }
}