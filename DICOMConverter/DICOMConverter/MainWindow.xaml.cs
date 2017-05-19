using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using DICOMConverter.Core;

namespace DICOMConverter.Wpf
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private List<Image<Gray, double>> _arrayImages;
        private readonly Manager _dcmManager;
        private int _currIndex;
        private string[] _fileNames;

        public MainWindow()
        {
            InitializeComponent();
            _dcmManager = new Manager();
        }

        private void ButtonOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            CreateImagesFromDicom();
        }

        private void ButtonSaveFolder_Click(object sender, RoutedEventArgs e)
        {
            SaveToFolder();
        }

        private string[] ReadFromFolder()
        {
            var folderBrowserDialog = new FolderBrowserDialog();
            var result = folderBrowserDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    return Directory.GetFiles(folderBrowserDialog.SelectedPath, "*.dcm");
                }
                catch (Exception ex)
                {
                    TextBoxLogs.Text = "Error: Could not read file from disk. Original error: " + ex.Message + "\r\n";
                }
            }
            throw new Exception();
        }

        private async void CreateImagesFromDicom()
        {
            try
            {
                _fileNames = ReadFromFolder();

                await Task.Run(() =>
                {
                    double min = 0;
                    double max = 0;
                    Dispatcher.Invoke(() =>
                    {
                        min = SliderIntensity.RangeStart;
                        max = SliderIntensity.RangeEnd;
                        TextBoxLogs.AppendText("Processing, please wait...\r\n");
                    });
                    var images = _dcmManager.GrayscaleImagesFromDicom(_fileNames, min, max);

                    Dispatcher.Invoke(() =>
                    {
                        _arrayImages = images;
                    });
                });

                _currIndex = 0;
                TextBoxLogs.AppendText("Done!\r\n");
                Image.Source = _arrayImages[_currIndex].BitmapImage();
            }
            catch (Exception ex)
            {
                TextBoxLogs.Text = "Error:" + ex.Message + "\r\n";
            }
        }

        private async void SaveToFolder()
        {
            try
            {
                var fileDialog = new SaveFileDialog
                {
                    FileName = "Dicom 3d model",
                    DefaultExt = ".xyz",
                    Filter = @"XYZ Files |.xyz",
                    RestoreDirectory = true
                };

                if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var name = "";
                    double min = 0;
                    double max = 0;

                    await Task.Run(() =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            min = SliderIntensity.RangeStart;
                            max = SliderIntensity.RangeEnd;
                            name = fileDialog.FileName;
                            TextBoxLogs.AppendText("Saving, please wait...\r\n");
                        });

                        _dcmManager.SaveToXyz(_arrayImages, min, max, name);
                    });

                    TextBoxLogs.AppendText("Done!\r\n");

                }
            }
            catch (Exception ex)
            {
                TextBoxLogs.Text = "Error: " + ex.Message;
            }
        }

        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            if (_arrayImages.Any())
            {
                if (_currIndex <= 0)
                {
                    _currIndex = _arrayImages.Count - 1;
                }

                Image.Source = _arrayImages[_currIndex].BitmapImage();
                _currIndex--;
            }
        }

        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            if (_arrayImages.Any())
            {
                if (_currIndex >= _arrayImages.Count)
                {
                    _currIndex = 0;
                }
                Image.Source = _arrayImages[_currIndex].BitmapImage();
                _currIndex++;
            }
        }
    }
}
