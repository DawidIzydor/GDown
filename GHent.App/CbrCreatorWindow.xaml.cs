using System.IO;
using System.Windows;
using GHent.Tools;

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
        /// <exception cref="T:System.IO.DirectoryNotFoundException">
        ///     The specified savePath is invalid (for example, it is on an
        ///     unmapped drive).
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref /> is a zero-length string, contains only white space, or
        ///     contains one or more invalid characters. You can query for invalid characters by using the
        ///     <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">
        ///     The specified savePath, file name, or both exceed the system-defined
        ///     maximum length.
        /// </exception>
        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            var regenerate = RegenerateCheck.IsChecked ?? false;
            var savePath = PathBox.Text;

            var dirs = Directory.GetDirectories(savePath);

            foreach (var sourceDirectory in dirs)
            {
                CbrGenerator.GenerateCbrFromDirectory(sourceDirectory, savePath, regenerate);
            }
        }
    }
}