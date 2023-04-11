using MaterialDesignThemes.Wpf;
using Project_Socket.Server;
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

// TODO
// 1. Add Game Menu: Play, Quit
// 2. Play Window: Server IP, Port, Nickname, Connect
// 3. If click Connect -> Game Window (which is already done)
// 4. Win Window: Win, Quit

namespace Project_Socket.Client
{
    /// <summary>
    /// Interaction logic for ClientWindow.xaml
    /// </summary>
    public partial class ClientGame : Window
    {
        private QuizQuestion question;
        public static int clientAnswer = 4; // 4 is nothing, 0-3 answer, 5 is skip
        private bool gameEnd = false, isInGame;
        public static bool isSkip = false;
        public static bool isTurn = false;

        public static float _Timer = 1;
        private static DateTime _lastTick = DateTime.Now;

        public ClientGame()
        {
            InitializeComponent();
            question = new QuizQuestion();
            isInGame = true;
            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();
        }

        private void MainThread()
        {
            DateTime nextLoop = DateTime.Now;          

            while (isInGame)
            {
                while (nextLoop < DateTime.Now)
                {
                    ThreadManager.Update();
                    Dispatcher.Invoke(() =>
                    {
                        UpdatePlayerList();
                        UpdateQuizQuestion();
                        UpdateChoicesColor();
                        UpdateTurnDisplay();
                        UpdateTimer();
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

        // You win function
        private void YouWin()
        {
            QuestionBlock.Text = "You WIN!";

            Choice_1.Content = "";
            Choice_2.Content = "";
            Choice_3.Content = "";
            Choice_4.Content = "";
        }


        // This function gets called if player click a button
        private void UpdateChoicesColor()
        {
            if (Client.question != null)
            {
                switch (Client.question.answer)
                {
                    case 0:
                        {
                            Choice_1.Background = System.Windows.Media.Brushes.Green;
                            break;
                        }
                    case 1:
                        {
                            Choice_2.Background = System.Windows.Media.Brushes.Green;
                            break;
                        }
                    case 2:
                        {
                            Choice_3.Background = System.Windows.Media.Brushes.Green;
                            break;
                        }
                    case 3:
                        {
                            Choice_4.Background = System.Windows.Media.Brushes.Green;
                            break;
                        }
                }
            }

            // Paint red if client choose the wrong answer
            if (Client.question != null && !Client.question.isCorrect(clientAnswer))
            {
                switch (clientAnswer)
                {
                    case 0:
                        {
                            Choice_1.Background = System.Windows.Media.Brushes.Red;
                            break;
                        }
                    case 1:
                        {
                            Choice_2.Background = System.Windows.Media.Brushes.Red;
                            break;
                        }
                    case 2:
                        {
                            Choice_3.Background = System.Windows.Media.Brushes.Red;
                            break;
                        }
                    case 3:
                        {
                            Choice_4.Background = System.Windows.Media.Brushes.Red;
                            break;
                        }
                }

                // Player disqualified
                gameEnd = true;

            }
        }

        private void UpdatePlayerList()
        {
            // Sort the list
            if (Client.playerList.Count == 0)
            {
                return;
            }
            List<Player> sortedList = Client.playerList;
           
            sortedList.Sort((u, v) =>
            {
                if (u.iskilled == v.iskilled)
                {
                    return u.Order.CompareTo(v.Order);
                }

                return u.iskilled.CompareTo(v.iskilled);
            });
            lstUsersView.ItemsSource = sortedList;          
        }
        private void UpdateQuizQuestion()
        {
            // Update Count UI
            questionCount.Content = "Total Question: " + Client.currentNumberOfQuestion.ToString() + "/" + Client.NumberOfQuestion.ToString();

            // Update question
            if (Client.question == null) return;
            QuestionBlock.Text = Client.question.question;

            // Update choices
            Choice_1.Content = Client.question.choices[0];
            Choice_2.Content = Client.question.choices[1];
            Choice_3.Content = Client.question.choices[2];
            Choice_4.Content = Client.question.choices[3];
        }
        private void UpdateTurnDisplay()
        {
            if (isTurn)
            {
                turnAnnounce.Visibility = Visibility.Visible;

            }
            else
            {
                turnAnnounce.Visibility = Visibility.Hidden;

            }
        }
        
        private void timerCountdown(int time)
        {
            int timeleft = time - 1;
            leftTimer.Text = timeleft.ToString();
        }

        private void Choice_Click(object sender, RoutedEventArgs e)
        {

            if (!isTurn || clientAnswer !=4) return;

            Button clickedButton = (Button)sender;

            // Convert button name to integer, to send to client or compare with answer
            switch (clickedButton.Name)
            {
                case "Choice_1":
                    {
                        clientAnswer = 0;
                        break;
                    }
                case "Choice_2":
                    {
                        clientAnswer = 1;
                        break;
                    }
                case "Choice_3":
                    {
                        clientAnswer = 2;
                        break;
                    }
                case "Choice_4":
                    {
                        clientAnswer = 3;
                        break;
                    }
            }
            Client.SendAnswer(clientAnswer);

            // Change button color 
            UpdateChoicesColor();
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            if (isSkip || !isTurn) return;
            clientAnswer = 5;
            isSkip = true;
            isTurn = false;
            Client.SendSkip();

            // Change to 'not your turn'
            UpdateTurnDisplay();

        }

        private void UpdateTimer()
        {
            _Timer -= (float)(DateTime.Now - _lastTick).Milliseconds / 1000;
            if (_Timer < 0) _Timer = 0;
            _lastTick = DateTime.Now;
            int tmp = (int)_Timer;
            leftTimer.Text = tmp.ToString();
            if (_Timer > Constants.TIME_PER_ROUND - 1)
            {
                clientAnswer = 4;
                Choice_1.Background = System.Windows.Media.Brushes.RosyBrown;
                Choice_2.Background = System.Windows.Media.Brushes.RosyBrown;
                Choice_3.Background = System.Windows.Media.Brushes.RosyBrown;
                Choice_4.Background = System.Windows.Media.Brushes.RosyBrown;
            }
        }
    }
}