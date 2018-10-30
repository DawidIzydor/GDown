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
        private string ExceedJpgFile = @"D:\dwh\comics\ghent\exceeded.jpg";

        private string ExceedHash;

        public FileManager()
        {
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
