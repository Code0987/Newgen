using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.WindowsAPICodePack.Shell;
using Newgen.Base;

namespace Video
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

        public Tuple<List<Category>, List<string>> GetVideos(int max = int.MaxValue)
        {
            var videosExtensions = new string[] { ".avi", ".wmv", ".mp4" };
            var categories = new List<Category>();
            var videos = new List<string>();
            if(LookupDirectories.Count == 0)
            {
                if(!ShellLibrary.IsPlatformSupported)
                    LookupDirectories.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
                else
                    using(var library = ShellLibrary.Load(KnownFolders.VideosLibrary, true))
                        foreach(var folder in library)
                            LookupDirectories.Add(folder.Path);
            }
            foreach(var directory in LookupDirectories)
                foreach(var file in Directory.GetFiles(directory))
                {
                    if(videos.Count >= max)
                        break;

                    var fi = new FileInfo(file);

                    if(!videosExtensions.Contains(Path.GetExtension(file)))
                        continue;

                    var name = Path.GetFileNameWithoutExtension(file);
                    var category0 = categories.Find(x => x.Title == name[0]);
                    if(category0 == null)
                    {
                        var category = new Category();
                        category.Title = name[0];
                        category.Files.Add(file);
                        categories.Add(category);
                    }
                    else
                    {
                        category0.Files.Add(file);
                    }
                    videos.Add(file);
                }
            return Tuple.Create(categories, videos);
        }
    }
}