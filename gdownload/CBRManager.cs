using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gdownload
{
    class CBRManager
    {

        private bool cbrlocked = false;

        public bool CreateCbr(string frompath, string savepath)
        {
            if (frompath == "" || savepath == "") return false;

            if (cbrlocked) return false;

            cbrlocked = true;

            try
            {

                if (File.Exists(savepath) == true)
                {
                    File.Delete(savepath);
                }

                ZipFile.CreateFromDirectory(frompath, savepath);
            }
            catch (Exception)
            {
                cbrlocked = false;
                return false;
            }

            cbrlocked = false;
            return true;
        }

    }
}
