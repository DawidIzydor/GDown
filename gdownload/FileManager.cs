using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace gdownload
{
    class FileManager
    {
        //TODO
        private string ExceedJpgFile = @"";

        private string ExceedHash = "9B1F52FF4C49ED065946191D0937FACDC9E75C24E3102558CF78A9B19C96379D";

        public FileManager()
        {
            if(ExceedJpgFile != "")
            ExceedHash = GetChecksum(ExceedJpgFile);
        }

        public string GetChecksum(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }

        public bool isExceeded(string compareTo)
        {
            if (ExceedHash == "") return false;

            string compareHash = GetChecksum(compareTo);

            if(compareHash == ExceedHash)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
