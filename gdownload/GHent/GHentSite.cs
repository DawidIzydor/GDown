using HtmlAgilityPack;
using System;
using System.IO;
using System.Net;

namespace GDownload.GHent
{
    class GHentSite
    {
        readonly HtmlWeb _web = new HtmlWeb();

        readonly string _savePath;
        public string Name { get; private set; }
        readonly string _url;

        int _imgnr = 0;
        int _downloaded = 0;

        readonly bool _parsed = false;
        public bool IsExceeded { get; private set; } = false;
        public bool Changed { get; private set; } = false;

        public bool CheckExceed { get; set; } = false;
        public bool IgnoreOngoing { get; set; } = false;

        public bool OngoingFound { get; private set; } = false;

        public bool Stop { get; set; } = false;

        readonly FileManager _fileManager;

        public GHentSite(string savePath, string url, bool checkExceed = true)
        {
            this._savePath = savePath;
            this._url = url;
            this.CheckExceed = checkExceed;

            if (checkExceed)
            {
                _fileManager = new FileManager();
            }
        }

        public void Parse(bool downloadImages = true, bool ignoreDatabase = false)
        {
            if (_parsed) return;

            int page = 0;
            int pages = 0;

            HtmlDocument doc;

            do
            {
                Changed = false;

                //main = web.Load(url + "?p=" + page)?.GetElementbyId("gdt");
                doc = _web.Load(_url + "?p=" + page);

                if (doc == null) return;

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
                            this.OngoingFound = true;
                            return;
                        }
                    }


                    pages = doc.DocumentNode.ChildNodes[2].ChildNodes[3].ChildNodes[12].ChildNodes[1].ChildNodes[0].ChildNodes.Count - 2;

                    Console.WriteLine("Downloading " + Name + ", " + pages + " pages");
                }
                else
                {
                    Console.WriteLine("Page " + (page+1) + " of " + pages);
                }

                if (!downloadImages) break;

                foreach (var el in doc.GetElementbyId("gdt").ChildNodes)
                {
                    if (el.Attributes["class"].Value != "gdtm") continue;

                    var link = el.SelectSingleNode(el.XPath + "//div//a");

                    ParseImg(link.Attributes["href"].Value);

                    if (IsExceeded) break;
                }

                page++;
            } while (page < pages && !IsExceeded);

            Console.WriteLine("Downloaded " + _downloaded + " images, " + (_imgnr - _downloaded) + " ignored." + (IsExceeded ? " EXCEEDED!" : ""));
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


            var imgLoad1 = _web.Load(url);

            var nl = imgLoad1.GetElementbyId("loadfail");

            var nlurl = nl.Attributes["onclick"].Value;
            nlurl = nlurl.Remove(0, nlurl.IndexOf('\'') + 1);
            nlurl = nlurl.Remove(nlurl.IndexOf('\''));

            var imgLoad = _web.Load(url + "?nl=" + nlurl);

            var a = imgLoad.GetElementbyId("i3");

            if (a == null) return;

            var img = a.SelectSingleNode(a.XPath + "//a//img");

            var imgLink = img.Attributes["src"].Value;

            var test = new Uri(imgLink);

            Changed = true;

            var test2 = _savePath + "\\" + Name + "\\" + _imgnr++ + ".jpg";

            using (var wc = new WebClient())
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
