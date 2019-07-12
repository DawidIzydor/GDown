using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDownload
{
    class CbrManager
    {

        private bool _cbrlocked = false;

        public bool CreateCbr(string frompath, string savepath)
        {
            if (frompath == "" || savepath == "") return false;

            if (_cbrlocked) return false;

            _cbrlocked = true;

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
                _cbrlocked = false;
                return false;
            }

            _cbrlocked = false;
            return true;
        }

    }
}
