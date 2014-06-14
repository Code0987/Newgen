using System;
using System.IO;

namespace Newgen.Packages {
    /// <summary>
    /// Widget redistribution definition
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
    }
}