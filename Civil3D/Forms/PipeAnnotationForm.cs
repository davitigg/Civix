using System;
using System.Windows.Forms;
using Civil3D.Enums;

namespace Civil3D.Forms
{
    public partial class PipeAnnotationForm : Form
    {
        public string Material => comboBox2.Text;
        public bool IsCircular => radioButton1.Checked;
        public uint DiameterMm => Convert.ToUInt32(textBox1.Text);
        public uint WidthMm => Convert.ToUInt32(textBox3.Text);
        public uint HeightMm => Convert.ToUInt32(textBox4.Text);
        public string AdditionalInfo => textBox2.Text;

        public PipeAnnotationForm(string description)
        {
            InitializeComponent();
            Text = description;
            AcceptButton = button1;
            CancelButton = button2;
        }

        private void RestrictInputToDigits(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            panel1.Visible = radioButton1.Checked;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            panel2.Visible = radioButton2.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(comboBox2.Text))
            {
                MessageBox.Show("გთხოვთ აირჩიოთ მასალა.", "შეცდომა", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (radioButton1.Checked && string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("გთხოვთ შეიყვანოთ დიამეტრი.", "შეცდომა", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (radioButton2.Checked)
            {
                if (string.IsNullOrWhiteSpace(textBox3.Text) || string.IsNullOrWhiteSpace(textBox4.Text))
                {
                    MessageBox.Show("გთხოვთ შეიყვანოთ სიგანე და სიმაღლე", "შეცდომა", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}