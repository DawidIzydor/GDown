using System;
using System.IO;
using HtmlAgilityPack;

namespace GDownload.GHent
{
    // ReSharper disable once UnusedMember.Global
    internal class GHentTag
    {
        private readonly CbrHelper _cBrHelper = new CbrHelper();

        public GHentTag(string savePath, string url)
        {
            SavePath = savePath;
            Url = url;
        }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public string CbrLocation { get; }
        public bool CheckExceeded { get; } = true;

        public bool Exceeded { get; private set; }

        public bool IgnoreOngoing { get; set; } = false;
        public string SavePath { get; }
        public string Url { get; }

        // ReSharper disable once UnusedMember.Global
        public void Parse()
        {
            Console.WriteLine("Searching by tag");
            Console.WriteLine("[IgnoreOngoing]: " + IgnoreOngoing);
            Console.WriteLine("[checkExceeded]: " + CheckExceeded);

            HtmlNodeCollection tableRows = DownloadTableRows();

            foreach (HtmlNode tableRow in tableRows)
            {
                if (!ParseRow(tableRow))
                {
                    break;
                }
            }

            if (Exceeded)
            {
                Console.WriteLine("Exceeded");
            }
            else
            {
                Console.WriteLine("Tag downloaded OK");
            }
        }

        private bool CanGenerateCbrFromSite(GHentSite gHentSite) => gHentSite != null &&
                                                                    IgnoreOngoing &&
                                                                    gHentSite.OngoingFound == false &&
                                                                    CbrLocation != "" &&
                                                                    gHentSite.Name != null &&
                                                                    (gHentSite.Changed ||
                                                                     File.Exists(
                                                                         CbrLocation + "\\" + gHentSite.Name +
                                                                         ".cbr") == false);

        private GHentSite DownloadGHentSite(string link)
        {
            GHentSite gHentSite = new GHentSite(SavePath, link, CheckExceeded) {IgnoreOngoing = IgnoreOngoing};
            gHentSite.Parse();
            return gHentSite;
        }

        private GHentSite DownloadGHentSiteFromRow(HtmlNode tableRow)
        {
            GHentSite gHentSite = null;

            if (!SkipTableHeader(tableRow.ChildNodes[0].Name))
            {
                string link = TryGetLinkFromRow(tableRow);

                if (link != "")
                {
                    gHentSite = DownloadGHentSite(link);
                }
            }

            return gHentSite;
        }

        private HtmlNodeCollection DownloadTableRows()
        {
            HtmlNode page = new HtmlWeb().Load(Url).DocumentNode;
            HtmlNodeCollection tableRows = page.SelectNodes("//table[contains(@class,'itg')]//tr");
            return tableRows;
        }

        private void GenerateCbr(GHentSite gHentSite)
        {
            if (CanGenerateCbrFromSite(gHentSite))
            {
                Console.WriteLine("Generating new CBR");

                if (_cBrHelper.CreateCbr(SavePath + "\\" + gHentSite.Name,
                    CbrLocation + "\\" + gHentSite.Name + ".cbr"))
                {
                    Console.WriteLine("OK");
                }
                else
                {
                    Console.WriteLine("Error");
                }
            }
        }

        private bool ParseRow(HtmlNode tableRow)
        {
            GHentSite gHentSite = DownloadGHentSiteFromRow(tableRow);

            GenerateCbr(gHentSite);

            if (gHentSite?.Exceeded ?? false)
            {
                Exceeded = true;
                return false;
            }

            return true;
        }

        private static bool SkipTableHeader(string name) => name == "th";

        private static string TryGetLinkFromRow(HtmlNode tableRow)
        {
            string link = "";

            try
            {
                link = tableRow.ChildNodes[2].ChildNodes[0].Attributes["href"].Value;
            }
            catch (Exception)
            {
                Console.WriteLine("Problem with parsing " + link);
            }

            return link;
        }
    }
}