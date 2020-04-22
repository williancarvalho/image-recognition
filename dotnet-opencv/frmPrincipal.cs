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
            ResetImage();
        }

        private void btnDettectByColor_Click(object sender, EventArgs e)
        {
            var dialogResult = colorDialog.ShowDialog();

            if(dialogResult == DialogResult.OK)
            {
                txtColorToDettect.BackColor = colorDialog.Color;

                var img = OpenCvSharp.Mat.FromImageData(originalImage);
                
                // normalize image
                var hsv_img = img.CvtColor(OpenCvSharp.ColorConversionCodes.BGR2HSV);
                var green_low = OpenCvSharp.InputArray.Create(new byte[] { 45, 100, 50 });
                var green_high = OpenCvSharp.InputArray.Create(new byte[] { 75, 255, 255 });
                var curr_mask = hsv_img.InRange(green_low, green_high);
                var hsv_img_masked = hsv_img.SetTo(OpenCvSharp.InputArray.Create(new byte[] { 75, 255, 200 }), curr_mask);
                var img_rgb = hsv_img_masked.CvtColor(OpenCvSharp.ColorConversionCodes.HSV2RGB);

                // convert to gray scale to get countours
                var img_gray = img_rgb.CvtColor(OpenCvSharp.ColorConversionCodes.RGB2GRAY);
                var threshold = img_gray.Threshold(90, 255, 0);
                threshold.FindContours(out OpenCvSharp.Point[][] contours, out OpenCvSharp.HierarchyIndex[] hierarchy, OpenCvSharp.RetrievalModes.Tree, OpenCvSharp.ContourApproximationModes.ApproxSimple);

                // draw countours
                img.DrawContours(contours, -1, new OpenCvSharp.Scalar(0, 0, 255), 3);

                var output = new Bitmap(new MemoryStream(img.ToBytes()));
                pictureBox.Image = output;
                pictureBox.Refresh();
            }
        }
    }
}
