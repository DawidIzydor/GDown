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
            GHent gHent = new GHent("D:\\test", "[Pixiv] Samurai (4342160)", "https://e-hentai.org/g/1289546/7e075e8870/");

            gHent.Parse();
        }
    }
}
