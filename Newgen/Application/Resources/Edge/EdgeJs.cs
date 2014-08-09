// NOTE: This is modified version of original file.

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newgen;

namespace EdgeJs {

    /// <summary>
    /// Class Edge.
    /// </summary>
    /// <remarks>...</remarks>
    public class Edge {

        /// <summary>
        /// The compile function
        /// </summary>
        private static Func<object, Task<object>> compileFunc;

        /// <summary>
        /// The initialized
        /// </summary>
        private static bool initialized;

        /// <summary>
        /// The location
        /// </summary>
        private static string location;

        /// <summary>
        /// The synchronize root
        /// </summary>
        private static object syncRoot = new object();

        /// <summary>
        /// The wait handle
        /// </summary>
        private static ManualResetEvent waitHandle = new ManualResetEvent(false);

        /// <summary>
        /// Gets the location.
        /// </summary>
        /// <value>The location.</value>
        /// <remarks>...</remarks>
        public static string Location {
            get {
                if (string.IsNullOrWhiteSpace(location))
                    location = Path.Combine(App.Current.Location, "Resources\\Edge");

                return location;
            }
        }

        /// <summary>
        /// Functions the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>Func&lt;System.Object, Task&lt;System.Object&gt;&gt;.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Unsupported architecture. Only x86 and x64 are supported.
        /// or
        /// Unable to initialize Node.js runtime.
        /// or
        /// Edge.Func cannot be used after Edge.Close had been called.
        /// </exception>
        /// <remarks>...</remarks>
        public static Func<object, Task<object>> Func(string code) {
            if (!initialized) {
                lock (syncRoot) {
                    if (!initialized) {
                        if (IntPtr.Size == 4) {
                            LoadLibrary(Location + @"\x86\node.dll");
                        }
                        else if (IntPtr.Size == 8) {
                            LoadLibrary(Location + @"\x64\node.dll");
                        }
                        else {
                            throw new InvalidOperationException(
                                "Unsupported architecture. Only x86 and x64 are supported.");
                        }

                        Thread v8Thread = new Thread(() => {
                            NodeStart(2, new string[] { "node", Location + "\\double_edge.js" });
                            waitHandle.Set();
                        });

                        v8Thread.IsBackground = true;
                        v8Thread.Start();
                        waitHandle.WaitOne();

                        if (!initialized) {
                            throw new InvalidOperationException("Unable to initialize Node.js runtime.");
                        }
                    }
                }
            }

            if (compileFunc == null) {
                throw new InvalidOperationException("Edge.Func cannot be used after Edge.Close had been called.");
            }

            var task = compileFunc(code);
            task.Wait();
            return (Func<object, Task<object>>)task.Result;
        }

        /// <summary>
        /// Initializes the internal.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Task&lt;System.Object&gt;.</returns>
        /// <remarks>...</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Task<object> InitializeInternal(object input) {
            compileFunc = (Func<object, Task<object>>)input;
            initialized = true;
            waitHandle.Set();

            return Task<object>.FromResult((object)null);
        }

        /// <summary>
        /// Loads the library.
        /// </summary>
        /// <param name="lpLibFileName">Name of the lp library file.</param>
        /// <returns>System.Int32.</returns>
        /// <remarks>...</remarks>
        [DllImport("kernel32.dll", EntryPoint = "LoadLibrary")]
        private static extern int LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpLibFileName);

        /// <summary>
        /// Nodes the start.
        /// </summary>
        /// <param name="argc">The argc.</param>
        /// <param name="argv">The argv.</param>
        /// <returns>System.Int32.</returns>
        /// <remarks>...</remarks>
        [DllImport("node.dll", EntryPoint = "#585", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NodeStart(int argc, string[] argv);
    }
}