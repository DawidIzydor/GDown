using gdownload.sql;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace gdownload.GHent
{
    class GHentSite
    {
        HtmlWeb web = new HtmlWeb();

        readonly string SavePath;
        public string Name { get; private set; }
        readonly string url;

        int imgnr = 0;
        int downloaded = 0;

        bool parsed = false;
        public bool IsExceeded { get; private set; } = false;
        public bool Changed { get; private set; } = false;

        public bool CheckExceed { get; set;} = false;
        public bool IgnoreOngoing {get; set;} = false;

        FileManager fileManager;

        public GHentSite(string SavePath, string url, bool CheckExceed = true)
        {
            this.SavePath = SavePath;
            this.url = url;
            this.CheckExceed = CheckExceed;

            if(CheckExceed)
            {
                fileManager = new FileManager();
            }
        }

        public void Parse(bool downloadImages = true, bool ignoreDatabase = false)
        {
            if (parsed) return;

            int page = 0;
            int pages = 0;
            
            HtmlNode main;

            do
            {
                Changed = false;

                main = web.Load(url+"?p="+page)?.GetElementbyId("gdt");

                if (main == null) return;

                if(page == 0)
                {
                    Name = main.SelectSingleNode("//body[1]//div[1]//div[2]//h1[1]").InnerHtml;
                    Name = Name.Replace(':', '_');
                    Name = Name.Replace('/', '_');
                    Name = Name.Replace('\\', '_');
                    Name = Name.Replace('*', '_');
                    Name = Name.Replace('?', '_');
                    Name = Name.Replace('"', '_');
                    Name = Name.Replace('<', '_');
                    Name = Name.Replace('>', '_');
                    Name = Name.Replace('|', '_');

                    if(IgnoreOngoing)
                        {
                    if(Name.ToUpper().Contains("ONGOING"))
                    {
                        Console.WriteLine("Ongoing detected, ignoring");
                        return;
                    }
                    }


                    pages = main.ParentNode.ChildNodes[12].ChildNodes[1].ChildNodes[0].ChildNodes.Count - 2;

                    Console.WriteLine("Downloading " + Name +", "+pages+" pages");
                }else{
                    Console.WriteLine("Page "+page+" of "+pages);
                }

                if (main == null) return;

                if (!downloadImages) break;

                foreach (var el in main.ChildNodes)
                {
                    if (el.Attributes["class"].Value != "gdtm") continue;

                    var link = el.SelectSingleNode(el.XPath + "//div//a");

                    ParseImg(link.Attributes["href"].Value);

                    if (IsExceeded) break;
                }

                page++;
            } while (page < pages && !IsExceeded);

            Console.WriteLine("Downloaded "+downloaded+" images, "+ (imgnr-downloaded) +" ignored." + (IsExceeded ? " EXCEEDED!" : ""));
        }

        private void ParseImg(string url)
        {
            if (Directory.Exists(SavePath + "\\" + Name) == false)
            {
                Directory.CreateDirectory(SavePath + "\\" + Name);
            }
            else
            {
                if (File.Exists(SavePath + "\\" + Name + "\\" + imgnr + ".jpg"))
                {
                    imgnr++;
                    return;
                }
            }


            var imgLoad1 = web.Load(url);

            var nl = imgLoad1.GetElementbyId("loadfail");

            var nlurl = nl.Attributes["onclick"].Value;
            nlurl = nlurl.Remove(0, nlurl.IndexOf('\'') + 1);
            nlurl = nlurl.Remove(nlurl.IndexOf('\''));

            var imgLoad = web.Load(url + "?nl=" + nlurl);

            var a = imgLoad.GetElementbyId("i3");

            if (a == null) return;

            var img = a.SelectSingleNode(a.XPath + "//a//img");

            var imgLink = img.Attributes["src"].Value;

            var test = new Uri(imgLink);

            Changed = true;

            var test2 = SavePath + "\\" + Name + "\\" + imgnr++ + ".jpg";
            
            using (var wc = new WebClient())
            {
                try
                {
                    wc.DownloadFile(test, test2);

                    if (CheckExceed)
                    {
                        if (fileManager.isExceeded(test2))
                        {
                            File.Delete(test2);
                            IsExceeded = true;
                            return;
                        }
                    }

                    downloaded++;
                }catch(Exception ex)
                {
                    Console.WriteLine("Error downloading "+Name+" ("+imgnr+"): "+ex.Message);
                }
            }
        }
    }
}
