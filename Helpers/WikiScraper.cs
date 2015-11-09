using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;
using MovieMaster.Objects;

namespace MovieMaster.Helpers
{
    public class WikipediaInfo
    {
        public string ImageFile;
        public List<MovieMetaData> MetaData;
    }

    public static class WikiScraper
    {
        static WikiScraper()
        {
            
        }

        public static string BasePath
        {
            get;
            set;
        }

        public static WikipediaInfo GetWikiData(string movieName, int year)
        {
            string testURL = string.Format("http://en.wikipedia.org/wiki/{0}_(film)", movieName.Replace(" ", "_"));
            bool failed = true;
            System.Net.WebClient wc = new WebClient();
            string _UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_2) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1309.0 Safari/537.17";
            wc.Headers.Add(HttpRequestHeader.UserAgent, _UserAgent);

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    
                    string content = wc.DownloadString(testURL);

                    var result = WikiScraper.MineContent(movieName, content);
                    return result;
                }
                catch (Exception ex)
                {
                    failed = true;
                    //didn't work
                }

                if (failed && i == 0)
                {
                    testURL = string.Format("http://en.wikipedia.org/wiki/{0}", movieName.Replace(" ", "_"));
                }

                if (failed && i == 1)
                {
                    testURL = string.Format("http://en.wikipedia.org/wiki/{0}_({1}_film)", movieName.Replace(" ", "_"), year);
                }
            }
            return null;
        }

        private static WikipediaInfo MineContent(string movieName, string content)
        {
            WikipediaInfo info = new WikipediaInfo();
            var doc = new XmlDocument();
            doc.LoadXml(content);

            XmlNodeList tables = doc.GetElementsByTagName("table");

            XmlNode infoTable = null;
            bool tableFound = false;
            for (int i = 0; i < tables.Count; i++)
            {
                if (tables[i].Attributes == null)
                    continue;

                foreach (var att in tables[i].Attributes)
                {
                    if (((System.Xml.XmlAttribute)att).Value == "infobox vevent" || ((System.Xml.XmlAttribute)att).Value == "infobox")
                    {
                        //this is the table....
                        infoTable = tables[i];
                        tableFound = true;
                        break;
                    }
                }
                if (tableFound)
                    break;
            }

            if (!tableFound)
                return null;

            //first row title
            string movieTitle = infoTable.ChildNodes[0].ChildNodes[0].InnerText;

            //second row image
            string imageURL = infoTable.ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0].Attributes["src"].Value.Replace("//", "http://");

            //TODO: uncomment to get the image....
            
            string fileName = imageURL.Substring(imageURL.LastIndexOf("/")+1);
            string imglocation = System.IO.Path.Combine(BasePath, fileName);

            if (!System.IO.File.Exists(imglocation))
            {
                var wc = new WebClient();
                wc.DownloadFile(imageURL, imglocation);
            }

            var metaData = new List<Tuple<string, string>>();

            //3rd Row begin metadata...
            for (int i = 2; i < infoTable.ChildNodes.Count; i++)
            {
                string name, value = string.Empty;
                name = infoTable.ChildNodes[i].ChildNodes[0].InnerText;
                if (infoTable.ChildNodes[i].ChildNodes[1].ChildNodes.Count >= 1)
                {
                    for (int j = 0; j < infoTable.ChildNodes[i].ChildNodes[1].ChildNodes.Count; j++)
                    {
                        if (j == 0)
                            value = infoTable.ChildNodes[i].ChildNodes[1].ChildNodes[j].InnerText;
                        else
                        {
                            if (!string.IsNullOrEmpty(infoTable.ChildNodes[i].ChildNodes[1].ChildNodes[j].InnerText.Trim()))
                                value += ", " + infoTable.ChildNodes[i].ChildNodes[1].ChildNodes[j].InnerText.Trim(new char[] { '\n' });
                        }
                    }
                }
                else
                    value = infoTable.ChildNodes[i].ChildNodes[1].InnerText;

                metaData.Add(new Tuple<string, string>(name, value));
            }

            info.ImageFile = imglocation;
            info.MetaData = metaData.Select(a => new MovieMetaData() { Value = string.Format("{0}|{1}", a.Item1, a.Item2) }).ToList();
            return info;
        }
    }
}
