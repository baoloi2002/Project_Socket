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

        public ClientGame()
        {
            InitializeComponent();
        }

        private void Choice_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Choose the question

            if (question == null)
            {
                // Check if clicked button is the correct
                Button clickedButton = (Button)sender;
                switch (clickedButton.Tag)
                {
                    case "1":
                        {
                            MessageBox.Show("Hey hey");
                            break;
                        }
                }
            }
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Skip the question
            // Return dialog box
            MessageBox.Show("Hello World!");
        }
    }
}