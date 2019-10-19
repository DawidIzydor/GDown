using System;
using System.IO;
using System.Net;
using GHentDownloaderLibrary.Abstract;
using HtmlAgilityPack;

namespace GHentDownloaderLibrary.Classes
{
    public class GImage : Image
    {
        private string SavePath => $"{SaveDirectoryPath}\\{Name}.jpg";

        public override CombinedItem Parse()
        {
            if (Directory.Exists(SaveDirectoryPath) == false)
            {
                Directory.CreateDirectory(SaveDirectoryPath);
            }
            else
            {
                if (File.Exists(SavePath))
                {
                    return null;
                }
            }

            using WebClient wc = new WebClient();
            wc.DownloadFile(new Uri(ExtractDirectImgLink()), SavePath);
            return this;
        }

        private string ExtractDirectImgLink()
        {
            HtmlNode a = Web.Load(NetPath + "?nl=" + ExtractNlUrl()).GetElementbyId("i3");

            return a?.SelectSingleNode(a.XPath + "//a//img").Attributes["src"].Value;
        }

        private string ExtractNlUrl()
        {
            HtmlNode nl = Web.Load(NetPath).GetElementbyId("loadfail");

            string nlurl = nl.Attributes["onclick"].Value;
            nlurl = nlurl.Remove(0, nlurl.IndexOf('\'') + 1);
            nlurl = nlurl.Remove(nlurl.IndexOf('\''));
            return nlurl;
        }
    }
}