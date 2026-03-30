using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Haui.ShapesDetector
{
    public partial class frmRobotTurning : Form
    {
        public frmRobotTurning()
        {
            InitializeComponent();
        }

        private void tr_ValueChanged(object sender, EventArgs e)
        {
            txtPointLocate.Text = string.Format("{0},{1},{2},{3}", trJ1.Value.ToString(), trJ2.Value.ToString(), trJ3.Value.ToString(), trJ4.Value.ToString());
        }

        private void txtPointLocate_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
