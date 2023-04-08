using System.Windows;
using System.Windows.Controls;

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
    public partial class ClientGame : UserControl
    {
        private QuizQuestion question;
        private int clientAnswer;
        private bool gameEnd = false, isTurn = true;

        public ClientGame()
        {
            InitializeComponent();
            question = new QuizQuestion();
        }

        private void waitForTurn()
        {
            // Do something to wait
            // If is turn, do update
            Update();
        }

        private void Update()
        {
            // Receive question from server and change window content
            Client.ReceiveQuizQuestion(ref question);

            QuestionBlock.Text = question.question;

            Choice_1.Content = question.choices[0];
            Choice_2.Content = question.choices[1];
            Choice_3.Content = question.choices[2];
            Choice_4.Content = question.choices[3];
        }

        private void Choice_Click(object sender, RoutedEventArgs e)
        {
            //if (question != null)
            //{
            // Check if clicked button is the correct
            Button clickedButton = (Button)sender;

            // Convert button name to integer, to send to client or compare with answer
            switch (clickedButton.Name)
            {
                case "Choice_1":
                    {
                        clientAnswer = 1;
                        break;
                    }
                case "Choice_2":
                    {
                        clientAnswer = 2;
                        break;
                    }
                case "Choice_3":
                    {
                        clientAnswer = 3;
                        break;
                    }
                case "Choice_4":
                    {
                        clientAnswer = 4;
                        break;
                    }
            }
            if (question.isCorrect(clientAnswer))
            {
                clickedButton.Background = System.Windows.Media.Brushes.Red;
                isTurn = false;

                // Disqualify (handled by server)
                Client.Disconnect();
                QuestionBlock.Visibility = Visibility.Collapsed;
            }
            switch (question.answer)
            {
                case 1:
                    Choice_1.Background = System.Windows.Media.Brushes.Red;
                    break;

                case 2:
                    Choice_2.Background = System.Windows.Media.Brushes.Green;
                    break;

                case 3:
                    Choice_3.Background = System.Windows.Media.Brushes.Green;
                    break;

                case 4:
                    Choice_4.Background = System.Windows.Media.Brushes.Green;
                    break;
            }

            Client.SendAnswer(clientAnswer);
            //}
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            isTurn = false;
            Client.SendSkip(1);
        }
    }
}