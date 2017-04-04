using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DICOMConverter
{
    public partial class MainForm : Form
    {
        private Image<Gray, double>[] arrayImages;

        public MainForm()
        {
            InitializeComponent();
        }

        private void buttonOpenFolder_Click(object sender, EventArgs e)
        {
            ReadFromFolder(0, 0, 5000);
        }

        private void ReadFromFolder(int numberFile, int minIntensivity, int maxIntensivity)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string[] fileNames = Directory.GetFiles(folderBrowserDialog.SelectedPath, "*.dcm");
                    DicomManager dcmManager = new DicomManager();

                    arrayImages = dcmManager.ImagesDicom(dcmManager, fileNames, minIntensivity, maxIntensivity);
                    dcmManager.XYZ(arrayImages, minIntensivity, maxIntensivity);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }
    }
}
