using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StarsCreate
{
    public partial class Form1 : Form
    {
        private int[,] PixValue;
        private int h, w;
        private Random rd;
        private Bitmap bm;

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
            h = Convert.ToInt32(textBox1.Text);
            w = Convert.ToInt32(textBox2.Text);
            PixValue = new int[w, h];
            bm = new Bitmap(w, h);
            ValToBitMap();

            //pictureBox1.Image = Image.FromHbitmap(bm.GetHbitmap());
            pictureBox1.Image = bm;
        }

        private void ValToBitMap()
        {
            for (int index = 0; index < bm.Width; index++)
            {
                for (int i = 0; i < bm.Height; i++)
                {
                    bm.SetPixel(index, i, Color.FromArgb(PixValue[index, i], PixValue[index, i], PixValue[index, i]));
                }
            }
        }
    }
}