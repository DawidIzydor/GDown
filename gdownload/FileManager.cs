using System;
using System.IO;
using System.Security.Cryptography;

namespace GDownload
{
    internal class FileManager
    {
        private readonly string _exceedHash = "9B1F52FF4C49ED065946191D0937FACDC9E75C24E3102558CF78A9B19C96379D";

        //TODO
        private readonly string _exceedJpgFile = @"";

        public FileManager()
        {
            if (_exceedJpgFile != "")
            {
                _exceedHash = GetChecksum(_exceedJpgFile);
            }
        }

        public bool IsExceeded(string compareTo)
        {
            if (_exceedHash == "")
            {
                return false;
            }

            string compareHash = GetChecksum(compareTo);

            return compareHash == _exceedHash;
        }

        private string GetChecksum(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", string.Empty);
            }
        }
    }
}