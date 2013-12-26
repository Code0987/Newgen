using System;
using System.Linq;
using System.Reflection;
using Newgen.Base;

namespace Newgen.Core
{
    public class WidgetProxy
    {
        public readonly string Path;
        private Assembly assembly;

        public NewgenWidget WidgetComponent { get; private set; }

        public bool IsLoaded { get; private set; }

        public string Name { get; private set; }

        public bool HasErrors { get; private set; }

        public int Column { get; set; }

        public int Row { get; set; }

        public string ObjectData { get; set; }

        public TileCellType TileCellType { get; set; }

        public WidgetProxy(string path, string name = null, bool isHtml = false, bool isGenerated = false, bool isSocial = false, bool isAd = false)
        {
            Path = path;
            Column = -1;
            Row = -1;

            if (isHtml)
            {
                InitializeHtml();
                return;
            }

            if (isGenerated)
            {
                InitializeGenerated();
                return;
            }

            if(isSocial)
            {
                Name = name;
                InitializeSocial();
                return;
            }

            if(isAd)
            {
                InitializeAd();
                return;
            }

            try
            {
                Initialize();
            }
            catch { HasErrors = true; return; }
        }

        private void Initialize()
        {
            Type widgetType = null;
            try
            {
                assembly = Assembly.LoadFrom(Path);
                widgetType = assembly.GetTypes().FirstOrDefault(type => typeof(NewgenWidget).IsAssignableFrom(type));
            }
            catch (ReflectionTypeLoadException ex)
            {
                ex = null;
                //throw new Exception("Failed to load provider from " + Path + ".\n" + ex);
                HasErrors = true;
                return;
            }

            if (widgetType == null)
            {
                //throw new Exception("Failed to find IWeatherProvider in " + Path);
                HasErrors = true;
                return;
            }

            WidgetComponent = Activator.CreateInstance(widgetType) as NewgenWidget;
            if (WidgetComponent == null)
            {
                HasErrors = true;
                return;
            }

            Name = WidgetComponent.Name;
            TileCellType = TileCellType.Native;
        }

        private void InitializeHtml()
        {
            WidgetComponent = new NewgenHtmlWidget(Path);
            Name = WidgetComponent.Name;
            TileCellType = TileCellType.Html;
        }

        private void InitializeGenerated()
        {
            if (Path.StartsWith("http://")) WidgetComponent = new NewgenWebPreviewWidget();
            else WidgetComponent = new NewgenAppWidget();
            TileCellType = TileCellType.Generated;
            Name = WidgetComponent.Name;
        }

        private void InitializeSocial()
        {
            WidgetComponent = new NewgenFriendWidget();
            TileCellType = TileCellType.Generated;
        }

        private void InitializeAd()
        {
            WidgetComponent = new NewgenAdWidget();
            TileCellType = TileCellType.Generated;
        }

        public void Load()
        {
            if (TileCellType == TileCellType.Generated)
            {
                if (string.IsNullOrEmpty(Name)) WidgetComponent.Load(Path);
                else WidgetComponent.Load(Path, Name, Environment.TickCount * Row * Column);
            }
            else WidgetComponent.Load();

            IsLoaded = true;
        }

        public void Unload()
        {
            try
            {
                WidgetComponent.Unload();
            }
            catch { }
            IsLoaded = false;
        }
    }
}