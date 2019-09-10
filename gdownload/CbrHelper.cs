using System;
using System.IO;
using System.IO.Compression;

namespace GDownload
{
    internal class CbrHelper
    {
        public bool CreateCbr(string pathFrom, string pathTo)
        {
            if (!ValidatePaths(pathFrom, pathTo))
            {
                return false;
            }

            try
            {
                DeleteExistingPath(pathTo);
                ZipFile.CreateFromDirectory(pathFrom, pathTo);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static void DeleteExistingPath(string pathTo)
        {
            if (File.Exists(pathTo))
            {
                File.Delete(pathTo);
            }
        }

        private static bool ValidatePaths(string frompath, string savepath) => frompath != "" && savepath != "";
    }
}