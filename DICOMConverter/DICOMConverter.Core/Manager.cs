using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using EvilDICOM.Core;
using EvilDICOM.Core.Element;
using EvilDICOM.Core.Helpers;

namespace DICOMConverter.Core
{
    public class Manager
    {
        private IEnumerable<DICOMObject> ReadAllObjects(string[] dicomFiles)
        {
            return dicomFiles.Select(name => DICOMObject.Read(name)).ToList();
        }

        private Image<Gray, double> GrayscaleImageFromDicomObject(DICOMObject obj, double minValueIntensity, double maxValueIntensity)
        {
            var str = obj.PixelStream;
            var height = ((UnsignedShort)obj.FindFirst(TagHelper.ROWS)).Data;
            var width = ((UnsignedShort)obj.FindFirst(TagHelper.COLUMNS)).Data;

            var image = new Image<Gray, double>(width, height);

            var bytesPerPixel = ((UnsignedShort)obj.FindFirst(TagHelper.BITS_ALLOCATED)).Data / 8;

            str.Position = 0;

            var buffer = new byte[width * bytesPerPixel];

            for (int y = 0; y < height; y++)
            {
                str.Read(buffer, 0, width * bytesPerPixel);

                for (int x = 0; x < width; x++)
                {
                    var pixel = image[x, y];

                    double intensity = 0;

                    for (int i = 1; i <= bytesPerPixel; i++)
                    {
                        intensity += buffer[x * bytesPerPixel + bytesPerPixel - i];
                    }

                    if (intensity < maxValueIntensity && intensity > minValueIntensity)
                    {
                        pixel.Intensity = intensity;
                        image[y, x] = pixel;
                    }
                    else
                    {
                        pixel.Intensity = 0;
                        image[y, x] = pixel;
                    }
                }
            }
            return image;
        }

        public List<Image<Gray, double>> GrayscaleImagesFromDicom(string[] fileNames, double minValueIntensity, double maxValueIntensity)
        {
            var dcmList = ReadAllObjects(fileNames).OrderBy(dcm => dcm.FindFirst(TagHelper.LOCATION));

            var imgs = new Image<Gray, double>[dcmList.Count()].ToList();

            Parallel.For(0, imgs.Count, k =>
            {
                imgs[k] = GrayscaleImageFromDicomObject(dcmList.ElementAt(k), minValueIntensity, maxValueIntensity);
            });

            return imgs;
        }

        public void SaveToXyz(List<Image<Gray, double>> images, double minIntensity, double maxIntensity, string fileName)
        {
            var file = new StreamWriter(fileName);

            for (int z = 0; z < images.Count; z++)
                for (int x = 0; x < images[z].Cols; x++)
                    for (int y = 0; y < images[z].Rows; y++)
                    {
                        if (images[z][x, y].Intensity > minIntensity && images[z][x, y].Intensity < maxIntensity)
                        {
                            file.Write(x + " " + y + " " + z + "\r\n");
                        }
                    }
            file.Close();
        }
    }
}
