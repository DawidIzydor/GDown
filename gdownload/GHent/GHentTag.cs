using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using HtmlAgilityPack;

namespace gdownload.GHent
{
    class GHentTag
    {
        string savePath;
        string url;
        bool checkExceeded;
        HtmlWeb web = new HtmlWeb();
        CBRManager cBRManager = new CBRManager();
        string cbrLocation;

        public bool IsExceeded { get; private set; } = false;

        public bool IgnoreOngoing { get; set; } = false;

        public GHentTag(string SavePath, string Url, string cbrLocation = "", bool CheckExceed = true)
        {
            savePath = SavePath;
            url = Url;
            checkExceeded = CheckExceed;
            this.cbrLocation = cbrLocation;
        }

        public void Parse()
        {
            Console.WriteLine("Searching by tag");
            Console.WriteLine("[IgnoreOngoing]: " + IgnoreOngoing);
            Console.WriteLine("[checkExceeded]: " + checkExceeded);
            var page = web.Load(url).DocumentNode;
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
                    link = tr.ChildNodes[2].ChildNodes[0].ChildNodes[2].ChildNodes[0].Attributes["href"].Value;
                }
                catch (Exception) { continue; }

                GHentSite gHentSite = new GHentSite(savePath, link, checkExceeded);
                gHentSite.IgnoreOngoing = this.IgnoreOngoing;

                gHentSite.Parse();

                if (this.IgnoreOngoing && gHentSite.OngoingFound == false)
                {
                    if (cbrLocation != "" && gHentSite.Name != null && (gHentSite.Changed || File.Exists(cbrLocation + "\\" + gHentSite.Name + ".cbr") == false))
                    {
                        Console.WriteLine("Generating new CBR");
                        if (cBRManager.CreateCbr(savePath + "\\" + gHentSite.Name, cbrLocation + "\\" + gHentSite.Name + ".cbr"))
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
