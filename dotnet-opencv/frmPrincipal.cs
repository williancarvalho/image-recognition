using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotnetOpenCV
{
    public partial class frmPrincipal : Form
    {
        Bitmap originalImage;
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
                    originalImage = new Bitmap(stream, false);
                    pictureBox.Image = originalImage;
                    pictureBox.Refresh();
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
            pictureBox.Image = originalImage;
            pictureBox.Refresh();
        }

        private void btnDettectByColor_Click(object sender, EventArgs e)
        {
            var dialogResult = colorDialog.ShowDialog();

            if(dialogResult == DialogResult.OK)
            {
                txtColorToDettect.BackColor = colorDialog.Color;
            }
        }
    }
}
