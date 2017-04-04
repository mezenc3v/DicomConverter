using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace DICOMConverter
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Image<Gray, double>[] _arrayImages;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            ReadFromFolder(sliderIntensity.RangeStart, sliderIntensity.RangeEnd);
        }

        private void ReadFromFolder(double minIntensivity, double maxIntensivity)
        {
            var folderBrowserDialog = new FolderBrowserDialog();
            var result = folderBrowserDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    var fileNames = Directory.GetFiles(folderBrowserDialog.SelectedPath, "*.dcm");
                    var dcmManager = new DicomManager();

                    _arrayImages = dcmManager.ImagesDicom(dcmManager, fileNames, minIntensivity, maxIntensivity);
                    dcmManager.Xyz(_arrayImages, minIntensivity, maxIntensivity);
                }
                catch (Exception ex)
                {
                    textBoxLogs.Text = "Error: Could not read file from disk. Original error: " + ex.Message;
                }
            }
        }
    }
}
