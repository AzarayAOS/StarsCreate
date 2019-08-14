using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace StarsCreate
{
    public partial class Form1 : Form
    {
        private Random rd;          // рандомайзер

        public float Et;
        public int spp = 4;
        public float sigm = 1;

        public CreatePicture CreatePic;

        private Stopwatch st = new Stopwatch();     // для подсчёта времени

        public Form1()
        {
            InitializeComponent();
            textBox3.Text = trackBar1.Value.ToString();
            rd = new Random();
        }

        private void TextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8) // цифры и клавиша BackSpace
            {
                e.Handled = true;
            }
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            textBox3.Text = trackBar1.Value.ToString();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            trackBar1.Maximum = Convert.ToInt32(textBox4.Text);
            CreatePic = new CreatePicture(
                Convert.ToInt32(textBox2.Text),
                Convert.ToInt32(textBox1.Text),
                rd.Next(100, 500),
                Convert.ToInt32(textBox3.Text),
                Convert.ToInt32(textBox4.Text),
                Convert.ToSingle(textBox5.Text),
                spp,
                sigm);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            CreatePic.CreateStars();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            CreatePic.GetBitmap().Save16bitBitmapToPng("D:\\JavaStarTrak\\Nikita\\Output_png\\1.png");
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            CreatePic.AddNoise();
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            st.Start();

            CreatePic.PixToBitAll();
            st.Stop();
            toolStripStatusLabel1.Text = "Отображено за " + st.Elapsed.ToString();
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            CreatePic.CreateTrack();
        }

        private void TextBox4_TextChanged(object sender, EventArgs e)
        {
        }

        private void TextBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8) // цифры и клавиша BackSpace
            {
                e.Handled = true;
            }
        }

        private void TextBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8) // цифры и клавиша BackSpace
            {
                e.Handled = true;
            }
        }

        private void TextBox3_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox3.Text))
            {
                int a = Convert.ToInt32(textBox3.Text);
                if (a > Convert.ToInt32(textBox4.Text)) textBox3.Text = textBox4.Text;
            }
        }

        private void TextBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8) // цифры и клавиша BackSpace
            {
                e.Handled = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void TrackBar1_ValueChanged(object sender, EventArgs e)
        {
            textBox3.Text = trackBar1.Value.ToString();
        }
    }
}