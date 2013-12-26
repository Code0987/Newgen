using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Shell;
using Newgen.Native;

namespace Newgen.Native
{
    public class IconExtractor
    {
        public static ImageSource GetThumbnail(string path)
        {
            var source = default(BitmapSource);
            try
            {
                // For Vista +
                if(ShellFile.IsPlatformSupported)
                {
                    // For file icon/thumbnail.
                    if(File.Exists(path))
                        try
                        {
                            source = ShellFile.FromFilePath(path).Thumbnail.LargeBitmapSource;
                        }
                        catch
                        {
                            source = ShellFile.FromFilePath(path).Thumbnail.BitmapSource;
                        }

                    // For folder icon/thumbnail.
                    else if(Directory.Exists(path))
                        try
                        {
                            source = ShellFolder.FromParsingName(path).Thumbnail.LargeBitmapSource;
                        }
                        catch
                        {
                            source = ShellFolder.FromParsingName(path).Thumbnail.BitmapSource;
                        }
                }

                // For XP
                if(source == null)
                {
                    var bmp = default(Bitmap);

                    //// First check if thumbnail is available.
                    //if(File.Exists(path))
                    //    try
                    //    {
                    //        bmp = (new ShellThumbnail()
                    //        {
                    //            DesiredSize = new System.Drawing.Size(128, 128)
                    //        })
                    //        .GetThumbnail(path);
                    //    }
                    //    catch
                    //    {
                    //        bmp = null;
                    //    }

                    // If no thumbnail is available, check for large size icon, then for small.
                    if(bmp == null)
                    {
                        try
                        {
                            bmp = (Bitmap)ShellThumbnail.GetLargeFileIcon(path).ToBitmap();
                        }
                        catch
                        {
                            bmp = (Bitmap)ShellThumbnail.GetSmallFileIcon(path).ToBitmap();
                        }
                    }
                    source = CreateBitmapSourceFromBitmap(bmp);
                }
            }
            catch
            {
                source = new BitmapImage(new Uri("/OOFMS;component/Resources/UnknownFile.ocp", UriKind.Relative));
            }

            return source;
        }

        private static Tuple<ImageSource, int>[] GetAllIcons(string path)
        {
            var icons = new List<Tuple<ImageSource, int>>();
            var count = 0;
            while(true)
            {
                var icon = ShellThumbnail.ExtractIconFromExe(path, true, count);

                if(icon == null)
                    break;

                icons.Add(Tuple.Create(
                    CreateBitmapSourceFromBitmap(icon.ToBitmap()) as ImageSource,
                    count
                    ));

                count++;
            }
            return icons.ToArray();
        }

        private static BitmapSource CreateBitmapSourceFromBitmap(System.Drawing.Bitmap bitmap)
        {
            if(bitmap == null)
                throw new ArgumentNullException("bitmap");

            if(Application.Current.Dispatcher == null)
                return null; // Is it possible?

            try
            {
                using(MemoryStream memoryStream = new MemoryStream())
                {
                    // You need to specify the image format to fill the stream.
                    // I'm assuming it is PNG
                    bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // Make sure to create the bitmap in the UI thread
                    if(InvokeRequired)
                        return (BitmapSource)Application.Current.Dispatcher.Invoke(
                            new Func<Stream, BitmapSource>(CreateBitmapSourceFromBitmap),
                            DispatcherPriority.Normal,
                            memoryStream
                            );

                    return CreateBitmapSourceFromBitmap(memoryStream);
                }
            }
            catch(Exception)
            {
                return null;
            }
        }

        private static BitmapSource CreateBitmapSourceFromBitmap(Stream stream)
        {
            BitmapDecoder bitmapDecoder = BitmapDecoder.Create(
                stream,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad
                );

            // This will disconnect the stream from the image completely...
            WriteableBitmap writable = new WriteableBitmap(bitmapDecoder.Frames.Single());
            writable.Freeze();

            return writable;
        }

        private static bool InvokeRequired
        {
            get { return Dispatcher.CurrentDispatcher != Application.Current.Dispatcher; }
        }

        public class ShellThumbnail : IDisposable
        {
            public static Icon GetSmallFileIcon(FileInfo file)
            {
                if(file.Exists)
                {
                    WinAPI.SHFILEINFO shFileInfo = new WinAPI.SHFILEINFO();
                    WinAPI.SHGetFileInfo(file.FullName, 0, out shFileInfo, (uint)Marshal.SizeOf(shFileInfo), WinAPI.SHGFI_ICON | WinAPI.SHGFI_SMALLICON);

                    return Icon.FromHandle(shFileInfo.hIcon);
                }
                else
                    return SystemIcons.WinLogo;
            }

