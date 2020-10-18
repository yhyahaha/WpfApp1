using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Data.Pdf;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var solutionDirectory = TryGetSolutionDirectoryInfo()?.FullName;
            //var pdfSamplePath = solutionDirectory + @"\Sample\小計.pdf";
			var pdfSamplePath = solutionDirectory + @"\Sample\小計.jpg";

			Windows.Storage.StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(pdfSamplePath);
			PdfDocument pdfDoc = await PdfDocument.LoadFromFileAsync(file);
			var pdfPage = pdfDoc.GetPage(0);



			var tesseractPath = solutionDirectory + @"\tesseract-master.1153";

			var imageFile = File.ReadAllBytes(pdfSamplePath);
			var text = ParseText(tesseractPath, imageFile, "eng", "jpn");
			Console.WriteLine("File:" + pdfSamplePath + "\n" + text + "\n");

		}

		private static string ParseText(string tesseractPath, byte[] imageFile, params string[] lang)
		{
			string output = string.Empty;
			var tempOutputFile = System.IO.Path.GetTempPath() + Guid.NewGuid();
			var tempImageFile = System.IO.Path.GetTempFileName();

			try
			{
				File.WriteAllBytes(tempImageFile, imageFile);

				ProcessStartInfo info = new ProcessStartInfo();
				info.WorkingDirectory = tesseractPath;
				info.WindowStyle = ProcessWindowStyle.Hidden;
				info.UseShellExecute = false;
				info.CreateNoWindow = true;
				info.FileName = "cmd.exe";
				info.Arguments =
					"/c tesseract.exe " +
					// Image file.
					tempImageFile + " " +
					// Output file (tesseract add '.txt' at the end)
					tempOutputFile +
					// Languages.
					" -l " + string.Join("+", lang);

				Console.WriteLine("Arguments:" + info.Arguments);

				// Start tesseract.
				Process process = Process.Start(info);
				process.WaitForExit();
				if (process.ExitCode == 0)
				{
					// Exit code: success.
					output = File.ReadAllText(tempOutputFile + ".txt");
				}
				else
				{
					throw new Exception("Error. Tesseract stopped with an error code = " + process.ExitCode);
				}
			}
			finally
			{
				File.Delete(tempImageFile);
				File.Delete(tempOutputFile + ".txt");
			}

			return output;
		}



		private static DirectoryInfo TryGetSolutionDirectoryInfo()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
                directory = directory.Parent;
            return directory;
        }


    }
}
