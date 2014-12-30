using System;
using System.IO;
using System.Windows;
using System.Xml;
using Ionic.Zip;
using Newgen.Base;

namespace WidgetPackageEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string[] args = e.Args;

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Title = "WPE - Part of Newgen WDK";
            Console.Write("WPE - Part of Newgen WDK");
            Console.Write("\n-----------------------------------------------------\n");

            Console.Write("\n");
            Console.Write("Commands :       -P <Widget Folder Path>");
            Console.Write("\n");
            Console.Write("                 -O <Output Path>");
            Console.Write("\n");
            Console.Write("                 -W <Widget Title>");
            Console.Write("\n");
            Console.Write("                 -I <ID>");
            Console.Write("\n");
            Console.Write("                 -V <Version>");
            Console.Write("\n");
            Console.Write("                 -D <Description>");
            Console.Write("\n");
            Console.Write("                 -A <Author>");
            Console.Write("\n");
            Console.Write("                 -AW <Author Website>");
            Console.Write("\n");

            WidgetInfo info = new WidgetInfo();

            string dirpath = "";
            string outpath = "";

            if(!(args.Length == 0))
            {
                Console.Write("\n");
                Console.Write("Parsing commands ...");
                Console.Write("\n");
                iFr.CommandLineArgumentsParser CommandLine = new iFr.CommandLineArgumentsParser(args);
                if(CommandLine["P"] != null) { Console.WriteLine(dirpath = CommandLine["P"]); }
                else { Console.WriteLine("Command P has some errors."); return; }

                if(CommandLine["O"] != null)
                    Console.WriteLine(outpath = CommandLine["O"]);
                else { Console.WriteLine("Command O has some errors."); return; }

                if(CommandLine["W"] != null)
                    Console.WriteLine(info.Name = CommandLine["W"]);
                else { Console.WriteLine("Command W has some errors."); return; }

                if(CommandLine["I"] != null)
                    Console.WriteLine(info.ID = CommandLine["I"]);
                else { Console.WriteLine("Command I has some errors."); return; }

                if(CommandLine["V"] != null)
                    Console.WriteLine(info.Version = CommandLine["V"]);
                else { Console.WriteLine("Command V has some errors."); return; }

                if(CommandLine["D"] != null)
                    Console.WriteLine(info.Description = CommandLine["D"]);
                else { Console.WriteLine("Command D has some errors."); return; }

                if(CommandLine["A"] != null)
                    Console.WriteLine(info.Author = CommandLine["A"]);
                else { Console.WriteLine("Command A has some errors."); return; }

                if(CommandLine["AW"] != null)
                    Console.WriteLine(info.AuthorWeb = CommandLine["AW"]);
                else { Console.WriteLine("Command AW has some errors."); return; }

                Console.Write("\n");
                Console.Write("Packing widget ...");
                Console.Write("\n");
            }
            else
            {
                Console.Write("\n");
                Console.Write("Widget folder : ");
                dirpath = Convert.ToString(Console.ReadLine());
                Console.Write("Output folder : ");
                outpath = Convert.ToString(Console.ReadLine());
                Console.Write("Widget name : ");
                info.Name = Convert.ToString(Console.ReadLine());
                Console.Write("Widget ID : ");
                info.ID = Convert.ToString(Console.ReadLine());
                Console.Write("Widget version (*.*.*.*) : ");
                info.Version = Convert.ToString(Console.ReadLine());
                Console.Write("Widget description : ");
                info.Description = Convert.ToString(Console.ReadLine());
                Console.Write("Widget author name : ");
                info.Author = Convert.ToString(Console.ReadLine());
                Console.Write("Widget author website url : ");
                info.AuthorWeb = Convert.ToString(Console.ReadLine());
                Console.Write("\n");
            }

            try
            {
                info.Save(dirpath + "\\Widget.xml");
                using(ZipFile zip = new ZipFile())
                {
                    string[] files = Directory.GetFiles(dirpath);

                    zip.AddFiles(files, info.Name);

                    zip.Comment =
                         info.Name + " Widget\n\n" +
                        "Packed by WPE - Part of Newgen WDK" +
                        "\n-----------------------------------------------------\n\n\n" +
                        "Widget         : " + info.Name + "\n" +
                        "Timestamp      : " + System.DateTime.Now.ToString("G") + "\n" +
                        "ID             : " + info.ID + "\n" +
                        "Version        : " + info.Version + "\n" +
                        "Description    : " + info.Description + "\n" +
                        "Author         : " + info.Author + "\n" +
                        "Website        : " + info.AuthorWeb + "\n\n" +
                        "-----------------------------------------------------";

                    zip.Save(outpath + "\\" + info.Name + ".nwp");
                }
                if(File.Exists(outpath + "\\meta.xml"))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(outpath + "\\meta.xml");
                    try
                    {
                        if(doc.DocumentElement.SelectNodes("//Widget[@Id='"+info.ID+"']").Count == 0)
                            throw new Exception("!!!");

                        Console.Write("Existing metadata found ...");
                    }
                    catch
                    {
                        Console.Write("Updating metadata ...");

                        XmlNode node = doc.CreateNode(XmlNodeType.Element, "Widget", null);

                        node.Attributes.Append(doc.CreateAttribute("Id"));
                        node.Attributes.Append(doc.CreateAttribute("Name"));
                        node.Attributes.Append(doc.CreateAttribute("Version"));
                        node.Attributes.Append(doc.CreateAttribute("Author"));
                        node.Attributes.Append(doc.CreateAttribute("Description"));
                        node.Attributes.Append(doc.CreateAttribute("AuthorWeb"));

                        node.Attributes["Id"].Value = info.ID;
                        node.Attributes["Name"].Value = info.Name;
                        node.Attributes["Version"].Value = info.Version;
                        node.Attributes["Author"].Value = info.Author;
                        node.Attributes["Description"].Value = info.Description;
                        node.Attributes["AuthorWeb"].Value = info.AuthorWeb;

                        doc.DocumentElement.AppendChild(node);

                        doc.Save(outpath + "\\meta.xml");
                    }
                }
                Console.Write("\n\n-----------------------------------------------------\n\n\n");
                Console.Write("Widget packed. Now you can distribute it. :)");
                App.Current.Shutdown(0);
            }
            catch(Exception ex)
            {
                Console.Write("\n\n-----------------------------------------------------\n\n\n");
                Console.Write("Failed to pack widget. :(");
                Console.Write("\n");
                Console.Write(ex.Message);
                App.Current.Shutdown(0);
            }
        }
    }
}