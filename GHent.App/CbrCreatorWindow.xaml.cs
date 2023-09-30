using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;

namespace GHent.App
{
    /// <summary>
    ///     Logika interakcji dla klasy CbrCreatorWindow.xaml
    /// </summary>
    public partial class CbrCreatorWindow : Window
    {
        public CbrCreatorWindow()
        {
            InitializeComponent();
            PathBox.Text = AppSettings.Default.LastSavePath;
        }

        private void GenerateButon_Click(object sender, RoutedEventArgs e)
        {
            var regenerate = RegenerateCheck.IsChecked ?? false;
            var path = PathBox.Text;

            var dirs = Directory.GetDirectories(path);

            foreach (var dir in dirs)
            {
                var dirName = dir.Split("\\").Last();
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