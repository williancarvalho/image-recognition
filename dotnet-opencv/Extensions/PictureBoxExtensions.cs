using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;

namespace PocImageWork.Extensions
{
    public static class PictureBoxExtensions
    {
        public static void SetImage(this PictureBox pct, byte[] imageBytes)
        {
            pct.Image = new Bitmap(new MemoryStream(imageBytes));
            pct.Refresh();
        }

        public static void SetImage(this PictureBox pct, OpenCvSharp.Mat image)
        {
            var imageBytes = image.ToBytes();
            pct.Image = new Bitmap(new MemoryStream(imageBytes));
            pct.Refresh();
        }

        public static byte[] ImageToByte(this PictureBox pct)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(pct.Image, typeof(byte[]));
        } 
    }
}
