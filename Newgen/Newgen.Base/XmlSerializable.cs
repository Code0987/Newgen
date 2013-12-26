using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Newgen.Base
{
    /// <summary>
    /// Abstract for Xml serialized objects
    /// </summary>
    public abstract class XmlSerializable
    {
        /// <summary>
        /// Saves to the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public virtual void Save(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            try
            {
                XmlSerializer slz = new XmlSerializer(this.GetType());
                slz.Serialize(fs, this);
            }
            catch { }
            finally { fs.Close(); }
        }

        /// <summary>
        /// Loads the specified t.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static object Load(Type t, string path)
        {
            if (File.Exists(path))
            {
                var sr = new StreamReader(path);
                var xr = new XmlTextReader(sr);
                var xs = new XmlSerializer(t);
                object result = null;
                try
                {
                    result = xs.Deserialize(xr);
                }
                catch { }
                xr.Close();
                sr.Close();
                return result;
            }
            return null;
        }
    }
}