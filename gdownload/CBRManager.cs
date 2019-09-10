using System;
using System.IO;
using System.IO.Compression;

namespace GDownload
{
    internal class CbrManager
    {
        private bool _cbrlocked;

        public bool CreateCbr(string frompath, string savepath)
        {
            if (frompath == "" || savepath == "")
            {
                return false;
            }

            if (_cbrlocked)
            {
                return false;
            }

            _cbrlocked = true;

            try
            {
                if (File.Exists(savepath))
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