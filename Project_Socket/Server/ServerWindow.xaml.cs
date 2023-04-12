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

        private List<Player> people;
        public ServerWindow()
        {
            InitializeComponent();
            // Center the window on startup
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

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
            List<Player> sortedList = GameManager.GetAllPlayers();
            sortedList.Sort((u, v) =>
            {
                if (u.iskilled == v.iskilled)
                {
                    return u.Order.CompareTo(v.Order);                    
                }

                return u.iskilled.CompareTo(v.iskilled);
            });
            // Set the sorted list as the ItemsSource of the ListView
            LOG.log = "";
            foreach(Player p in sortedList)
            {
                LOG.log += p.Name + " " + p.Order.ToString() + " " + p.iskilled.ToString() + "\n";
            }
            lstUsersView.ItemsSource = sortedList;
        }

        private void ShowQuiz(QuizQuestion quiz)
        {
            tbQuestion.Text = quiz.question;
            if (quiz.choices != null)
            {
                for (int i = 0; i < quiz.choices.Length; i++)
                {
                    tbQuestion.Text += "\n" + quiz.choices[i].ToString();
                }
            }
            tbQuestion.Text += "\nAnswer: " + (char)(quiz.answer + 'A');
        }

        private void UpdateUI()
        {
            SortList();
            if (GameManager.IsTimerStarted)
            {
                int tmp = (int)GameManager._startTime;
                tbTimer.Text = tmp.ToString();
            }
            else
            if (GameManager.IsGameStarted)
            {
                if (MatchManager.curQuiz != -1 && MatchManager.quizList != null && MatchManager.curQuiz < MatchManager.quizList.Length )
                {
                    ShowQuiz(MatchManager.quizList[MatchManager.curQuiz]);
                }
                int tmp = (int)MatchManager._waitTimer;
                tbTimer.Text = tmp.ToString();
            }           
            tbLog.Text = LOG.log;
        }
    }
}
