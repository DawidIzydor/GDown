using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;

namespace GHent.App
{
    /// <summary>
    ///     Logika interakcji dla klasy CbrCreatorWindow.xaml
    /// </summary>
    public partial class CbrCreatorWindow
    {
        public CbrCreatorWindow()
        {
            InitializeComponent();
            PathBox.Text = AppSettings.Default.LastSavePath;
        }

        /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.IO.IOException"><paramref /> is a file name.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.ArgumentException"><paramref /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters by using the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            var regenerate = RegenerateCheck.IsChecked ?? false;
            var path = PathBox.Text;

            var dirs = Directory.GetDirectories(path);

            foreach (var dir in dirs)
            {
                var dirName = dir.Split("/").Last();
                var cbrPath = $"{Path.Combine(path, dirName)}.cbr";
                if (regenerate == false && File.Exists(cbrPath))
                {
                    continue;
                }

                ZipFile.CreateFromDirectory(dir, cbrPath);
            }
        }
    }
}