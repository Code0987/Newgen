using System;

namespace Newgen {

    /// <summary>
    /// Program.
    /// </summary>
    /// <remarks>Proxy to run <see cref="Newgen.App"/>, as to avoid circular dependencies on official packages.</remarks>
    public static class Program {

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <remarks>...</remarks>
        [STAThread]
        public static void Main(string[] args) {
            Newgen.App.Main(args);
        }
    }
}