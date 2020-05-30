using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using WMPLib;
using System.IO;
using System.Data.Sql;
using MyExeptions;
using System.Xml;


namespace Game_Dota2
{
    // Перечесление с типами ходов игрока.
    enum Type { Attack, Defend, Escape }
    public partial class Form1 : Form
    {
        // Определение кнопки включения\выключения звука.
        bool VolumeButtonIsActivited = true;
        // Определение кнопки включения\выключения настроек.
        bool settingButtonIsActivited = true;
        // Определение положения кнопки для раскрытия панели с командами.
        bool ArrowIsClose = true;
        // Лист с названиями треков.
        string[] playlist;
        // Медиа плейеры.
        WindowsMediaPlayer mediaPlayer = new WindowsMediaPlayer();
        WindowsMediaPlayer media_2 = new WindowsMediaPlayer();
        WindowsMediaPlayer repeatPlayer = new WindowsMediaPlayer();
        // Изначальная громкость RepeatPlayer.
        int countForMain = 100;
        static Random rnd = new Random();
        // Рандомное число времени ожидания поиска игры.
        int gameTime = rnd.Next(5, 11);
        // Переменные для вывода времени поиска игры.
        int minute = 0, second = 0;
        // Путь к звукам для игры.
        string pathToSounds = "..\\..\\..\\Sounds\\";
        string pathToPicture = "..\\..\\..\\Picture\\";
        // Изначальное количество золота.
        int countGold = 650;
        // Типы для работы одного из таймеров в приложении.
        string type = "ListForGame";
        // Шапка считываемого файла.
        string head = "name;type;baseStr;baseAgi;baseInt;moveSpeed;baseArmor;minDmg;regeneration";
        // Кнопки для героев.
        PictureBox[] hero;
        // Информация о героях игры.
        List<string[]> informationUnits = new List<string[]>();
        // Индекс выбранного героя.
        int index;
        // Геймлогика
        GameLogic game;
        // Выбранные параметры пользователем.
        string[] ChosenParametrs = new string[3];
        // Поинт курсора.
        Point lastPoint;
        bool flagToStartBattle = false;

