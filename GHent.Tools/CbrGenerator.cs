using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GHent.Tools
{
    public class CbrGenerator
    {
        /// <exception cref="T:System.IO.DirectoryNotFoundException">
        ///     <paramref name="sourceDirectory" /> is invalid or does not
        ///     exist (for example, it is on an unmapped drive).
        /// </exception>
        /// <exception cref="T:System.IO.IOException">
        ///     <paramref /> already exists.
        ///     -or-
        ///     A file in the specified directory could not be opened.
        /// </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        ///     <paramref /> specifies a directory.
        ///     -or-
        ///     The caller does not have the required permission to access the directory specified in <paramref /> or the file
        ///     specified in <paramref />.
        /// </exception>
        public static void GenerateCbrFromDirectory(string sourceDirectory, string savePath, bool regenerateCbr = false)
        {
            var dirName = sourceDirectory.Split(Path.PathSeparator).Last();
            var cbrPath = $"{Path.Combine(savePath, dirName)}.cbr";
            if (regenerateCbr == false && File.Exists(cbrPath))
            {
                return;
            }

            ZipFile.CreateFromDirectory(sourceDirectory, cbrPath);
        }
    }
}