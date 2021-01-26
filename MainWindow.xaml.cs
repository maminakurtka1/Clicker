using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading; // используем это пространство имен для работы с таймером

namespace Clicker
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer gameTimer = new DispatcherTimer(); // создаем DispatcherTimer и назовем его gameTimer
        List<Ellipse> removeThis = new List<Ellipse>(); // создаем список removeThis, он будет использован для удаления кружков на которые мы нажимаем

        //нужные нам для игры переменные
        int spawnRate = 60; // скорость спавна кружков по умолчанию
        int currentRate; // текущая скорость спавна
        int lastScore = 0; // последний результат
        int health = 350; // максимальное значение здоровья игрока
        int posX, posY; // Х и У координаты кружков
        int score = 0; // текущий счет
        double growthRate = 0.6; // скорость роста кружков

        Random rand = new Random();

        // используем класс MediaPlayer для воспроизведения звука:

        MediaPlayer playClickSound = new MediaPlayer(); // в случае если мы кликаем на кружок
        MediaPlayer playerPopSound = new MediaPlayer(); // в случае если он лопает сам

        // используем Uri для поиска наших звуков среди файлов

        Uri ClickedSound;
        Uri PoppedSound;
        Brush brush;
        public MainWindow()
        {
            InitializeComponent();

            // прописываем условия для начала игры

            gameTimer.Tick += GameLoop;
            gameTimer.Interval = TimeSpan.FromMilliseconds(20); // задаем интервал для тика в 20 милисекунд
            gameTimer.Start();
            currentRate = spawnRate; // установка текущей скорости спавна

            // указываем путь к нашим звуковым файлам

            ClickedSound = new Uri("pack://siteoforigin:,,,/sound/clickedpop.mp3");
            PoppedSound = new Uri("pack://siteoforigin:,,,/sound/pop.mp3");

        }

        private void GameLoop(object sender, EventArgs e) 
        {
            txtScore.Content = "Score: " + score; // счетчик очков
            txtLastScore.Content = "Last Score: " + lastScore; // показываем последний счет

            // уменьшаем на 2 текущую скорость спавна
            currentRate -= 2;

            if (currentRate < 1)
            {
                currentRate = spawnRate;

                // генерим случайные значения Х и У для кружков
                posX = rand.Next(15, 700);
                posY = rand.Next(50, 350);

                // рандомим цвета кружков
                brush = new SolidColorBrush(Color.FromRgb((byte)rand.Next(1, 255), (byte)rand.Next(1, 255), (byte)rand.Next(1, 255)));

                // создаем новый кружок, у него есть хар-ки по умолчанию
                Ellipse circle = new Ellipse
                {
                    Tag = "circle",
                    Height = 10,
                    Width = 10,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Fill = brush
                };

                // размещаем созданный кружок с координатами Х и У зарандомленными заранее
                Canvas.SetLeft(circle, posX);
                Canvas.SetTop(circle, posY);
                // добавляем кружок на канвас
                MyCanvas.Children.Add(circle);
            }

            // этот форич необходим для увелечения кружков 

            foreach (var x in MyCanvas.Children.OfType<Ellipse>())
            {
                // увеличиваем размеры кружков

                x.Height += growthRate; 
                x.Width += growthRate; 
                x.RenderTransformOrigin = new Point(0.5, 0.5);

                if (x.Width > 70) //если размер кружка увеличивается до 70, то он лопается
                {
                    // кружок >70 попадает в список на удаление
                    removeThis.Add(x);
                    health -= 15; // за каждый пропущенный кружок теряется 15 хп
                    playerPopSound.Open(PoppedSound); // воспроизведение звуков появления и лопанья кружка
                    playerPopSound.Play();

                }

            } 

            if (health > 1)
            {
                // связываем прямоугольный индикатор с числовым значением
                healthBar.Width = health;
            }
            else
            {
                // вызываем конец игры в случае если здоровье меньше единицы
                GameOverFunction();
            }

            // удаляем кружки с поля
            foreach (Ellipse i in removeThis)
            {
                MyCanvas.Children.Remove(i); 
            }

            if (score > 5) // в зависимости от счета меняются значения скорости спавна и роста кружков
            {
                spawnRate = 25; 
            }
            if (score > 20)
            {
                spawnRate = 15;
                growthRate = 1.5; 
            }
        }

        private void ClickOnCanvas(object sender, MouseButtonEventArgs e)
        {
            // проверка на клик по кружку

            if (e.OriginalSource is Ellipse)
            {
                Ellipse circle = (Ellipse)e.OriginalSource;
                MyCanvas.Children.Remove(circle);
                score++;

                // воспроизводим звук клика по кружку
                playClickSound.Open(ClickedSound);
                playClickSound.Play();
            }
        }

        private void GameOverFunction()
        {
            gameTimer.Stop(); // останавливаем таймер

            // всплывающий мэсседжбокс
            MessageBox.Show("Game Over" + Environment.NewLine + "You Scored: " + score + Environment.NewLine + "Click Ok to play again!", "Result: ");

            // события после нажатия ОК
            foreach (var y in MyCanvas.Children.OfType<Ellipse>())
            {
                // удаляем все кружки с поля
                removeThis.Add(y);
            }
            foreach (Ellipse i in removeThis)
            {
                MyCanvas.Children.Remove(i);
            }

            // возвращаем все к дефолтным значениям
            growthRate = .6;
            spawnRate = 60;
            lastScore = score;
            score = 0;
            currentRate = 5;
            health = 350;
            removeThis.Clear();
            gameTimer.Start();


        }
    }
}