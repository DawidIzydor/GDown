using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace gdownload
{
    class GHent
    {
        HtmlWeb web = new HtmlWeb();

        readonly string SavePath;
        string Name;
        readonly string url;

        int imgnr = 0;

        bool parsed = false;
        
        public GHent(string SavePath, string url)
        {
            this.SavePath = SavePath;
            this.url = url;
        }
        
        public void Parse()
        {
            if (parsed) return;

            int page = 0;
            
            HtmlNode main;

            do
            {
                main = web.Load(url+"?p="+page)?.GetElementbyId("gdt");

                if(page == 0)
                {
                    Name = main.SelectSingleNode("//body[1]//div[1]//div[2]//h1[1]").InnerHtml;
                }

                if (main == null) return;

                foreach (var el in main.ChildNodes)
                {
                    if (el.Attributes["class"].Value != "gdtm") continue;

                    var link = el.SelectSingleNode(el.XPath + "//div//a");

                    ParseImg(link.Attributes["href"].Value);
                }

                page++;
            } while (main.SelectSingleNode("//body[1]//div[3]//table[1]//tbody[1]//tr[1]//td["+page+1+"]")?.Attributes["class"].Value != "ptdd");
        }

        private void ParseImg(string url)
        {
            var imgLoad1 = web.Load(url);
            
            var nl = imgLoad1.GetElementbyId("loadfail");

            var nlurl = nl.Attributes["onclick"].Value;
            nlurl = nlurl.Remove(0, nlurl.IndexOf('\'')+1);
            nlurl = nlurl.Remove(nlurl.IndexOf('\''));

            var imgLoad = web.Load(url + "?nl=" + nlurl);


            var a = imgLoad.GetElementbyId("i3");
            
            if (a == null) return;

            var img = a.SelectSingleNode(a.XPath + "//a//img");

            var imgLink = img.Attributes["src"].Value;

            var test = new Uri(imgLink);

            if(Directory.Exists(SavePath + "\\" + Name) == false)
            {
                Directory.CreateDirectory(SavePath + "\\" + Name);
            }

            var test2 = SavePath + "\\" + Name + "\\" + imgnr++ + ".jpg";
            
            using (var wc = new WebClient())
            {
                wc.DownloadFileAsync(test, test2);
            }
        }
    }
}
