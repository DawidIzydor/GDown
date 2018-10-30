using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gdownload
{
    class Program
    {
        static void Main(string[] args)
        {
            string SavePath = @"";
            string CBRLocation = @"";

            List<string> htmlList = new List<string>()
            {
            };

            CBRManager cBRManager = new CBRManager();

            if(!Directory.Exists(CBRLocation))
            {
                Directory.CreateDirectory(CBRLocation);
            }

            foreach (var html in htmlList)
            {
                GHent gHent = new GHent(SavePath, html);

                gHent.Parse();

                if (gHent.Changed)
                {
                    cBRManager.CreateCbr(SavePath + "\\" + gHent.Name, CBRLocation + "\\" + gHent.Name + ".cbr");
                }

                if(gHent.isExceeded)
                {
                    break;
                }
            }
        }
    }
}
