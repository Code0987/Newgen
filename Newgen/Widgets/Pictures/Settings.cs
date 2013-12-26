using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.WindowsAPICodePack.Shell;
using Newgen.Base;

namespace Pictures
{
    /// <summary>
    /// Settings
    /// </summary>
    public class Settings : XmlSerializable
    {
        public List<string> LookupDirectories { get; set; }

        /// <summary>
        /// Initializes settings.
        /// </summary>
        public Settings()
        {
            this.LookupDirectories = new List<string>();
        }

        public Tuple<List<Category>, List<string>> GetImages(int max = int.MaxValue)
        {
            var imagesExtensions = new string[] { ".jpg", ".jpeg", ".png" };
            var categories = new List<Category>();
            var images = new List<string>();
            if(LookupDirectories.Count == 0)
            {
                if(!ShellLibrary.IsPlatformSupported)
                    LookupDirectories.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
                else
                    using(var library = ShellLibrary.Load(KnownFolders.PicturesLibrary, true))
                        foreach(var folder in library)
                            LookupDirectories.Add(folder.Path);
            }
            foreach(var directory in LookupDirectories)
                foreach(var file in Directory.GetFiles(directory))
                {
                    if(images.Count >= max)
                        break;

                    var fi = new FileInfo(file);

                    if(!imagesExtensions.Contains(Path.GetExtension(file)))
                        continue;

                    var year = fi.CreationTimeUtc.Year.ToString().ToUpper();
                    var category0 = categories.Find(x => x.Title.ToString().ToUpper() == year);
                    if(category0 == null)
                    {
                        var category = new Category();
                        category.Title = year;
                        category.Files.Add(file);
                        categories.Add(category);
                    }
                    else
                    {
                        category0.Files.Add(file);
                    }
                    images.Add(file);
                }
            return Tuple.Create(categories, images);
        }
    }
}