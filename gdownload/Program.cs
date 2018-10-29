using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gdownload
{
    class Program
    {
        static void Main(string[] args)
        {
            string SavePath = "";
            List<string> htmlList = new List<string>()
            {
                
            };

            foreach (var html in htmlList)
            {

                GHent gHent = new GHent(SavePath, html);

                gHent.Parse();
            }
        }
    }
}
