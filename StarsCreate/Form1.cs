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
    /// <summary>
    /// СТруктура синхронизации
    /// </summary>
    internal struct ParamContent
    {
        public SynchronizationContext uiContext;
        public int kol;
        public int StartValue;
        public int Step;

        /// <summary>
        /// Инициализатор
        /// </summary>
        /// <param name="Ui">Синхронизатор</param>
        /// <param name="kol">Количество звёзд</param>
        /// <param name="start">Начальное значение</param>
        /// <param name="step">Прощение</param>
        public ParamContent(SynchronizationContext Ui, int kol, int start, int step)
        {
            this.uiContext = Ui;
            this.kol = kol;
            this.StartValue = start;
            this.Step = step;
        }
    }

    public partial class Form1 : Form
    {
        private Random rd;          // рандомайзер

        public float Et;
        public int spp = 4;
        public float sigm = 1;

        private List<Thread> threads = new List<Thread>();

        private ParamContent[] Pc;
        public CreatePicture CreatePic;
        public int KolParallel = 2;         // количество потоков
        public int FinishThread;            // количество завершенных потоков

        private Stopwatch st = new Stopwatch();     // для подсчёта времени

        public int kol;                             // количество изображений

        // контекст синхронизации
        private SynchronizationContext uiContext;

        private string FileNameDir = "D:\\JavaStarTrak\\Nikita\\Output_png\\";      // путь к папке, где генерить изображения

        public Form1()
        {
            InitializeComponent();

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
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            //trackBar1.Maximum = Convert.ToInt32(textBox4.Text.Length > 0 ? textBox4.Text : "0");
            CreatePic = new CreatePicture(
                Convert.ToInt32(textBox2.Text.Length > 0 ? textBox2.Text : "0"),        // ширина
                Convert.ToInt32(textBox1.Text.Length > 0 ? textBox1.Text : "0"),        // высота
                rd.Next(100, 500),                                                      // количество генерируемых звёзд
                Convert.ToInt32(textBox3.Text.Length > 0 ? textBox3.Text : "0"),        // Шум
                Convert.ToInt32(textBox4.Text.Length > 0 ? textBox4.Text : "0"),        // фоновая подставка
                Convert.ToSingle(textBox5.Text.Length > 0 ? textBox5.Text : "0"),       // Энергия трека
                spp,
                sigm,
                "D:\\JavaStarTrak\\Nikita\\Output_png\\1.png");                         // пусть сохранения
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            CreatePic.CreateStars();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            //CreatePic.GetBitmap().Save16bitBitmapToPng("D:\\JavaStarTrak\\Nikita\\Output_png\\1.png");
            CreatePic.BitMapSave();
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
            textBox3.Text = textBox4.Text.Length > 0 ? textBox4.Text : "0";
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

                if (a > Convert.ToInt32(textBox4.Text.Length > 0 ? textBox4.Text : "0"))

                    textBox3.Text = textBox4.Text;
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
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            st.Restart();
            st.Start();

            CreatePicture CreatePic2 = new CreatePicture(
                    Convert.ToInt32(textBox2.Text.Length > 0 ? textBox2.Text : "0"),        // ширина
                    Convert.ToInt32(textBox1.Text.Length > 0 ? textBox1.Text : "0"),        // высота
                    rd.Next(100, 500),                                                      // количество генерируемых звёзд
                    Convert.ToInt32(textBox3.Text.Length > 0 ? textBox3.Text : "0"),        // Шум
                    Convert.ToInt32(textBox4.Text.Length > 0 ? textBox4.Text : "0"),        // фоновая подставка
                    Convert.ToSingle(textBox5.Text.Length > 0 ? textBox5.Text : "0"),       // Энергия трека
                    spp,
                    sigm,
                    FileNameDir + "0.png");      // пусть сохраненияCreatePicture CreatePic1 = new CreatePicture(

            CreatePic2.StartCreate();
            st.Stop();
            toolStripStatusLabel1.Text = "Создано за " + st.Elapsed.ToString();
        }

        /// <summary>
        /// Этот метод исполняется в основном UI потоке
        /// </summary>
        public void UpdateUI(object state)
        {
            string text = state as string;

            //label5.Text = n.ToString();

            switch (text)
            {
                case "StartProccec":
                    {
                        break;
                    }

                case "WorkProccec":
                    {
                        progressBar1.Value = progressBar1.Value + 1 < progressBar1.Maximum ? progressBar1.Value + 1 : progressBar1.Maximum;

                        toolStripStatusLabel2.Text = progressBar1.Value.ToString();

                        toolStripStatusLabel4.Text = TimeToStreamCorect(st.Elapsed.Ticks);

                        toolStripStatusLabel6.Text = TimeReturn(st.Elapsed.Ticks, Convert.ToInt64(progressBar1.Value));
                        break;
                    }
                case "StopProccec":
                    {
                        FinishThread++;
                        if (FinishThread >= KolParallel)
                        {
                            st.Stop();
                            progressBar1.Value = progressBar1.Maximum;
                            //toolStripStatusLabel1.Text = "Создано " + progressBar1.Value.ToString() + " изображений за " + st.Elapsed.ToString() + "  Завершено!!!";

                            toolStripStatusLabel2.Text = progressBar1.Value.ToString(); toolStripStatusLabel2.Text = progressBar1.Value.ToString();
                            toolStripStatusLabel4.Text = TimeToStreamCorect(st.Elapsed.Ticks);
                            threads.Clear();
                        }
                        break;
                    }

                default:
                    {
                        toolStripStatusLabel1.Text = text;
                        Thread.Sleep(500);
                        break;
                    }
            }
        }

        private string TimeReturn(long t, long x)
        {
            long s;
            s = t * kol;
            s = s / x;
            s = s - t;

            DateTime date = new DateTime(s);

            return date.ToLongTimeString();
        }

        private static string TimeToStreamCorect(long s)
        {
            DateTime date = new DateTime(s);

            return date.ToLongTimeString();
        }

        private void Button8_Click(object sender, EventArgs e)
        {
            kol = Convert.ToInt32(textBox6.Text);
            KolParallel = Convert.ToInt32(textBox7.Text);
            FinishThread = 0;
            Pc = new ParamContent[KolParallel];

            st.Restart();
            st.Start();
            progressBar1.Maximum = kol;
            progressBar1.Value = 0;
            uiContext = SynchronizationContext.Current;
            //Pc.uiContext = uiContext;
            //Pc.kol = kol;

            for (int i = 0; i < KolParallel; i++)
            {
                Thread thread = new Thread(Procces);
                threads.Add(thread);
                Pc[i] = new ParamContent(uiContext, kol, i, KolParallel);
            }

            Thread.Sleep(100);

            for (int i = 0; i < KolParallel; i++)
            {
                threads[i].Start(Pc[i]);
                //Thread.Sleep(250);
            }

            //Thread = new Thread(Procces);

            //Thread.Start(Pc);

            toolStripStatusLabel2.Text = "0";
        }

        private void Procces(object state)
        {
            ParamContent Pc = (ParamContent)state;

            // вытащим контекст синхронизации из state'а
            //SynchronizationContext uiContext = state as SynchronizationContext;
            SynchronizationContext uiContent = Pc.uiContext;
            int kol = Pc.kol;
            uiContext.Post(UpdateUI, "StartProccec");

            for (int i = Pc.StartValue; i < kol; i += Pc.Step)
            {
                try
                {
                    Random rd1 = new Random(i + DateTime.Now.Millisecond);
                    CreatePicture CreatePic1 = new CreatePicture(
                    Convert.ToInt32(textBox2.Text.Length > 0 ? textBox2.Text : "0"),        // ширина
                    Convert.ToInt32(textBox1.Text.Length > 0 ? textBox1.Text : "0"),        // высота
                    rd.Next(100, 500),                                                      // количество генерируемых звёзд
                    Convert.ToInt32(textBox3.Text.Length > 0 ? textBox3.Text : "0"),        // Шум
                    Convert.ToInt32(textBox4.Text.Length > 0 ? textBox4.Text : "0"),        // фоновая подставка
                    Convert.ToSingle(textBox5.Text.Length > 0 ? textBox5.Text : "0"),       // Энергия трека
                    spp,
                    sigm,
                    FileNameDir + i.ToString() + ".png");      // пусть сохранения

                    CreatePic1.StartCreate();

                    uiContext.Post(UpdateUI, "WorkProccec");
                }
                catch (ThreadAbortException exc)
                {
                    uiContext.Post(UpdateUI, "Поток прерван!!!");
                }
            }

            uiContext.Post(UpdateUI, "StopProccec");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (threads.Count != 0)
            {
                for (int i = 0; i < KolParallel; i++)
                    threads[i].Abort();

                for (int i = 0; i < KolParallel; i++)
                    threads[i].Join();
            }
        }
    }
}