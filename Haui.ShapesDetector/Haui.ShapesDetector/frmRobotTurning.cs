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
        private string mess = "";
        private bool isChangeSelect = false;
        public frmRobotTurning()
        {
            _robotService = new RobotService();
            InitializeComponent();
        }

        private void tr_ValueChanged(object sender, EventArgs e)
        {
            txtPointLocate.Text = string.Format("{0},{1},{2},{3}", trJ1.Value.ToString(), trJ2.Value.ToString(), trJ3.Value.ToString(), trJ4.Value.ToString());
            if (!isChangeSelect && mess != txtPointLocate.Text)
            {
                Robot.Write("t" + txtPointLocate.Text);
                mess = txtPointLocate.Text;
            }
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            Robot.Write("h");
        }

        private void txtPointLocate_TextChanged(object sender, EventArgs e)
        {
            //Robot.Write("t" + txtPointLocate.Text);
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            Robot.Write("t" + txtPointLocate.Text);
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
            MessageBox.Show("Lưu vị trí robot thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmRobotTurning_Load(object sender, EventArgs e)
        {
            if (!Robot.IsOpen)
            {
                Robot.PortName = clsFileIO.ReadValue("COM_ROBOT");
                Robot.BaudRate = int.Parse(clsFileIO.ReadValue("BAURATE_ROBOT"));
                Robot.Open();
                Robot.DataReceived += Robot_DataReceived;
            }

            cboPos.SelectedIndex = 0;
            //isChangeSelect = true;
        }

        private void Robot_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

        }

        private void cboPos_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                isChangeSelect = true;
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
                    pos = "90,90,90,90";
                }
                var posArray = pos.Split(',');
                trJ1.Value = int.Parse(posArray[0]);
                trJ2.Value = int.Parse(posArray[1]);
                trJ3.Value = int.Parse(posArray[2]);
                trJ4.Value = int.Parse(posArray[3]);
                isChangeSelect = false;
                string _mess = string.Format("{0},{1},{2},{3}", trJ1.Value.ToString(), trJ2.Value.ToString(), trJ3.Value.ToString(), trJ4.Value.ToString());
                Robot.Write("t" + _mess);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải vị trí robot. Vui lòng kiểm tra lại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmRobotTurning_FormClosing(object sender, FormClosingEventArgs e)
        {
            Robot.Close();
            this.DialogResult = DialogResult.OK;
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            int changeValue = int.Parse(txtChangeValue.Text); // Giá trị thay đổi, có thể điều chỉnh theo nhu cầu
            Button btn = sender as Button;
            switch (btn.Name)
            {
                case "btnUp1":
                    trJ1.Value += changeValue;
                    break;
                case "btnUp2":
                    trJ2.Value += changeValue;
                    break;
                case "btnUp3":
                    trJ3.Value += changeValue;
                    break;
                case "btnUp4":
                    trJ4.Value += changeValue;
                    break;
            }
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            int changeValue = int.Parse(txtChangeValue.Text); // Giá trị thay đổi, có thể điều chỉnh theo nhu cầu
            Button btn = sender as Button;
            switch (btn.Name)
            {
                case "btnDown1":
                    trJ1.Value -= changeValue;
                    break;
                case "btnDown2":
                    trJ2.Value -= changeValue;
                    break;
                case "btnDown3":
                    trJ3.Value -= changeValue;
                    break;
                case "btnDown4":
                    trJ4.Value -= changeValue;
                    break;
            }
        }

    }
}
