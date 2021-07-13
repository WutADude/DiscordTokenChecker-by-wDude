using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using xNet;

namespace DiscordTokenChecker_by_wDude
{
    public partial class Form1 : Form
    {
        //Подключение класса функций
        Functions Functions = new Functions();

        // Константы для премещения формы при зажатии мыши
        public const int WM_NCLBUTTONDOWN = 0xA1; // Событие при нажатии левой кнопки мыши
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam); 

        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public Form1() // Инструкции после инициализации формы
        {
            InitializeComponent();
            Functions.MainForm = this; // Ссылка на главную форму в классе функций
            Functions._proxyType = ProxyType.Http; // Стандартный тип используемых прокси, который будет использован если пользователь не выбрал тип сам
        }

        private void Form1_Load(object sender, EventArgs e) // Инструкции при загрузке формы
        {
            Functions.CheckStartPath(); // Проверка на папку, из которой запускается программа
        }

        private void button1_Click(object sender, EventArgs e) // Закрытие формы по нажатию кнопки
        {
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e) // Сворачивание формы по нажатию кнопки
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e) // Передвижение формы при зажатии левой кнопки мыши на панели в верхней части кнопки
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void button3_Click(object sender, EventArgs e) // Главная кнопка, отвечающая за импорт токенов и запуск чека
        {
            if (!Functions.TokensImported)
            {
                Functions.GetTokensFromFile();
            }
            else if (Functions.ThreadsList.Count > 0)
            {
                Functions.StopWorking();
            }
            else
            {
                Functions.StartWorking();
                panel3.Enabled = false;
            }
        }

        private void button3_MouseEnter(object sender, EventArgs e)
        {
            if (Functions.TokensImported && Functions.ThreadsList.Count > 0)
                button3.Text = "Остановить";
            else if (Functions.TokensImported) 
                button3.Text = "Начать проверку токенов";
        }

        private void button3_MouseLeave(object sender, EventArgs e)
        {
            if (Functions.TokensImported && Functions.ThreadsList.Count > 0)
                button3.Text = "В процессе...";
            else
                button3.Text = $"Импортированно токенов: {Functions.TokenList.Count}";
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            label3.Text = $"{trackBar1.Value}";
            Functions.numOfThreads = trackBar1.Value;
            if (trackBar1.Value > Functions.TokenList.Count && Functions.TokensImported)
                MessageBox.Show("ОСТОРОЖНО! Если потоков больше, чем количество импортированных токенов,\n" +
                    "то программа может вылететь при старте! Пожалуйста уменьшите кол-во потоков.", "ВНИМАНИЕ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void button4_Click(object sender, EventArgs e) //Кнопка, отвечающая за импорт прокси
        {
            Functions.GetProxysFromFile();
        }

        private void button4_MouseEnter(object sender, EventArgs e)
        {
            if (Functions.ProxysImported)
                button4.Text = "Добавить прокси";
        }

        private void button4_MouseLeave(object sender, EventArgs e)
        {
            if (Functions.ProxysImported)
                button4.Text = $"Импортированно прокси: {Functions.ProxyList.Count}"; 
        }

        private void radioButton1_Click(object sender, EventArgs e) // Использовать HTTP прокси
        {
            Functions._proxyType = ProxyType.Http;
        }

        private void radioButton2_Click(object sender, EventArgs e) // Использовать SOCKS4 прокси
        {
            Functions._proxyType = ProxyType.Socks4;
        }

        private void radioButton3_Click(object sender, EventArgs e) // Использовать SOCKS5 прокси
        {
            Functions._proxyType = ProxyType.Socks5;
        }

        private void timer1_Tick(object sender, EventArgs e) //Таймер, обновляет счётчики  и текст контроллов каждые 50 милисек
        {
            leftTokensLabel.Text = Functions.TokenList.Count.ToString();
            goodTokensLabel.Text = Functions.GoodTokens.Count.ToString();
            verifiedTokensLabel.Text = Functions.VerifiedTokens.Count.ToString();
            unverefiedTokensLabel.Text = Functions.UnverifiedTokens.Count.ToString();
            paymentTokensLabel.Text = Functions.WithPaymentMethodsTokens.Count.ToString();
            nopaymentTokensLabel.Text = Functions.WithoutPaymentMethodsTokens.Count.ToString();
            nitro_fullTokensLabel.Text = Functions.WithFullNitroTokens.Count.ToString();
            nitro_classicTokensLabel.Text = Functions.WithClassicNitroTokens.Count.ToString();
            threadsOnWork.Text = Functions.ThreadsList.Count.ToString();
            deletedDoublesLabel.Text = Functions.numOfDoubles.ToString();
            if (Functions.ThreadsList.Count == 0)
                panel3.Enabled = true;
            if (Functions.ProxysImported)
                button4.Text = $"Осталось прокси: {Functions.ProxyList.Count}";
        }

        private void label16_MouseEnter(object sender, EventArgs e) // Цвет текста при наведении
        {
            label16.ForeColor = System.Drawing.Color.FromArgb(114, 137, 218);
        }

        private void label16_MouseLeave(object sender, EventArgs e) // Стандартный цвет текста
        {
            label16.ForeColor = System.Drawing.Color.FromArgb(166, 168, 170);
        }

        private void label16_Click(object sender, EventArgs e)
        {
            Process.Start("https://lolz.guru/wdude/");
        }
    }
}
