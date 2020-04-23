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

                    ResetImage();
                }

                lblFileName.Text = openFileDialog.FileName;
                groupBoxActions.Enabled = true;
            }
        }

        private void ResetImage()
        {
            pictureBox.Image = new Bitmap(new MemoryStream(originalImage));
            pictureBox.Refresh();
        }

        private void frmPrincipal_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            trackBarPrecision.Value = trackBarPrecision.Minimum;
            ResetImage();
        }

        private void IdentifyBorders()
        {
            var img = OpenCvSharp.Mat.FromImageData(originalImage);

            ColorToHSV(screenColorPicker.Color, out double pixelHue, out double pixelSaturation, out double pixelValue);
            var precision = trackBarPrecision.Value;

            // normalize image
            var hsv_img = img.CvtColor(OpenCvSharp.ColorConversionCodes.BGR2HSV);
            var pixel_low = OpenCvSharp.InputArray.Create(new byte[] { (byte)pixelHue, (byte)pixelSaturation, (byte)(255 - precision) });
            var pixel_high = OpenCvSharp.InputArray.Create(new byte[] { (byte)pixelHue, (byte)precision, (byte)pixelValue });
            var curr_mask = hsv_img.InRange(pixel_low, pixel_high);
            var hsv_img_masked = hsv_img.SetTo(OpenCvSharp.InputArray.Create(new byte[] { 255, 255, 255 }), curr_mask);
            var img_rgb = hsv_img_masked.CvtColor(OpenCvSharp.ColorConversionCodes.HSV2RGB);

            // convert to gray scale to get countours
            var img_gray = img_rgb.CvtColor(OpenCvSharp.ColorConversionCodes.RGB2GRAY);
            var threshold = img_gray.Threshold(90, 255, 0);
            threshold.FindContours(out OpenCvSharp.Point[][] contours, out OpenCvSharp.HierarchyIndex[] hierarchy, OpenCvSharp.RetrievalModes.Tree, OpenCvSharp.ContourApproximationModes.ApproxSimple);

            // draw countours
            img.DrawContours(contours, -1, new OpenCvSharp.Scalar(0, 0, 255), 3);

            var output = new Bitmap(new MemoryStream(img_rgb.ToBytes()));
            pictureBox.Image = output;
            pictureBox.Refresh();
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
    }
}