        public Form1()
        {
            InitializeComponent();
            // Назначение репитплеера.
            repeatPlayer = mediaPlayer;
            // Назначение кнопки выхода из приложение.
            Exit.Click += (sender, args) => { Close(); };
            // Назначение кнопки, отвечающий за выдвигающуюся панель.
            Arrow.Click += (sender, args) => { goingPanelInBattle.Enabled = true; };
            // Парсинг информации о героях.
            ParserUnits();
            // Создание таблицы
            CreateTable();
            // Подгрузка фона.
            BackgroundImage = Image.FromFile(pathToPicture + "Main.jpg");
            // Создание кнопок выбора героев.
            hero = new PictureBox[informationUnits.Count];
            for (int i = 0; i < hero.Length; i++)
                hero[i] = new PictureBox();
            // Подгрузка файлов для игры.
            Resourses();
            // Запуск фоновой музыки.
            mediaPlayer.URL = "MainMenu.mp3";
        }
        /// <summary>
        /// Создание таблицы dataGridView.
        /// </summary>
        private void CreateTable()
        {
            try
            {
                string[] infoHead = head.Split(';');
                for (int x = 0; x < infoHead.Length; x++)
                {
                    dataAboutHeroes.Columns.Add("Header", infoHead[x]);
                    dataAboutHeroes.Columns[x].SortMode = DataGridViewColumnSortMode.NotSortable;
                }
                for (int y = 0; y < informationUnits.Count; y++)
                    dataAboutHeroes.Rows.Add(informationUnits[y]);
            }
            catch (ArgumentException)
            {
                MessageBox.Show("При создании таблицы произошла ошибка", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("При создании таблицы произошла ошибка", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }
        /// <summary>
        /// Парсинг инофрмации из файла dota2.csv
        /// </summary>
        private void ParserUnits()
        {
            try
            {   // Массив героев,которые используются, в случае повреждениz или изменения исходника.
                string[] heroes = new string[] { "Abaddon;0;23;17;21;310;1.43;55;1.5", "Death Prophet; 2; 17; 14; 23; 310; 3; 47; 2" };
                // Считывание информации о героях.
                string[] info = File.ReadAllLines("..\\..\\..\\Dota2.csv");
                //Замена точек на запятые для дальнейшего парса даблов.
                for (int i = 0; i < info.Length; i++)
                    info[i] = info[i].Replace('.', ',');
                // В игре не может быть больше 120 героев.
                if (info.Length > 119)
                    throw new MyException("Слишком много героев.");
                // Выбрасывание эксепшена, если элементов в файле 0.
                if (info.Length == 0)
                    throw new MyException("В исходном файле ненайдено элементов.");
                // Проверка шапки файла.
                if (info[0] != head)
                    throw new MyException("Шапка в файле не соответсвтует исходной.");
                // Если в файле только шапка, то добавлем 1 героя. Иначе сканируем весь файл и парсим его в список массивов.
                // записываем только тех героев, у которых параметров, столько, сколько в шапке.
                // Так же идет проверка на значения характеристик.
                if (info.Length == 1)
                {
                    informationUnits.Add(heroes[0].Split(';'));
                    informationUnits.Add(heroes[1].Split(';'));
                }
                else
                    for (int i = 1; i < info.Length; i++)
                        if (CheckInformation(info[i]))
                            informationUnits.Add(info[i].Split(';'));
                // Провека, что после удаления пустых строк и сломанных героев, Остались герои.
                // Если героев нет.
                if (informationUnits.Count == 0)
                {
                    informationUnits.Add(heroes[0].Split(';'));
                    informationUnits.Add(heroes[1].Split(';'));
                }
                // Если героя один.
                if (informationUnits.Count == 1)
                    if (informationUnits[0] == heroes[0].Split(';'))
                        informationUnits.Add(heroes[1].Split(';'));
                    else
                        informationUnits.Add(heroes[0].Split(';'));
            }
            catch (ArgumentException)
            {
                if (DialogResult.Yes == MessageBox.Show("При работе с файлом произошла ошикбка" + "\nВосстановить исходник?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error))
                    RecoveryFile(this, new EventArgs());
                else
                    Close();
            }
            catch (FileNotFoundException)
            {
                if (DialogResult.Yes == MessageBox.Show("Исходный файл не найден" + "\nВосстановить исходник?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error))
                    RecoveryFile(this, new EventArgs());
                else
                    Close();
            }
            catch (IOException)
            {
                if (DialogResult.Yes == MessageBox.Show("При работе с файлом произошла ошикбка" + "\nВосстановить исходник?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error))
                    RecoveryFile(this, new EventArgs());
                else
                    Close();
            }
            catch (MyException e)
            {
                if (DialogResult.Yes == MessageBox.Show(e.Message + "\nВосстановить исходник?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error))
                    RecoveryFile(this, new EventArgs());
                else
                    Close();
            }
            catch (Exception e)
            {
                if (DialogResult.Yes == MessageBox.Show(e.Message + "\nВосстановить исходник?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error))
                    RecoveryFile(this, new EventArgs());
                else
                    Close();
            }
        }
        /// <summary>
        /// Метод проверяющий параметры персонажей в файле.
        /// </summary>
        /// <param name="info"></param>
        /// <returns>True если информация о герое корректная,иначе False</returns>
        bool CheckInformation(string info)
        {
            // Проверка количества параметров у героя.
            string[] infoHero = info.Split(';');
            if (infoHero.Length != 9)
                return false;
            // Проверка инт характеристик.
            for (int i = 1; i < 8; i++)
            {
                if (i != 6)
                {
                    int parametr = 0;
                    if (!(int.TryParse(infoHero[i], out parametr) && parametr >= 0))
                        return false;
                }
            }
            // Проверка дабл характеристик.
            for (int i = 6; i < 9; i++)
            {
                if (i != 7)
                {
                    double parametr = 0;
                    if (!(double.TryParse(infoHero[i], out parametr) && parametr >= 0))
                        return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Подгрузка музыкальных файлов для программы.
        /// </summary>
        private void Resourses()
        {
            try
            {
                // подгрузка музыки для фона и т д.
                playlist = File.ReadAllLines("..\\..\\..\\NameOfMusic.txt");
                for (int i = 0; i < playlist.Length; i++)
                    if (!File.Exists(playlist[i]))
                        File.Copy(pathToSounds + playlist[i], playlist[i]);
                // Подгрузка озвучки героев.
                for (int i = 0; i < informationUnits.Count; i++)
                    if ((File.Exists(pathToSounds + informationUnits[i][0] + ".mp3")) && !(File.Exists(informationUnits[i][0] + ".mp3")))
                        File.Copy(pathToSounds + informationUnits[i][0] + ".mp3", informationUnits[i][0] + ".mp3");
            }
            catch (Exception)
            {
                MessageBox.Show("При подгрузке музкальных файлов произошла ошибка", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        /// <summary>
        /// Метод позволяющий двигать форму.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - lastPoint.X;
                this.Top += e.Y - lastPoint.Y;
            }
        }
        /// <summary>
        /// Метод позволяющий двигать форму.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            lastPoint = new Point(e.X, e.Y);
        }
        /// <summary>
        /// Событие-закрытие формы.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                //Очистка всех подгруженных звуковых файлов.
                for (int i = 0; i < playlist.Length; i++)
                    File.Delete(playlist[i]);
                // Отчистка озвучек героев.
                for (int i = 0; i < informationUnits.Count; i++)
                    if (File.Exists(informationUnits[i][0] + ".mp3"))
                        File.Delete(informationUnits[i][0] + ".mp3");
            }
            catch (Exception)
            {
                MessageBox.Show("При Удалении музыкальных файлов произошла ошибка", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        /// <summary>
        /// Событие-Активация формы.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Activated(object sender, EventArgs e)
        {
            // запуск фоновой музыки.
            mediaPlayer.controls.play();
            media_2.controls.play();
            repeatPlayer.controls.play();
        }
        /// <summary>
        /// Событие-Деактивация формы.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Deactivate(object sender, EventArgs e)
        {  //Пауза фоновой музыки
            mediaPlayer.controls.pause();
            media_2.controls.pause();
            repeatPlayer.controls.pause();
        }
        /// <summary>
        /// Кнопка выключения\включения звука фона
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClickForButtonVolume(object sender, EventArgs e)
        {
            // Если кнопка не была нажата пользователем ранее,выключение звука.
            // Иначе включение звука.
            try
            {
                if (VolumeButtonIsActivited)
                {
                    // Подгрузка картинки
                    volumeButton.BackgroundImage = Image.FromFile(pathToPicture + "mute.png");
                    // Звук всех плееров = 0.
                    mediaPlayer.settings.volume = 0;
                    media_2.settings.volume = 0;
                    repeatPlayer.settings.volume = 0;
                    VolumeButtonIsActivited = false;
                }
                else
                {
                    volumeButton.BackgroundImage = Image.FromFile(pathToPicture + "speaker.png");
                    mediaPlayer.settings.volume = 100;
                    media_2.settings.volume = 30;
                    repeatPlayer.settings.volume = countForMain;
                    VolumeButtonIsActivited = true;
                }
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Картинки для элементов управления были повреждены", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Картинки для элементов управления не были найдены", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Кнопка поиска игры.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchGameButtonClick(object sender, EventArgs e)
        {
            // Включение поиска игры.
            timeBeforeStartGame.Visible = true;
            timerForSomeThings.Enabled = true;
            // Удаление ненужных элементов управления.
            Controls.Remove(StartGameButton);
            Controls.Remove(settingButton);
            Controls.Remove(panelSetting);
            Controls.Remove(PanelTable);
        }
        /// <summary>
        /// Таймер -"Repeater"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerForMainAudio(object sender, EventArgs e)
        {
            if ("Остановлено" == repeatPlayer.status.ToString() || "Stopped" == repeatPlayer.status.ToString())
                repeatPlayer.controls.play();
        }
        /// <summary>
        /// Кнопка играть
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AcceptButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Запуск загрузочного экрана, время ожидание такое же, как и поиск игры.
                Controls.Remove(panelStart);
                Controls.Remove(GameAccept);
                pictureBox1.Visible = true;
                pictureBox1.BackgroundImage = Image.FromFile(pathToPicture + "Voide.jpg");
                CreateButtonsForHeroes();
                type = "ListForLoadingGame";
                // Генерация времени ожидания.
                gameTime = rnd.Next(4, 8);
                timerForSomeThings.Enabled = true;
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Картинки для элементов управления были повреждены", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Картинки для элементов управления не были найдены", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Таймер, работающий в различных направлениях,в зависимости от указанного типа.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerOfSomeThings(object sender, EventArgs e)
        {
            // Перевод секунд в минуты.
            if (second == 60) { minute += 1; second = 0; }
            //  Тип таймера для подсчета золота и времени.
            if (type == "GoldAndTime")
                GoldAndTime();
            else
            {
                // Время поиска игры.
                timeBeforeStartGame.Text = $"Поиск Игры {minute.ToString("0#")}:{second.ToString("0#")}";
                // Срабатывание после равенства установленного времени, и посчитанного.
                if (second == gameTime)
                {
                    // Тип таймера для команд,которые примеются после нажатия AcceptButton.
                    if (type == "ListForGame")
                    {
                        ListMethodsForGame();
                    }
                    else
                    {
                        // Тип таймера для команд,которые примеются после загрузочного Экрана.
                        if (type == "ListForLoadingGame")
                            LoadingGame();
                    }
                }
            }
            second += 1;
        }
        /// <summary>
        /// Метод,проверяющий корректность введенных данных в таблицу
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            // Инфорцая об ошибках, при вводе некорректных значений в ячейки = null
            dataAboutHeroes.Rows[e.RowIndex].ErrorText = null;
            // Проверка столбца с именами персонажей.
            if (e.ColumnIndex == 0)
            {
                // Имя не может быть пустой строкой.
                if (e.FormattedValue.ToString() == "")
                {
                    e.Cancel = true;
                    dataAboutHeroes.Rows[e.RowIndex].ErrorText = "the name of hero cann't be a empty";
                }
                // Иначе запись нового имени в лист с информацией о всех героях.
                informationUnits[e.RowIndex][e.ColumnIndex] = e.FormattedValue.ToString();
                return;
            }
            // Проверка столбцов с INT32 значениями.
            if ((e.ColumnIndex >= 1 && e.ColumnIndex <= 5) || (e.ColumnIndex == 7))
            {
                int newInteger;
                // Если значение INT32 и положительное,то запись в лист с информацией о героях.
                if (!int.TryParse(e.FormattedValue.ToString(),
                out newInteger) || newInteger < 0)
                {
                    e.Cancel = true;
                    dataAboutHeroes.Rows[e.RowIndex].ErrorText = "the value must be a non-negative integer";
                }
                informationUnits[e.RowIndex][e.ColumnIndex] = e.FormattedValue.ToString();
                return;
            }
            // Проверка столбцов с Double значениями.
            if ((e.ColumnIndex == 6) || (e.ColumnIndex == 8))
            {
                double newDouble;
                // Если значение помещается в Double и оно положительное,то запись в лист с информацией о героях.
                if (!double.TryParse(e.FormattedValue.ToString(),
               out newDouble) || newDouble < 0)
                {
                    e.Cancel = true;
                    dataAboutHeroes.Rows[e.RowIndex].ErrorText = "the value must be a non-negative double";
                }
                informationUnits[e.RowIndex][e.ColumnIndex] = e.FormattedValue.ToString();
                return;
            }
        }
        /// <summary>
        /// Список команд,которые применяются после загрузочного экрана.
        /// </summary>
        private void LoadingGame()
        {
            try
            {
                Controls.Remove(pictureBox1);
                // Картинка с выбором героев.
                BackgroundImage = Image.FromFile(pathToPicture + "MenuWithHeroes.jpg");
                // Отключение таймера считающего время поиска игры.
                timerForSomeThings.Enabled = false;
                // Музыка выбора героя , на репите.(обновление секунда.)
                mediaPlayer.URL = "SelectHeroes.mp3";
                repeatTimer.Interval = 1000;
                repeatTimer.Enabled = true;
                timerForSomeThings.Enabled = false;
                minute = second = 0;
                Pick.Visible = true;
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Картинки для элементов управления были повреждены", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Картинки для элементов управления не были найдены", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Событие клик кнопки-"Настройки".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingButtonClick(object sender, EventArgs e)
        {
            // Если кнопка не включалась.
            if (settingButtonIsActivited)
            {
                //Настройка панели-"настройки".
                panelSetting.Visible = true;
                settingButton.BackColor = PictureInSetting.BackColor;
                volumeButton.BackColor = PictureInSetting.BackColor;
                panelSetting.Controls.Add(settingButton);
                panelSetting.Controls.Add(volumeButton);
                panelSetting.Controls.SetChildIndex(settingButton, 0);
                panelSetting.Controls.SetChildIndex(volumeButton, 1);
                settingButtonIsActivited = false;
            }
            // Если пользователь уже нажимал на кнопку.
            else
            {
                // Выключение  панели-"настройки".
                panelSetting.Visible = false;
                settingButton.BackColor = Color.Transparent;
                volumeButton.BackColor = Color.Transparent;
                Controls.Add(settingButton);
                Controls.Add(volumeButton);
                settingButtonIsActivited = true;
            }
        }
        /// <summary>
        /// Запуск игры с XML
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartGameFromXML(object sender, EventArgs e)
        {
            // Получение информации из XML
            string[][] infoHeroes = ReadXML("Heroes.xml");
            // Если информация есть,то начинаем игру.
            if (infoHeroes != null)
            {
                // Создание класса gameLogic
                game = new GameLogic(infoHeroes[0], infoHeroes[1]);
                // Запись секунд боя
                second = int.Parse(infoHeroes[2][0]);
                // Запись минут боя
                minute = int.Parse(infoHeroes[2][1]);
                // Запись золота боя
                countGold = int.Parse(infoHeroes[2][2]);
                // Выполнения настройки игры, перед боем.
                StartGameBattle();
            }
        }
        /// <summary>
        /// Таймер отвечающий за создание класса GameLogic После озвучки выбора героя.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerForEndheroes_Tick(object sender, EventArgs e)
        {
            if ("Остановлено" == repeatPlayer.status.ToString() || "Stopped" == repeatPlayer.status.ToString() || flagToStartBattle)
            {
                // Создание индекса для бота на 1 меньше максимального списка героя.
                int indexForBot = rnd.Next(0, informationUnits.Count - 1);
                // Если выпал индекс героя уже выбранного,то прибавляем 1 к индексу бота.
                if (indexForBot == index)
                    indexForBot++;
                // Создание класса GameLogic.
                game = new GameLogic(informationUnits[index], informationUnits[indexForBot]);
                // Настройка элементов,перед началом Battle.
                StartGameBattle();
            }
        }
        /// <summary>
        /// Метод выполняющий настройку картинок\полей\элементов управления,перед началом Battle.
        /// </summary>
        private void StartGameBattle()
        {
            try
            {
                // Выключение таймера,рассчитынного на определение конца озвучки героя. 
                timerForEndSoundsHeroes.Enabled = false;
                // Добавления кнопок "Громкость", "Выход"
                panelBattle.Controls.Add(Exit);
                panelBattle.Controls.Add(volumeButton);
                Controls?.Remove(panelSetting);
                volumeButton.BackColor = Color.Transparent;
                // Раскрытие панели для батла.
                panelBattle.Visible = true;
                // Загрузка картинки для героев
                // Иначе загрузка картинки,неизвестного героя.
                if (File.Exists(pathToPicture + game.Hero.name + ".png"))
                    pictureForHero.BackgroundImage = Image.FromFile(pathToPicture + game.Hero.name + ".png");
                else
                    pictureForHero.BackgroundImage = Image.FromFile(pathToPicture + "NewHeroes.jpg");

                if (File.Exists(pathToPicture + game.bot.name + ".png"))
                    pictureForBot.BackgroundImage = Image.FromFile(pathToPicture + game.bot.name + ".png");
                else
                    pictureForBot.BackgroundImage = Image.FromFile(pathToPicture + "NewHeroes.jpg");

                // Название героев
                heroName.Text = game.Hero.name;
                botName.Text = game.bot.name;

                // Кол-во хп
                barHeroHP.Value = (int)game.Hero.HealtNow();
                barBotHP.Value = (int)game.bot.HealtNow();
                // Кол-во хп запись в строке.
                heroHP.Text = game.Hero.TostringHealth();
                botHP.Text = game.bot.TostringHealth();

                // Подключение музыкального сопровождения.
                mediaPlayer.controls.stop();
                media_2.URL = "BattleMusic.mp3";
                media_2.controls.stop();
                repeatPlayer = media_2;
                countForMain = 15;
                if (VolumeButtonIsActivited)
                    repeatPlayer.settings.volume = 15;
                repeatPlayer.controls.play();

                repeatTimer.Interval = 1000;
                repeatTimer.Enabled = true;

                mediaPlayer.URL = "";
                // Тип для тамера,который будет считать золото и время батла
                type = "GoldAndTime";
                timerForSomeThings.Enabled = true;
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Картинки для элементов управления были повреждены", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Картинки для элементов управления не были найдены", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Команды для таймера посчета золота и времени битвы.
        /// </summary>
        private void GoldAndTime()
        {
            // Подсчет времени битвы
            timeForBattle.Text = $"{minute.ToString("0#")}:{second.ToString("0#")}";
            // Проверка ,что золота не может быть больше 9999
            if (countGold < 9999)
                countGold++;
            // Подсчет золота.
            countOfGold.Text = $"{countGold}";
        }
        /// <summary>
        /// Кнопка-"Информация об игре".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InfoButton(object sender, EventArgs e)
        {
            // Информация для пользователя.
            string info =
                "Игра Дота 2" +
                "\nПравила Игры:" +
                "\nВыиграет тот, у кого остался в живых персонаж" +
                "\nВсего есть 3 варианта хода" +
                "\nБежать Защищаться Атаковать" +
                "\nВам предстоит выбрать героя и сражаться с ботом" +
                "\n\nТак же у вас есть возможность изменять показатели героев(В настройках)" +
                "\nВы можите запустить Игру с последнего сохранения(В настройках)" +
                "\nВы можите Восстановить исходник с героями,в случае его поломки(В настройках)" +
                "\nВы играете за Силу Света" +
                "\nПриятной игры!";
            MessageBox.Show(info, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        /// <summary>
        /// Метод,отрисовывающий HP героев 
        /// и опредеяющий,стоит ли закончить игру
        /// </summary>
        private void DrawHPandEndGame()
        {
            // Кол-во хп в прогресс баре.
            barHeroHP.Value = (int)game.Hero.HealtNow();
            barBotHP.Value = (int)game.bot.HealtNow();
            // Кол-во хп героев в ToString().
            heroHP.Text = game.Hero.TostringHealth();
            botHP.Text = game.bot.TostringHealth();
            // Обнуление лейбла c информацией о раунде.
            result.Text = "";
            // Запись в лейбел информации о раунде.
            result.Text += game.ToString() + "\n";
            // Возврат ссылки на героя,который погиб.
            // Либо пустой ссылки, что означает , что на данный ход никто не погиб.
            object person = game.Hero.checkHealth(game.bot);
            // Проверка,надо ли закрывать игру.
            if (person != null)
                CloseBattle((Unit)person);
            // Проверка на то,что ход,который будет сохранен в XML, не последний.
            if (person == null)
                CreateXML("Heroes.xml", game.Hero, game.bot);
        }
        /// <summary>
        /// Метод,выполняющийся,по окончании игры
        /// </summary>
        /// <param name="dead">Ссылка на героя,который погиб в схватке</param>
        private void CloseBattle(Unit dead)
        {
            // Выключение RepeatPlayer
            repeatTimer.Enabled = false;
            repeatPlayer.controls.stop();

            // Имя героя,который выграл схватку.
            string name;
            // Определение,кто выйграл в битве.
            if (dead.Equals(game.bot))
            {
                name = $"{game.Hero.name}";
                numberOfEnemyKills.Text = "1";
                mediaPlayer.URL = "EndBattle.mp3";
            }
            else
            {
                name = $"{game.bot.name}";
                numberOfFriendsKills.Text = "1";
                mediaPlayer.URL = "EnemyKillYou.mp3";
            }
            // Вывод информации о победителе.
            MessageBox.Show($"{name} Одолел опонента!", "EndGame", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // Вывод информации о рестарте приложение.
            if (DialogResult.Yes == MessageBox.Show("Хочешь еще сыграть?)", "Restart", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                // нажали ДА.
                // Рестарт формы.
                Program.Restart = true;
                Close();
            }
            else
            {
                // нажали НЕТ.
                // Закрытие формы.
                Close();
            }
        }
        /// <summary>
        /// Кнопка-Атаковать
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AttackButton(object sender, EventArgs e)
        {
            // Вызов метода Battle,с параметром "Атаковать".
            game.Battle(Type.Attack, game.Hero, game.bot);
            // Начисление золота,за ход.
            countGold += 500;
            // Проверка,что золота не больше 9999.
            if (countGold > 9999)
                countGold = 9999;
            // Проигрывание мелодии.
            mediaPlayer.URL = "papich" + rnd.Next(0, 3) + ".mp3";
            // Вызов метода,занимающийся отрисовкой HP.
            DrawHPandEndGame();
        }
        /// <summary>
        /// Кнопка-Защищаться
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DefendButton(object sender, EventArgs e)
        {
            // Вызов метода Battle,с параметром "Защита".
            game.Battle(Type.Defend, game.Hero, game.bot);
            // Начисление золота,за ход.
            countGold += 375;
            // Проверка,что золота не больше 9999.
            if (countGold > 9999)
                countGold = 9999;
            // Проигрывание мелодии.
            mediaPlayer.URL = "papich" + rnd.Next(3, 6) + ".mp3";
            // Вызов метода,занимающийся отрисовкой HP.
            DrawHPandEndGame();
        }
        /// <summary>
        /// Кнопка-Скрыться
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EscapeButton(object sender, EventArgs e)
        {
            // Вызов метода Battle,с параметром "Бег с поля боя".
            game.Battle(Type.Escape, game.Hero, game.bot);
            // Начисление золота,за ход.
            countGold += 250;
            // Проверка,что золота не больше 9999.
            if (countGold > 9999)
                countGold = 9999;
            // Проигрывание мелодии.
            mediaPlayer.URL = "papich" + rnd.Next(6, 8) + ".mp3";
            // Вызов метода,занимающийся отрисовкой HP.
            DrawHPandEndGame();
        }
        /// <summary>
        /// Таймер отвечающий за именение выдвижной панели 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerToGoPanel_Tick(object sender, EventArgs e)
        {
            try
            {
                // Если стрелочка не была ранее нажата.
                if (ArrowIsClose)
                {
                    // Изменени положения панели на 3 пикселя каждые 16 милисекнд.
                    if (PanelMethodsForBattle.Location.Y != 288)
                    {
                        PanelMethodsForBattle.Location = new Point(PanelMethodsForBattle.Location.X, PanelMethodsForBattle.Location.Y + 3);
                        return;
                    }
                    // Подгрузка картинки и изменение положения  стрелочки.
                    ArrowIsClose = false;
                    Arrow.BackgroundImage = Image.FromFile(pathToPicture + "Up.png");
                }
                else
                {
                    if (PanelMethodsForBattle.Location.Y != 138)
                    {
                        PanelMethodsForBattle.Location = new Point(PanelMethodsForBattle.Location.X, PanelMethodsForBattle.Location.Y - 3);
                        return;
                    }
                    ArrowIsClose = true;
                    Arrow.BackgroundImage = Image.FromFile(pathToPicture + "Down.png");
                }
                // Выкключение таймера
                goingPanelInBattle.Enabled = false;
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Картинки для элементов управления были повреждены", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Картинки для элементов управления не были найдены", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Обработка кнопок для героев на стадии выбора.
        /// </summary>
        private void CreateButtonsForHeroes()
        {
            try
            {
                for (int i = 0; i < informationUnits.Count; i++)
                {
                    //Задание размеров и нахождения кнопки
                    //Костыль,для связки кнопки и индекса героя.
                    hero[i].Name = i.ToString();
                    hero[i].Location = new Point(30 + (95 * i) % 1140, 60 + 60 * (i / 12));
                    hero[i].Size = new Size(90, 50);
                    hero[i].BackgroundImageLayout = ImageLayout.Stretch;
                    // Существует ли картинка ? (если имя героя было изменено)
                    if (File.Exists(pathToPicture + informationUnits[i][0] + ".png"))
                        hero[i].BackgroundImage = Image.FromFile(pathToPicture + informationUnits[i][0] + ".png");
                    else
                        hero[i].BackgroundImage = Image.FromFile(pathToPicture + "NewHeroes.jpg");
                    // Добавление кнопки на форму.
                    Controls.Add(hero[i]);
                    // Если убрал курсор мыши с кнопки, то обводка Invalidate(Удаляется)
                    hero[i].MouseLeave += (sender, args) =>
                    {
                        PictureBox picture = (PictureBox)sender;
                        Rectangle pictureBounds = picture.Bounds;
                        pictureBounds.X -= 2;
                        pictureBounds.Y -= 2;
                        pictureBounds.Width += 4;
                        pictureBounds.Height += 4;
                        Invalidate(pictureBounds);
                        picture.Cursor = Cursors.Default;
                    };
                    // При наведении курсором миши на героя, появляется обводка.
                    hero[i].MouseMove += (sender, args) =>
                    {
                        PictureBox picture = (PictureBox)sender;
                        picture.Cursor = Cursors.Hand;
                        using (Graphics g = CreateGraphics())
                        {
                            g.DrawRectangle(new Pen(Color.Blue, 2), picture.Bounds);
                        }
                    };
                    // Обработка события клик.
                    hero[i].Click += (sender, args) =>
                    {
                        Invalidate();
                        PictureBox picture = (PictureBox)sender;
                        // Костыль для доставания индекса героя для данной кнопки.
                        index = int.Parse(picture.Name);
                        // Фоновая музыка выбора героя.
                        if (File.Exists(informationUnits[index][0] + ".mp3"))
                            mediaPlayer.URL = informationUnits[index][0] + ".mp3";
                        else
                            mediaPlayer.URL = "SilverName.mp3";
                        repeatTimer.Enabled = false;
                        // Включение фоновой заставки и удаления кнопок выбора героев.
                        fonPicture.Visible = true;
                        for (int j = 0; j < informationUnits.Count; j++)
                            Controls.Remove(hero[j]);
                        // Удаление фона.
                        Controls.Remove(fonPicture);
                        Controls.Remove(Pick);
                        // Включения таймера, отслеживающего окночание озвучки героя.
                        timerForEndSoundsHeroes.Enabled = true;

                        //Создание кнопки,по нажатию которой начинается Battle
                        Button Warning = new Button();
                        Warning.Size = new Size(250, 100);
                        Warning.Anchor = System.Windows.Forms.AnchorStyles.None;
                        Warning.BackColor = System.Drawing.Color.Transparent;
                        Warning.Cursor = System.Windows.Forms.Cursors.Hand;
                        Warning.FlatAppearance.BorderColor = System.Drawing.Color.DimGray;
                        Warning.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                        Warning.FlatAppearance.MouseDownBackColor = Color.Transparent;
                        Warning.FlatAppearance.MouseOverBackColor = Color.Transparent;
                        Warning.ForeColor = System.Drawing.Color.White;
                        Warning.Location = new System.Drawing.Point(950, 500);
                        Warning.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
                        Warning.Name = "Warning";
                        Warning.Size = new System.Drawing.Size(200, 65);
                        Warning.Text = "Перейти к игре  (вручную)";
                        Warning.Font = ButtonAttack.Font;
                        Warning.Click += (sender1, args1) =>
                        {
                            flagToStartBattle = true;
                            Controls.Remove(Warning);
                        };
                        Controls.Add(Warning);
                    };
                }
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Картинки для элементов управления были повреждены", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Картинки для элементов управления не были найдены", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Восстановление исходника
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecoveryFile(object sender, EventArgs e)
        {
            try
            {
                // Удаление сломанного файла.
                if (File.Exists("..\\..\\..\\Dota2.csv"))
                    File.Delete("..\\..\\..\\Dota2.csv");
                // Восстановление сломанного файла.
                File.Copy("..\\..\\Resources\\Dota2.csv", "..\\..\\..\\Dota2.csv");
                File.SetAttributes("..\\..\\..\\Dota2.csv", FileAttributes.Normal);
                Program.Restart = true;
                Close();
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Восстановительный файл был поврежден", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Восстановительный файл был утерян", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
            catch (IOException)
            {
                MessageBox.Show("Восстановительный файл был поврежден\\утерян", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }
        /// <summary>
        /// Список команд,которые применяются после нахождения игры.
        /// </summary>
        private void ListMethodsForGame()
        {
            //Лабел с поиском игры.
            Controls.Remove(timeBeforeStartGame);
            panelStart.Visible = true;
            // Выключение всех таймеров.
            timerForSomeThings.Enabled = false;
            repeatTimer.Enabled = false;
            //Озвучка найденой игры.
            mediaPlayer.URL = "Main.mp3";
            minute = second = 0;

        }
        /// <summary>
        /// Создание XML файла,и дальнейшая запись в него информации и битве.
        /// </summary>
        /// <param name="filename">Название Файла</param>
        /// <param name="player">Ссылка на героя пользователя</param>
        /// <param name="bot">Ссылка на героя бота</param>
        private void CreateXML(string filename, Unit player, Unit bot)
        {
            // Если XML создавался ранее,удаляем
            if (File.Exists(filename))
                File.Delete(filename);
            // Найстройки XML файла.
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = false;
            settings.Encoding = Encoding.UTF8;
            //Массив с ключами для значений полей героев.
            string[] elementHead = (head + ";Health").Split(';');
            //Массивы с инфомацией о героях.
            string[] fullInfoPlayer = player.FullInfo();
            string[] fullInfoBot = bot.FullInfo();
            // Запись в XML файл.
            using (XmlWriter writer = XmlWriter.Create(filename, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Heroes");
                writer.WriteStartElement("Player");
                for (int i = 0; i < elementHead.Length; i++)
                    writer.WriteElementString(elementHead[i].ToString(), fullInfoPlayer[i].ToString());
                writer.WriteEndElement();
                writer.WriteStartElement("Bot");
                for (int i = 0; i < elementHead.Length; i++)
                    writer.WriteElementString(elementHead[i].ToString(), fullInfoBot[i].ToString());
                writer.WriteEndElement();
                writer.WriteStartElement("Atributes");
                writer.WriteElementString("TimeButtleSecond", $"{second}");
                writer.WriteElementString("TimeButtleMinute", $"{minute}");
                writer.WriteElementString("Gold", $"{countGold}");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.Close();
            }
        }
        /// <summary>
        /// Считывание информации о героях для битвы из XML
        /// </summary>
        /// <param name="filename">Имя файла XML</param>
        /// <returns>Массив с информацией для битвы</returns>
        private string[][] ReadXML(string filename)
        {
            try
            {
                // Загрузка XML
                XmlDocument doc = new XmlDocument();
                doc.Load(filename);
                int count = 0;
                // получим корневой элемент
                XmlElement root = doc.DocumentElement;
                // Получение ключей.
                string[] elementHead = (head + ";Health").Split(';');
                string[] elementForButtle = "TimeButtleSecond;TimeButtleMinute;Gold".Split(';');
                string[][] infoHeroes = new string[3][];
                foreach (XmlNode xnode in root)
                {
                    // Если узел не Atributes,то считываем героев.
                    // Иначе считываем атрибуты битвы.
                    if (xnode.Name != "Atributes")
                    {
                        string[] infoHero = new string[elementHead.Length];
                        for (int i = 0; i < elementHead.Length; i++)
                            infoHero[i] = xnode.SelectSingleNode(elementHead[i])?.InnerText;
                        infoHeroes[count++] = infoHero;
                    }
                    else
                    {
                        string[] infoButtle = new string[3];
                        for (int i = 0; i < 3; i++)
                            infoButtle[i] = xnode.SelectSingleNode(elementForButtle[i])?.InnerText;
                        infoHeroes[count] = infoButtle;
                    }
                }
                return infoHeroes;
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("XML файл не найден. ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (XmlException)
            {
                MessageBox.Show("При работе с XML произошла ошибка. ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }
        /// <summary>
        /// Метод для правильной работы атрибутов.
        /// </summary>
        /// <param name="check">Комбобокс атрибута,который проверяется</param>
        /// <param name="number">номер комбобокса атрибута</param>
        /// <param name="otherAtribute">Другой комбобокс с атрибутов</param>
        /// <param name="otherAtribute2">Третий комбобокс с атрибутом</param>
        /// <param name="valueOfAtribute">Значение для атрибута</param>
        private void MethodForAtributes(ComboBox check, int number, ComboBox otherAtribute, ComboBox otherAtribute2, TextBox valueOfAtribute)
        {
            // Проверка равен ли 2 атрибут None
            if (check.SelectedItem?.ToString() != "None")
            {
                // Проверка равен ли атрибут для сортировки, другим атрибутам, в дугих ячейках.
                if (check.SelectedItem?.ToString() == otherAtribute.SelectedItem?.ToString() ||
                check.SelectedItem?.ToString() == otherAtribute2.SelectedItem?.ToString())
                {
                    // Если да, то делаем его None
                    // И вычключаем поле с вводом значения.
                    check.SelectedItem = "None";
                    valueOfAtribute.Text = "";
                    valueOfAtribute.Visible = false;
                }
                else
                {
                    // Если нет, то записываем выбранный параметр в массив.
                    ChosenParametrs[number] = check.SelectedItem.ToString();
                    valueOfAtribute.Text = "";
                    valueOfAtribute.Visible = true;
                }
            }
            else
            {
                valueOfAtribute.Text = "";
                valueOfAtribute.Visible = false;
            }

        }
        /// <summary>
        /// КомбоБокс с выбором атрибута для сортировки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FirstAtributeSelected(object sender, EventArgs e)
        {
            MethodForAtributes(FirstAttribute, 0, SecondAttribute, ThirdAttribute, FirstValue);
        }
        /// <summary>
        /// КомбоБокс с выбором атрибута для сортировки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SecondAtributeSelected(object sender, EventArgs e)
        {
            MethodForAtributes(SecondAttribute, 1, FirstAttribute, ThirdAttribute, SecondValue);
        }
        /// <summary>
        /// КомбоБокс с выбором атрибута для сортировки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThirdAtributeSelected(object sender, EventArgs e)
        {
            MethodForAtributes(ThirdAttribute, 2, SecondAttribute, FirstAttribute, ThirdValue);
        }
        /// <summary>
        /// Метод,определяющий индекс шапки.
        /// </summary>
        /// <param name="str">Слово из шапки</param>
        /// <returns>индекс,если нет такого слова в шапке -1</returns>
        private int IndexElementInHead(string str)
        {
            string[] splitHead = head.Split(';');
            for (int i = 0; i < splitHead.Length; i++)
            {
                if (str == splitHead[i])
                    return i;
            }
            return -1;
        }
        /// <summary>
        /// Сортировка таблицы по атрибутам
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowButton(object sender, EventArgs e)
        {
            // ЗАпись индексов выбранных пользователем атрибутов.
            int firstIndex = IndexElementInHead(ChosenParametrs[0]),
                secondIndex = IndexElementInHead(ChosenParametrs[1]),
                thirdIndex = IndexElementInHead(ChosenParametrs[2]);
            // Цикл проверяющий значения выбранные пользователем, у каждого героя.
            for (int i = 0; i < informationUnits.Count; i++)
            {
                // Проверка на то что в текстбоксе есть значение, а индекс атрибута не -1.
                if (firstIndex != -1 && (FirstValue.Text != "" && FirstValue.Text != null))
                    // Если Значение выбранное пользователем, не равно значению записанному в ячейку
                    // То скрытие ячейки.
                    if (informationUnits[i][firstIndex] != FirstValue.Text)
                    {
                        dataAboutHeroes.Rows[i].Visible = false;
                        continue;
                    }
                // Проверка 2 атрибута.
                if (secondIndex != -1 && (SecondValue.Text != "" && SecondValue.Text != null))
                    if (informationUnits[i][secondIndex] != SecondValue.Text)
                    {
                        dataAboutHeroes.Rows[i].Visible = false;
                        continue;
                    }
                // Проверка 3 атрибута.
                if (thirdIndex != -1 && (ThirdValue.Text != "" && ThirdValue.Text != null))
                    if (informationUnits[i][thirdIndex] != ThirdValue.Text)
                        dataAboutHeroes.Rows[i].Visible = false;
            }
        }
        /// <summary>
        /// Ресет таблицы.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetButton(object sender, EventArgs e)
        {
            for (int i = 0; i < informationUnits.Count; i++)
                dataAboutHeroes.Rows[i].Visible = true;
        }
        /// <summary>
        /// Кнопка в настройках - "Setting Heroes"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingHeroesClick(object sender, EventArgs e)
        {   //Включение таблицы.
            PanelTable.Visible = true;
            // Запуск события клик кнопки настроек,чтобы скрыть панель с настройками.
            SettingButtonClick(sender, e);
        }

        private void panelBattle_Paint(object sender, PaintEventArgs e)
        {

        }

        /// <summary>
        /// Закрытие панели с таблицей
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButtonTable(object sender, EventArgs e)
        {
            PanelTable.Visible = false;
        }
    }
}
