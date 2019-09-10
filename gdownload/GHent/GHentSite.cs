using System;
using System.IO;
using System.Net;
using HtmlAgilityPack;

namespace GDownload.GHent
{
    internal class GHentSite
    {
        private readonly FileManager _fileManager;

        private readonly bool _parsed = false;

        private readonly string _savePath;
        private readonly string _url;
        private readonly HtmlWeb _web = new HtmlWeb();
        private int _downloaded;

        private int _imgnr;

        public GHentSite(string savePath, string url, bool checkExceed = true)
        {
            _savePath = savePath;
            _url = url;
            CheckExceed = checkExceed;

            if (checkExceed)
            {
                _fileManager = new FileManager();
            }
        }

        public bool Changed { get; private set; }

        public bool CheckExceed { get; set; }
        public bool IgnoreOngoing { get; set; } = false;
        public bool IsExceeded { get; private set; }
        public string Name { get; private set; }

        public bool OngoingFound { get; private set; }

        public bool Stop { get; set; } = false;

        public void Parse(bool downloadImages = true, bool ignoreDatabase = false)
        {
            if (_parsed)
            {
                return;
            }

            int page = 0;
            int pages = 0;

            HtmlDocument doc;

            do
            {
                Changed = false;

                //main = web.Load(url + "?p=" + page)?.GetElementbyId("gdt");
                doc = _web.Load(_url + "?p=" + page);

                if (doc == null)
                {
                    return;
                }

                if (page == 0)
                {
                    Name = doc.GetElementbyId("gn").InnerHtml;
                    Name = Name.Replace(':', '_');
                    Name = Name.Replace('/', '_');
                    Name = Name.Replace('\\', '_');
                    Name = Name.Replace('*', '_');
                    Name = Name.Replace('?', '_');
                    Name = Name.Replace('"', '_');
                    Name = Name.Replace('<', '_');
                    Name = Name.Replace('>', '_');
                    Name = Name.Replace('|', '_');

                    if (IgnoreOngoing)
                    {
                        if (Name.ToUpper().Contains("ONGOING"))
                        {
                            Console.WriteLine("Ongoing detected, ignoring");
                            OngoingFound = true;
                            return;
                        }
                    }

                    pages = doc.DocumentNode.ChildNodes[2].ChildNodes[3].ChildNodes[12].ChildNodes[1].ChildNodes[0]
                                .ChildNodes.Count - 2;

                    Console.WriteLine("Downloading " + Name + ", " + pages + " pages");
                }
                else
                {
                    Console.WriteLine("Page " + (page + 1) + " of " + pages);
                }

                if (!downloadImages)
                {
                    break;
                }

                foreach (HtmlNode el in doc.GetElementbyId("gdt").ChildNodes)
                {
                    if (el.Attributes["class"].Value != "gdtm")
                    {
                        continue;
                    }

                    HtmlNode link = el.SelectSingleNode(el.XPath + "//div//a");

                    ParseImg(link.Attributes["href"].Value);

                    if (IsExceeded)
                    {
                        break;
                    }
                }

                page++;
            } while (page < pages && !IsExceeded);

            Console.WriteLine("Downloaded " + _downloaded + " images, " + (_imgnr - _downloaded) + " ignored." +
                              (IsExceeded ? " EXCEEDED!" : ""));
        }

        private void ParseImg(string url)
        {
            if (Directory.Exists(_savePath + "\\" + Name) == false)
            {
                Directory.CreateDirectory(_savePath + "\\" + Name);
            }
            else
            {
                if (File.Exists(_savePath + "\\" + Name + "\\" + _imgnr + ".jpg"))
                {
                    _imgnr++;
                    return;
                }
            }

            HtmlDocument imgLoad1 = _web.Load(url);

            HtmlNode nl = imgLoad1.GetElementbyId("loadfail");

            string nlurl = nl.Attributes["onclick"].Value;
            nlurl = nlurl.Remove(0, nlurl.IndexOf('\'') + 1);
            nlurl = nlurl.Remove(nlurl.IndexOf('\''));

            HtmlDocument imgLoad = _web.Load(url + "?nl=" + nlurl);

            HtmlNode a = imgLoad.GetElementbyId("i3");

            if (a == null)
            {
                return;
            }

            HtmlNode img = a.SelectSingleNode(a.XPath + "//a//img");

            string imgLink = img.Attributes["src"].Value;

            Uri test = new Uri(imgLink);

            Changed = true;

            string test2 = _savePath + "\\" + Name + "\\" + _imgnr++ + ".jpg";

            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.DownloadFile(test, test2);

                    if (CheckExceed)
                    {
                        if (_fileManager.IsExceeded(test2))
                        {
                            File.Delete(test2);
                            IsExceeded = true;
                            return;
                        }
                    }

                    _downloaded++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error downloading " + Name + " (" + _imgnr + "): " + ex.Message);
                }
            }
        }
    }
}