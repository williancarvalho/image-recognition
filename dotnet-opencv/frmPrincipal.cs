using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using PocImageWork.Extensions;

namespace DotnetOpenCV
{
    public partial class frmPrincipal : Form
    {
        byte[] originalImage;

        public frmPrincipal()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            var dialogResult = openFileDialog.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                using (var stream = openFileDialog.OpenFile())
                {
                    var ms = new MemoryStream();
                    stream.CopyTo(ms);

                    originalImage = ms.ToArray();

                    pictureBox.SetImage(originalImage);
                }

                lblFileName.Text = openFileDialog.FileName;
                groupBoxActions.Enabled = true;
            }
        }

        private void frmPrincipal_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            trackBarPrecision.Value = trackBarPrecision.Minimum;
            pictureBox.SetImage(originalImage);
        }

        private void IdentifyBorders()
        {
            var img = OpenCvSharp.Mat.FromImageData(originalImage);

            ColorToHSV(screenColorPicker.Color, out double pixelHue, out double pixelSaturation, out double pixelValue);
            var precision = trackBarPrecision.Value;

            // normalize image
            var hsv_img = img.CvtColor(OpenCvSharp.ColorConversionCodes.BGR2HSV);

            pictureBox2.SetImage(hsv_img.ToBytes());

            var pixel_low = OpenCvSharp.InputArray.Create(new byte[] { (byte)pixelHue, (byte)pixelSaturation, (byte)(255 - precision) });
            var pixel_high = OpenCvSharp.InputArray.Create(new byte[] { (byte)pixelHue, (byte)precision, (byte)pixelValue });
            var curr_mask = hsv_img.InRange(pixel_low, pixel_high);
            var hsv_img_masked = hsv_img.SetTo(OpenCvSharp.InputArray.Create(new byte[] { 255, 255, 255 }), curr_mask);
            var img_rgb = hsv_img_masked.CvtColor(OpenCvSharp.ColorConversionCodes.HSV2RGB);

            pictureBox3.SetImage(img_rgb);

            // convert to gray scale to get countours
            var img_gray = img_rgb.CvtColor(OpenCvSharp.ColorConversionCodes.RGB2GRAY);
            var threshold = img_gray.Threshold(90, 255, 0);
            threshold.FindContours(out OpenCvSharp.Point[][] contours, out OpenCvSharp.HierarchyIndex[] hierarchy, OpenCvSharp.RetrievalModes.Tree, OpenCvSharp.ContourApproximationModes.ApproxSimple);

            pictureBox4.SetImage(img_gray);
            pictureBox5.SetImage(threshold);

            // draw countours
            img.DrawContours(contours, -1, new OpenCvSharp.Scalar(0, 0, 255), 3);
            
            pictureBox.SetImage(img_rgb);
        }


        


        public static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : min / max;
            value = max;
        }

        private void trackBarPrecision_Scroll(object sender, EventArgs e)
        {
            lblPrecision.Text = $"{(int)((decimal)trackBarPrecision.Value / (decimal)trackBarPrecision.Maximum * 100)}%";
            IdentifyBorders();
        }

        private void btn_analise4_Click(object sender, EventArgs e)
        {
            /*
              var img_gray = OpenCvSharp.Mat.FromImageData(pictureBox4.ImageToByte());

              ColorToHSV(screenColorPicker.Color, out double pixelHue, out double pixelSaturation, out double pixelValue);
              var precision = trackBarPrecision.Value;

              var pixel_low = OpenCvSharp.InputArray.Create(new byte[] { (byte)pixelHue, (byte)pixelSaturation, (byte)(255 - precision) });
              var pixel_high = OpenCvSharp.InputArray.Create(new byte[] { (byte)pixelHue, (byte)precision, (byte)pixelValue });
              var curr_mask = img_gray.InRange(pixel_low, pixel_high);
              var hsv_img_masked = img_gray.SetTo(OpenCvSharp.InputArray.Create(new byte[] { 255, 255, 255 }), curr_mask);

              var threshold = img_gray.Threshold(90, 255, 0);
              threshold.FindContours(out OpenCvSharp.Point[][] contours, out OpenCvSharp.HierarchyIndex[] hierarchy, OpenCvSharp.RetrievalModes.Tree, OpenCvSharp.ContourApproximationModes.ApproxSimple);

              pictureBox1.SetImage(img_gray.ToBytes());

              // draw countours
              img_gray.DrawContours(contours, -1, new OpenCvSharp.Scalar(0, 0, 255), 3);
              */
            //pictureBox4.Image.Save(@"c:\temp\folhas\gray_" + Path.GetFileName(lblFileName.Text) , ImageFormat.Jpeg);
            FindCountour();

        }

        private void DetectFace(CascadeClassifier cascade)
        {
            Mat result;

            var src = OpenCvSharp.Mat.FromImageData(pictureBox4.ImageToByte());

//            using (var src = new Mat(img, ImreadModes.Color))
            using (var gray = new Mat())
            {
                result = src.Clone();
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

                // Detect faces
                Rect[] faces = cascade.DetectMultiScale(
                    gray, 1.08, 2, HaarDetectionType.ScaleImage, new OpenCvSharp.Size(30, 30));

                // Render all detected faces
                foreach (Rect face in faces)
                {
                    var center = new OpenCvSharp.Point
                    {
                        X = (int)(face.X + face.Width * 0.5),
                        Y = (int)(face.Y + face.Height * 0.5)
                    };
                    var axes = new OpenCvSharp.Size
                    {
                        Width = (int)(face.Width * 0.5),
                        Height = (int)(face.Height * 0.5)
                    };
                    Cv2.Ellipse(result, center, axes, 0, 0, 360, new Scalar(255, 0, 255), 4);
                }
            }
            pictureBox5.Image = BitmapConverter.ToBitmap(result);
            
        }

        public void FindCountour()
        {
            //fonte: https://www.tech-quantum.com/contour-detection-in-an-image/?unapproved=522&moderation-hash=42b9ee5b7066327344207f9fe7c3f16c#comment-522
            //var image = OpenCvSharp.Mat.FromImageData(pictureBox.ImageToByte());
            var image = Cv2.ImRead(lblFileName.Text);

            //Convert to gray
            Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
            //Threshold the image
            Mat thresholdImage = new Mat();
            Cv2.Threshold(gray, thresholdImage, 128, 255, ThresholdTypes.Binary);
            Cv2.ImShow("Thresold", thresholdImage);

            pictureBox2.SetImage(thresholdImage);

            // ocorreu um erro bem aqui na linha de baixo, OpenCvSharp.OpenCVException: '_kernel.type() == CV_8U'

            //Run Open and Close morphological transformation
            Cv2.MorphologyEx(thresholdImage, thresholdImage, MorphTypes.Open, InputArray.Create(new Scalar(5, 5)));
            Cv2.MorphologyEx(thresholdImage, thresholdImage, MorphTypes.Close, InputArray.Create(new Scalar(5, 5)));
            //Threshold the image again
            Cv2.Threshold(thresholdImage, thresholdImage, 128, 255, ThresholdTypes.BinaryInv);
            Cv2.ImShow("Morph", thresholdImage);

            pictureBox3.SetImage(thresholdImage);

            //Find contours
            Mat[] contours = null;
            Mat hierarcy = new Mat();
            Cv2.FindContours(thresholdImage, out contours, hierarcy, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
            //Draw contours
            Scalar greenColor = new Scalar(0, 255, 0);
            Cv2.DrawContours(image, contours, -1, greenColor, 2);
            Cv2.ImShow("image", image);

            pictureBox4.SetImage(thresholdImage);
            pictureBox5.SetImage(image);
            

            /*
            //OpenCvSharp.InputOutputArray thresholdImage;
            
            var image = OpenCvSharp.Mat.FromImageData(pictureBox4.ImageToByte());

            Mat[] contours = null;
            Mat hierarcy = new Mat();
            var cvtimage = thresholdImage.CvtColor(OpenCvSharp.ColorConversionCodes.);
            Cv2.FindContours(cvtimage, out contours, hierarcy, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
            //Draw contours
            Scalar greenColor = new Scalar(0, 255, 0);
            Cv2.DrawContours(image, contours, -1, greenColor, 2);
            Cv2.ImShow("image", image);

            pictureBox1.SetImage(image);
            */
        }


    }
}
