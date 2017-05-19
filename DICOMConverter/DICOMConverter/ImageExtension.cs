using Emgu.CV;
using System.IO;
using System.Windows.Media.Imaging;
using Emgu.CV.Structure;

namespace DICOMConverter.Wpf
{
    public static class ImageExtension
    {
        public static BitmapImage BitmapImage(this Image<Gray,double> bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                var bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}
