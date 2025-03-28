using GHent.Shared.ProgressReporter;
using System.IO;
using System.IO.Compression;

namespace GHent.Shared.CbrCreator
{
    public class ZipFileCbrCreator(IEventableProgressReporter progressReporter) : ICbrCreator
    {
        public void CreateCbr(string saveToPath, string createFromPath)
        {
            string cbrFileName = GetCbrFileName(saveToPath, createFromPath);

            if (File.Exists(cbrFileName))
            {
                progressReporter.Report(ProgressType.Information, amount: 0, message: $"Will override the cbr file", reportedItem: cbrFileName);
                File.Delete(cbrFileName);
            }
            progressReporter.Report(ProgressType.Information, amount: 0, message: $"Will save {createFromPath} into {cbrFileName}", reportedItem: cbrFileName);
            ZipFile.CreateFromDirectory(createFromPath, cbrFileName);
        }

        private static string GetCbrFileName(string savePathText, string directoryPath)
        {
            var dirSplit = directoryPath.Split('\\');
            var filename = dirSplit[^1] != "" ? dirSplit[^1] : dirSplit[^2];
            var cbrFileName = Path.Combine(savePathText, filename) + ".cbr";
            return cbrFileName;
        }
    }
}