using MaterialDesignThemes.Wpf;
using SuperSimpleTcp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Project_Socket.Server
{

    public partial class ServerWindow : Window
    {
        private static bool _isServerRunning = false;

        private QuizQuestion[] quizList;
        private List<Player> people;
        public ServerWindow()
        {
            InitializeComponent();
            // Center the window on startup
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            quizList = LoadQuestions("QuizList.json");
            ShowQuiz(quizList[0]);

            // START GAME THREAD
            _isServerRunning = true;
            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            // START SERVER
            Server.Start();

        }
        // GAME LOOP IS HERE
        private void MainThread()
        {
            Console.WriteLine($"Main thread started. Running at {Constants.TICKS_PER_SEC} ticks per second.");
            DateTime nextLoop = DateTime.Now;

            while (_isServerRunning)
            {
                while (nextLoop < DateTime.Now)
                {
                    // If the time for the next loop is in the past, aka it's time to execute another tick
                    GameManager.Update(); // Execute game logic
                    Dispatcher.Invoke(() =>
                    {
                        UpdateUI();
                    });

                    nextLoop = nextLoop.AddMilliseconds(Constants.MS_PER_TICK); // Calculate at what point in time the next tick should be executed

                    if (nextLoop > DateTime.Now)
                    {
                        // If the execution time for the next tick is in the future, aka the server is NOT running behind
                        Thread.Sleep(nextLoop - DateTime.Now); // Let the thread sleep until it's needed again.
                    }
                }
            }
        }

        private void SortList()
        {
            // Sort the list
            List<Player> sortedList = people.OrderBy(p => p.Order).ToList();

            // Update the SortOrder property of each item
            for (int i = 0; i < sortedList.Count; i++)
            {
                sortedList[i].Order = i;
            }

            // Set the sorted list as the ItemsSource of the ListView
            lstUsersView.ItemsSource = sortedList;
        }


        private QuizQuestion[] LoadQuestions(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);
            QuizQuestion[] questions = JsonSerializer.Deserialize<QuizQuestion[]>(jsonString);
            Random random = new Random();
            for(int i = questions.Length - 1; i > 0; i--)
            {
                int j = random.Next(i+1);
                QuizQuestion tmp = questions[j];
                questions[j] = questions[i];
                questions[i] = tmp;
            }
            return questions;
        }

        private void ShowQuiz(QuizQuestion quiz)
        {
            tbQuestion.Text = quiz.question;
            if (quiz.choices != null)
            {
                for (int i=0; i<quiz.choices.Length; i++)
                {
                    tbQuestion.Text += "\n" + quiz.choices[i].ToString();
                }
            }
        }      

        private void UpdateUI()
        {
            lstUsersView.ItemsSource = GameManager.GetAllPlayers();
            if (GameManager.IsTimerStarted)
            {
                tbTimer.Text = GameManager._startTime.ToString();
            }
            else
            if (GameManager.IsGameStarted)
            {
                //tbTimer.Text = MatchManager._waitTimer.ToString();
            }

        }
    }
}
