using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Configuration;
using System.Xml;

namespace MovieMaster.Helpers
{
    public class SettingsManager
    {
        static bool _loading = false;
        static string _mediaFolder;
        static string _imageFolder;
        static string _theme;

        static SettingsManager()
        {
            ToggleLoading();
            NameValueCollection configSectionSettings = (NameValueCollection)ConfigurationManager.GetSection("UserSettings");
            if (String.IsNullOrEmpty(configSectionSettings["MediaFolder"]))
                SettingsManager.MediaFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", string.Empty));//configSectionSettings["MediaFolder"];
            else
                SettingsManager.MediaFolder = configSectionSettings["MediaFolder"];

            SettingsManager.ImagesFolder = System.IO.Path.Combine(SettingsManager.MediaFolder, "Images");
            SettingsManager.Theme = configSectionSettings["Theme"];
            if (Theme == null)
                Theme = "Default";
            ToggleLoading();
        }

        private static void ToggleLoading()
        {
            _loading = !_loading;
        }

        public static string MediaFolder
        {
            get
            {
                return _mediaFolder;
            }
            set
            {
                if (!_loading)
                    WriteBackToAppConfig("MediaFolder", value);
                _mediaFolder = value;
                _imageFolder = System.IO.Path.Combine(_mediaFolder, "Images");
            }
        }

        public static string ImagesFolder
        {
            get
            {
                return _imageFolder;
            }
            set
            {
                if (!_loading)
                    WriteBackToAppConfig("ImageFolder", value);
                _imageFolder = value;                
            }
        }

        public static string Theme
        {
            get
            {
                return _theme;
            }
            set
            {
                if (!_loading)
                    WriteBackToAppConfig("Theme", value);
                _theme = value;  
            }
        }

        private static void WriteBackToAppConfig(string key, string value)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            string filename = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            filename = filename.Replace("file:///", string.Empty);
            filename = filename + ".config";
            doc.Load(filename);

            var elements = doc.GetElementsByTagName("UserSettings");
            if (elements != null && elements.Count > 0)
            {
                XmlNode node = elements.Item(0);
                for (int i = 0; i < node.ChildNodes.Count; i++)
                {
                    if (node.ChildNodes[i].Attributes["key"].Value == key)
                    {
                        node.ChildNodes[i].Attributes["value"].Value = value;
                        break;
                    }
                }
                doc.Save(filename);
            }
        }
    }
}
