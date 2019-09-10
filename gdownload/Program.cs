using System.Collections.Generic;
using System.IO;
using GDownload.GHent;

namespace GDownload
{
    internal class Program
    {
        private static void Main()
        {
            string SavePath = @"";
            string CBRLocation = @"";

            // ReSharper disable once CollectionNeverUpdated.Local
            List<string> htmlList = new List<string>();

            CbrManager cBRManager = new CbrManager();

            if (!Directory.Exists(CBRLocation))
            {
                Directory.CreateDirectory(CBRLocation);
            }

            foreach (string html in htmlList)
            {
                GHentSite gHent = new GHentSite(SavePath, html);

                gHent.Parse();

                if (gHent.Changed)
                {
                    cBRManager.CreateCbr(SavePath + "\\" + gHent.Name, CBRLocation + "\\" + gHent.Name + ".cbr");
                }

                if (gHent.IsExceeded)
                {
                    break;
                }
            }
        }
    }
}