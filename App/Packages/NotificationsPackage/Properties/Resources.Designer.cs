﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34011
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NotificationsPackage.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("NotificationsPackage.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Notifications.
        /// </summary>
        public static string Notifications {
            get {
                return ResourceManager.GetString("Notifications", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to External.
        /// </summary>
        public static string TileContextMenuItemExternal {
            get {
                return ResourceManager.GetString("TileContextMenuItemExternal", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to External Browser App Command.
        /// </summary>
        public static string TileContextMenuItemExternalCommand {
            get {
                return ResourceManager.GetString("TileContextMenuItemExternalCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Notifications Explorer (embedded).
        /// </summary>
        public static string TileContextMenuItemIE {
            get {
                return ResourceManager.GetString("TileContextMenuItemIE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Node Webkit.
        /// </summary>
        public static string TileContextMenuItemNW {
            get {
                return ResourceManager.GetString("TileContextMenuItemNW", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to launch external browser !.
        /// </summary>
        public static string UnableToLaunchExternalBrowser {
            get {
                return ResourceManager.GetString("UnableToLaunchExternalBrowser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to run NW !.
        /// </summary>
        public static string UnableToRunNW {
            get {
                return ResourceManager.GetString("UnableToRunNW", resourceCulture);
            }
        }
    }
}