            public static Icon GetSmallFileIcon(string fileName)
            {
                return GetSmallFileIcon(new FileInfo(fileName));
            }

            public static Icon GetLargeFileIcon(FileInfo file)
            {
                if(file.Exists)
                {
                    WinAPI.SHFILEINFO shFileInfo = new WinAPI.SHFILEINFO();
                    WinAPI.SHGetFileInfo(file.FullName, 0, out shFileInfo, (uint)Marshal.SizeOf(shFileInfo), WinAPI.SHGFI_ICON | WinAPI.SHGFI_LARGEICON);

                    return Icon.FromHandle(shFileInfo.hIcon);
                }
                else
                    return SystemIcons.WinLogo;
            }

            public static Icon GetLargeFileIcon(string fileName)
            {
                return GetLargeFileIcon(new FileInfo(fileName));
            }

            ~ShellThumbnail()
            {
                Dispose();
            }

            private WinAPI.IMalloc alloc = null;
            private bool disposed = false;
            private System.Drawing.Size _desiredSize = new System.Drawing.Size(100, 100);
            private Bitmap _thumbNail;

            public Bitmap ThumbNail
            {
                get
                {
                    return _thumbNail;
                }
            }

            public System.Drawing.Size DesiredSize
            {
                get { return _desiredSize; }
                set { _desiredSize = value; }
            }

            private WinAPI.IMalloc Allocator
            {
                get
                {
                    if(!disposed)
                    {
                        if(alloc == null)
                        {
                            WinAPI.SHGetMalloc(ref alloc);
                        }
                    }
                    else
                    {
                        Debug.Assert(false, "Object has been disposed.");
                    }
                    return alloc;
                }
            }

            public Bitmap GetThumbnail(string fileName)
            {
                if(!File.Exists(fileName) && !Directory.Exists(fileName))
                {
                    throw new FileNotFoundException(string.Format("The file '{0}' does not exist", fileName), fileName);
                }
                if(_thumbNail != null)
                {
                    _thumbNail.Dispose();
                    _thumbNail = null;
                }
                WinAPI.IShellFolder folder = null;
                try
                {
                    folder = getDesktopFolder;
                }
                catch(Exception ex)
                {
                    throw ex;
                }
                if(folder != null)
                {
                    IntPtr pidlMain = IntPtr.Zero;
                    try
                    {
                        int cParsed = 0;
                        int pdwAttrib = 0;
                        string filePath = Path.GetDirectoryName(fileName);
                        folder.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, filePath, ref cParsed, ref pidlMain, ref pdwAttrib);
                    }
                    catch(Exception ex)
                    {
                        Marshal.ReleaseComObject(folder);
                        throw ex;
                    }
                    if(pidlMain != IntPtr.Zero)
                    {
                        Guid iidShellFolder = new Guid("000214E6-0000-0000-C000-000000000046");
                        WinAPI.IShellFolder item = null;
                        try
                        {
                            folder.BindToObject(pidlMain, IntPtr.Zero, ref iidShellFolder, ref item);
                        }
                        catch(Exception ex)
                        {
                            Marshal.ReleaseComObject(folder);
                            Allocator.Free(pidlMain);
                            throw ex;
                        }
                        if(item != null)
                        {
                            WinAPI.IEnumIDList idEnum = null;
                            try
                            {
                                item.EnumObjects(IntPtr.Zero, (WinAPI.ESHCONTF.SHCONTF_FOLDERS | WinAPI.ESHCONTF.SHCONTF_NONFOLDERS), ref idEnum);
                            }
                            catch(Exception ex)
                            {
                                Marshal.ReleaseComObject(folder);
                                Allocator.Free(pidlMain);
                                throw ex;
                            }
                            if(idEnum != null)
                            {
                                int hRes = 0;
                                IntPtr pidl = IntPtr.Zero;
                                int fetched = 0;
                                bool complete = false;
                                while(!complete)
                                {
                                    hRes = idEnum.Next(1, ref pidl, ref fetched);
                                    if(hRes != 0)
                                    {
                                        pidl = IntPtr.Zero;
                                        complete = true;
                                    }
                                    else
                                    {
                                        if(_getThumbNail(fileName, pidl, item))
                                        {
                                            complete = true;
                                        }
                                    }
                                    if(pidl != IntPtr.Zero)
                                    {
                                        Allocator.Free(pidl);
                                    }
                                }
                                Marshal.ReleaseComObject(idEnum);
                            }
                            Marshal.ReleaseComObject(item);
                        }
                        Allocator.Free(pidlMain);
                    }
                    Marshal.ReleaseComObject(folder);
                }
                return ThumbNail;
            }

