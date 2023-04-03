using System;
using System.Windows;

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
    public partial class ClientWindow : Window
    {
        public ClientWindow()
        {
            InitializeComponent();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            //string[] userInput = txtServerAddress.Text.Split(':');
            //string serverIP = userInput[0];
            //int serverPort = int.Parse(userInput[1]);
            //string nickname = txtNickname.Text;

            string serverIP = "127.0.0.1";
            int serverPort = 6294;
            string nickname = "Bob";

            // Create a TCP/IP socket by using a client object
            Client client = new Client();
            client.ClientConnect(serverIP, serverPort);

            // Send the nickname to the server
            //try
            //{
            //    // Package and send to server
            //    Packet temp = new Packet();
            //    //temp.AddData(nickname);

            //    client.SendPacket(temp);
            //    // Check if the nickname is valid
            //    //if (client.ReceivePacket() == "1")
            //    //{
            //    //    // If the nickname is valid, go to the game page
            //    ClientGame clientGame = new ClientGame();
            //    this.Content = clientGame;
            //    //}
            //    //else
            //    //{
            //    //    // If the nickname is invalid, show the error message
            //    //    MessageBox.Show("Nickname is invalid!");
            //    //}
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
            
        }
    }
}