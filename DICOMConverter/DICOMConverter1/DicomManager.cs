using Emgu.CV;
using Emgu.CV.Structure;
using EvilDICOM.Core;
using EvilDICOM.Core.Element;
using EvilDICOM.Core.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICOMConverter
{
    public class DicomManager
    {
        public List<DICOMObject> ReadAllObjects(string[] dicomFiles)
        {
            List<DICOMObject> dcmObjects = new List<DICOMObject>();

            foreach (string name in dicomFiles)
            {
                dcmObjects.Add(DICOMObject.Read(name));
            }
            return dcmObjects;
        }

        public Image<Gray, double>[] ImagesDicom(DicomManager dcmManager, string[] fileNames, double minValueIntensity, double maxValueIntensity)
        {
            List<DICOMObject> dcmList = dcmManager.ReadAllObjects(fileNames);

            dcmList = dcmList.OrderBy(dcm => dcm.FindFirst(TagHelper.LOCATION)).ToList();

            Image<Gray, double>[] imgs = new Image<Gray, double>[dcmList.Count];

            for (int k = 0; k < dcmList.Count; k++)
            {
                Stream str = dcmList[k].PixelStream;

                ushort height = ((UnsignedShort)dcmList[k].FindFirst(TagHelper.ROWS)).Data;
                ushort width = ((UnsignedShort)dcmList[k].FindFirst(TagHelper.COLUMNS)).Data;

                imgs[k] = new Image<Gray, double>(width, height);

                int bytesPerPixel = ((UnsignedShort)dcmList[k].FindFirst(TagHelper.BITS_ALLOCATED)).Data / 8;

                str.Position = 0;

                byte[] buffer = new byte[width * bytesPerPixel];

                for (int y = 0; y < height; y++)
                {
                    str.Read(buffer, 0, width * bytesPerPixel);

                    for (int x = 0; x < width; x++)
                    {
                        var pixel = imgs[k][x, y];

                        double intensity = 0;

                        for (int i = 1; i < bytesPerPixel + 1; i++)
                        {
                            intensity += buffer[x * bytesPerPixel + bytesPerPixel - i];
                        }

                        if (intensity < maxValueIntensity && intensity > minValueIntensity)
                        {
                            pixel.Intensity = intensity;
                            imgs[k][y, x] = pixel;
                        }
                        else
                        {
                            pixel.Intensity = 0;
                            imgs[k][y, x] = pixel;
                        }
                    }
                }
            }
            return imgs;
        }

        public void XYZ(Image<Gray, double>[] imgs, int minIntensity, int maxIntensity)
        {
            StreamWriter file = new StreamWriter("C:\\Users\\kochka100\\Desktop\\TestFile.xyz");

            for (int z = 0; z < imgs.Length; z++)
                for (int x = 0; x < imgs[z].Cols; x++)
                    for (int y = 0; y < imgs[z].Rows; y++)
                    {
                        if (imgs[z][x, y].Intensity > minIntensity && imgs[z][x, y].Intensity < maxIntensity)
                            file.Write(x + " " + y + " " + z + "\r\n");
                    }

            file.Close();
        }
    }
}