            private bool _getThumbNail(string file, IntPtr pidl, WinAPI.IShellFolder item)
            {
                IntPtr hBmp = IntPtr.Zero;
                WinAPI.IExtractImage extractImage = null;
                try
                {
                    string pidlPath = PathFromPidl(pidl);
                    if(Path.GetFileName(pidlPath).ToUpper().Equals(Path.GetFileName(file).ToUpper()))
                    {
                        WinAPI.IUnknown iunk = null;
                        int prgf = 0;
                        Guid iidExtractImage = new Guid("BB2E617C-0920-11d1-9A0B-00C04FC2D6C1");
                        item.GetUIObjectOf(IntPtr.Zero, 1, ref pidl, ref iidExtractImage, ref prgf, ref iunk);
                        extractImage = (WinAPI.IExtractImage)iunk;
                        if(extractImage != null)
                        {
                            Console.WriteLine("Got an IExtractImage object!");
                            WinAPI.SIZE sz = new WinAPI.SIZE();
                            sz.cx = DesiredSize.Width;
                            sz.cy = DesiredSize.Height;
                            StringBuilder location = new StringBuilder(260, 260);
                            int priority = 0;
                            int requestedColourDepth = 32;
                            WinAPI.EIEIFLAG flags = WinAPI.EIEIFLAG.IEIFLAG_ASPECT | WinAPI.EIEIFLAG.IEIFLAG_SCREEN;
                            int uFlags = (int)flags;
                            extractImage.GetLocation(location, location.Capacity, ref priority, ref sz, requestedColourDepth, ref uFlags);
                            extractImage.Extract(ref hBmp);
                            if(hBmp != IntPtr.Zero)
                            {
                                _thumbNail = Bitmap.FromHbitmap(hBmp);
                            }
                            Marshal.ReleaseComObject(extractImage);
                            extractImage = null;
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch(Exception ex)
                {
                    if(hBmp != IntPtr.Zero)
                    {
                        WinAPI.DeleteObject(hBmp);
                    }
                    if(extractImage != null)
                    {
                        Marshal.ReleaseComObject(extractImage);
                    }
                    throw ex;
                }
            }

            private string PathFromPidl(IntPtr pidl)
            {
                StringBuilder path = new StringBuilder(260, 260);
                int result =  WinAPI.SHGetPathFromIDList(pidl, path);
                if(result == 0)
                {
                    return string.Empty;
                }
                else
                {
                    return path.ToString();
                }
            }

            private WinAPI.IShellFolder getDesktopFolder
            {
                get
                {
                    WinAPI.IShellFolder ppshf = null;
                    int r =  WinAPI.SHGetDesktopFolder(ref ppshf);
                    return ppshf;
                }
            }

            public void Dispose()
            {
                if(!disposed)
                {
                    if(alloc != null)
                    {
                        Marshal.ReleaseComObject(alloc);
                    }
                    alloc = null;
                    if(_thumbNail != null)
                    {
                        _thumbNail.Dispose();
                    }
                    disposed = true;
                }
            }

            public static Icon ExtractIconFromExe(string file, bool large, int nIconIndex = 0)
            {
                unsafe
                {
                    int readIconCount = 0;
                    IntPtr[] hDummy = new IntPtr[1] { IntPtr.Zero };
                    IntPtr[] hIconEx = new IntPtr[1] { IntPtr.Zero };

                    try
                    {
                        if(large)
                            readIconCount = WinAPI.ExtractIconEx(file, nIconIndex, hIconEx, hDummy, 1);
                        else
                            readIconCount = WinAPI.ExtractIconEx(file, nIconIndex, hDummy, hIconEx, 1);

                        if(readIconCount > 0 && hIconEx[0] != IntPtr.Zero)
                        {
                            // GET FIRST EXTRACTED ICON
                            Icon extractedIcon = (Icon)Icon.FromHandle(hIconEx[0]).Clone();

                            return extractedIcon;
                        }
                        else // NO ICONS READ
                            return null;
                    }
                    catch(Exception ex)
                    {
                        /* EXTRACT ICON ERROR */

                        // BUBBLE UP
                        throw new Exception("Could not extract icon", ex);
                    }
                    finally
                    {
                        // RELEASE RESOURCES
                        foreach(IntPtr ptr in hIconEx)
                            if(ptr != IntPtr.Zero)
                                WinAPI.DestroyIcon(ptr);

                        foreach(IntPtr ptr in hDummy)
                            if(ptr != IntPtr.Zero)
                                WinAPI.DestroyIcon(ptr);
                    }
                }
            }
        }
    }
}