using Haui.ShapesDetector.Common;
using Haui.ShapesDetector.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;

namespace Haui.ShapesDetector
{
    public partial class frmRobotTurning : Form
    {
        private readonly RobotService _robotService;
        private SerialPort Robot = new SerialPort();

        public frmRobotTurning()
        {
            _robotService = new RobotService();
            InitializeComponent();
        }

        private void tr_ValueChanged(object sender, EventArgs e)
        {
            txtPointLocate.Text = string.Format("{0},{1},{2},{3}", trJ1.Value.ToString(), trJ2.Value.ToString(), trJ3.Value.ToString(), trJ4.Value.ToString());

            //Robot.Write("m" + txtPointLocate.Text);
        }

        private void txtPointLocate_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string key;
            if (cboPos.SelectedIndex == 0)
            {
                key = "Pos0";
            }
            else
            {
                key = cboPos.Text.Replace("Material ", "Pos");
            }

            _robotService.UpdatePosition(key, txtPointLocate.Text);

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Robot.Close();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void frmRobotTurning_Load(object sender, EventArgs e)
        {
            cboPos.SelectedIndex = 0;
            //if (!Robot.IsOpen)
            //{
            //    Robot.PortName = clsFileIO.ReadValue("COM_ROBOT");
            //    Robot.BaudRate = int.Parse(clsFileIO.ReadValue("BAURATE_ROBOT"));
            //    Robot.Open();
            //    Robot.DataReceived += Robot_DataReceived;
            //}
        }

        private void cboPos_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string key;
                if (cboPos.SelectedIndex == 0)
                {
                    key = "Pos0";
                }
                else
                {
                    key = cboPos.Text.Replace("Material ", "Pos");
                }
                string pos = _robotService.GetPosition(key);
                if (string.IsNullOrEmpty(pos))
                {
                    pos = "0,0,0,0";
                }
                var posArray = pos.Split(',');
                trJ1.Value = int.Parse(posArray[0]);
                trJ2.Value = int.Parse(posArray[1]);
                trJ3.Value = int.Parse(posArray[2]);
                trJ4.Value = int.Parse(posArray[3]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải vị trí robot. Vui lòng kiểm tra lại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
    }
}
