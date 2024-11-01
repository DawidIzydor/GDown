using GHent.Shared.ProgressReporter;
using System.IO;
using System.IO.Compression;

namespace GHent.Shared.CbrCreator
{
    public class CbrCreator(IProgressReporter<ProgressData<string>> progressReporter) : ICbrCreator
    {
        public void CreateCbr(string saveToPath, string createFromPath)
        {
            string cbrFileName = GetCbrFileName(saveToPath, createFromPath);

            if (File.Exists(cbrFileName))
            {
                progressReporter?.ReportWithDone(new ProgressData<string> { Type = ProgressType.Information, Value = cbrFileName, Information = "Will override" }, 0);
                File.Delete(cbrFileName);
            }
            progressReporter?.ReportWithDone(new ProgressData<string>
            {
                Type = ProgressType.Information,
                Value = $"Will save {createFromPath} into {cbrFileName}"
            }, 0);
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