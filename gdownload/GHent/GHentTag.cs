using System;
using System.IO;
using HtmlAgilityPack;

namespace GDownload.GHent
{
    class GHentTag
    {
        readonly string _savePath;
        readonly string _url;
        readonly bool _checkExceeded;
        readonly HtmlWeb _web = new HtmlWeb();
        readonly CbrManager _cBrManager = new CbrManager();
        readonly string _cbrLocation;

        public bool IsExceeded { get; private set; }

        public bool IgnoreOngoing { get; set; } = false;

        public GHentTag(string savePath, string url, string cbrLocation = "", bool checkExceed = true)
        {
            _savePath = savePath;
            _url = url;
            _checkExceeded = checkExceed;
            _cbrLocation = cbrLocation;
        }

        public void Parse()
        {
            Console.WriteLine("Searching by tag");
            Console.WriteLine("[IgnoreOngoing]: " + IgnoreOngoing);
            Console.WriteLine("[checkExceeded]: " + _checkExceeded);
            var page = _web.Load(_url).DocumentNode;
            var trs = page.SelectNodes("//table[contains(@class,'itg')]//tr");

            foreach (var tr in trs)
            {
                if (tr.ChildNodes[0].Name == "th")
                {
                    continue;
                }

                string link;

                try
                {
                    link = tr.ChildNodes[2].ChildNodes[0].Attributes["href"].Value;
                }
                catch (Exception) { continue; }

                GHentSite gHentSite = new GHentSite(_savePath, link, _checkExceeded);
                gHentSite.IgnoreOngoing = IgnoreOngoing;

                gHentSite.Parse();

                if (IgnoreOngoing && gHentSite.OngoingFound == false)
                {
                    if (_cbrLocation != "" && gHentSite.Name != null && (gHentSite.Changed || File.Exists(_cbrLocation + "\\" + gHentSite.Name + ".cbr") == false))
                    {
                        Console.WriteLine("Generating new CBR");
                        if (_cBrManager.CreateCbr(_savePath + "\\" + gHentSite.Name, _cbrLocation + "\\" + gHentSite.Name + ".cbr"))
                        {
                            Console.WriteLine("OK");
                        }
                        else
                        {
                            Console.WriteLine("Error");
                        }
                    }
                }

                if (gHentSite.IsExceeded)
                {
                    IsExceeded = true;
                    break;
                }
            }

            if (IsExceeded)
            {
                Console.WriteLine("Exceeded");
            }
            else
            {
                Console.WriteLine("Tag downloaded OK");
            }
        }
    }
}
