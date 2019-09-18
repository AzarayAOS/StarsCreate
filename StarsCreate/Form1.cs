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
        private SynchronizationContext uiContext;
        public int kol;
        public int StartValue;
        public int Step;

        public SynchronizationContext UiContext { get => uiContext; set => uiContext = value; }

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
        private bool flagprocces;   // флаг запуска процесса генерации серии кадров

        private List<Thread> threads = new List<Thread>();

        private ParamContent[] Pc;
        public CreatePicture CreatePic;
        public int KolParallel = 2;         // количество потоков
        public int FinishThread;            // количество завершенных потоков

        private Stopwatch st = new Stopwatch();     // для подсчёта времени

        public int kol;                             // количество изображений

        // контекст синхронизации
        private SynchronizationContext uiContext;

        //private string FileNameDir = "D:\\JavaStarTrak\\Nikita\\Output_png";      // путь к папке, где генерить изображения
        private string FileNameDir = Application.StartupPath;

        public Form1()
        {
            InitializeComponent();
            textBox8.Text = FileNameDir + "\\";
            rd = new Random();
            flagprocces = true;
        }

        private void TextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8) // цифры и клавиша BackSpace
            {
                e.Handled = true;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (System.IO.Directory.Exists(textBox8.Text))
            {
                FileNameDir = textBox8.Text;
            }

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
                FileNameDir + "\\1",                        // пусть сохранения
                    checkBox1.Checked,                      // разрешение на генерацию звёзд
                    checkBox2.Checked,                      // разрешение на генерацию шума
                    checkBox3.Checked,                      // разрешение на генерацию трека
                    checkBox4.Checked                       // разрешение на запись координат трека в файл
                    );
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

            toolStripStatusLabel2.Text = progressBar1.Value.ToString();
            toolStripStatusLabel4.Text = TimeToStreamCorect(st.Elapsed.Ticks);
            toolStripStatusLabel6.Text = "  Завершено!!!";
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
            
            
                int a = Convert.ToInt32(textBox3.Text);

                if (a > Convert.ToInt32(textBox4.Text.Length > 0 ? textBox4.Text : "0"))

                    if (!string.IsNullOrEmpty(textBox3.Text))
                        textBox3.Text = textBox4.Text;
            
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
            if (System.IO.Directory.Exists(textBox8.Text))
            {
                FileNameDir = textBox8.Text;
            }

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
                    FileNameDir + "1",                      // пусть сохранения
                    checkBox1.Checked,                      // разрешение на генерацию звёзд
                    checkBox2.Checked,                      // разрешение на генерацию шума
                    checkBox3.Checked,                      // разрешение на генерацию трека
                    checkBox4.Checked                       // разрешение на запись координат трека в файл
                    );
            CreatePic2.StartCreate();
            st.Stop();
            toolStripStatusLabel2.Text = progressBar1.Value.ToString();
            toolStripStatusLabel4.Text = TimeToStreamCorect(st.Elapsed.Ticks);
            toolStripStatusLabel6.Text = "  Завершено!!!";
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

                            toolStripStatusLabel2.Text = progressBar1.Value.ToString();
                            toolStripStatusLabel4.Text = TimeToStreamCorect(st.Elapsed.Ticks);
                            toolStripStatusLabel6.Text = "  Завершено!!!";
                            threads.Clear();

                            StopProcces();
                            flagprocces = true;

                            button8.Text = "Генерация набора изображений";
                            button1.Enabled = true;
                            button2.Enabled = true;
                            button3.Enabled = true;
                            button4.Enabled = true;
                            button5.Enabled = true;
                            button6.Enabled = true;
                            button7.Enabled = true;
                            groupBox2.Enabled = true;
                        }
                        break;
                    }

                default:
                    {
                        toolStripStatusLabel6.Text = text;
                        Thread.Sleep(1000);
                        break;
                    }
            }
        }

        private string TimeReturn(long t, long x)
        {
            long s;
            s = t * kol;
            s /= x;
            s -= t;

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
            if (flagprocces)
            {
                if (System.IO.Directory.Exists(textBox8.Text))
                {
                    FileNameDir = textBox8.Text;
                }

                threads.Clear();
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
                flagprocces = false;

                button8.Text = "Остановить!";
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
                button6.Enabled = false;
                button7.Enabled = false;
                groupBox2.Enabled = false;
            }
            else
            {
                StopProcces();
                flagprocces = true;

                button8.Text = "Генерация набора изображений";
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;
                button6.Enabled = true;
                button7.Enabled = true;
                groupBox2.Enabled = true;
            }
        }

        private void Procces(object state)
        {
            ParamContent Pc = (ParamContent)state;

            // вытащим контекст синхронизации из state'а
            //SynchronizationContext uiContext = state as SynchronizationContext;
            SynchronizationContext uiContent = Pc.UiContext;
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
                    FileNameDir + i.ToString(),             // пусть сохранения
                    checkBox1.Checked,                      // разрешение на генерацию звёзд
                    checkBox2.Checked,                      // разрешение на генерацию шума
                    checkBox3.Checked,                      // разрешение на генерацию трека
                    checkBox4.Checked                       // разрешение на запись координат трека в файл
                    );

                    CreatePic1.StartCreate();

                    uiContext.Post(UpdateUI, "WorkProccec");
                }
                catch (ThreadAbortException)
                {
                    uiContext.Post(UpdateUI, "Поток прерван!!!");
                }
            }

            uiContext.Post(UpdateUI, "StopProccec");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopProcces();
        }

        private void CheckBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox3.Checked)
            {
                checkBox4.Checked = false;
                checkBox4.Enabled = false;
            }
            else
                checkBox4.Enabled = true;
        }

        private void Button9_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog Fbd = new FolderBrowserDialog
            {
                SelectedPath = FileNameDir,
                Description = "Укажите пусть к папке, в которую будут сохраняться сгенерированные изображения."
            };

            if (Fbd.ShowDialog() == DialogResult.OK)
            {
                if (System.IO.Directory.Exists(Fbd.SelectedPath))
                {
                    FileNameDir = Fbd.SelectedPath + "\\";
                    textBox8.Text = FileNameDir;
                }
            }
        }

        private void StopProcces()
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