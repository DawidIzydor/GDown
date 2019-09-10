using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            CbrHelper cBrHelper = new CbrHelper();

            if (!Directory.Exists(CBRLocation))
            {
                Directory.CreateDirectory(CBRLocation);
            }

            foreach (GHentSite gHent in htmlList.Select(html => new GHentSite(SavePath, html)))
            {
                gHent.Parse();

                if (gHent.Changed)
                {
                    cBrHelper.CreateCbr(SavePath + "\\" + gHent.Name, CBRLocation + "\\" + gHent.Name + ".cbr");
                }

                if (gHent.Exceeded)
                {
                    break;
                }
            }
        }
    }
}