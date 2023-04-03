using System;
using System.Windows;
using Project_Socket;

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
        public Packet send, receive;
                
        public ClientWindow()
        {
            InitializeComponent();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            Client client = new Client();
            //string[] userInput = txtServerAddress.Text.Split(':');
            //string serverIP = userInput[0];
            //int serverPort = int.Parse(userInput[1]);
            //string nickname = txtNickname.Text;

            string serverIP = "127.0.0.1";
            int serverPort = 6294;
            string nickname = "Bob";

            // Create a TCP/IP socket by using a client object
            try
            {
                client.ClientConnect(serverIP, serverPort);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            // Receive WelcomePlayer
            try
            {
                using (Packet packet = client.receivePacket())
                {
                    int packetType = packet.ReadInt();
                    if (packetType == (int)ServerPackets.WelcomePlayer)
                    {
                        int id = packet.ReadInt();
                        MessageBox.Show("Welcome to the game!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //Send the nickname to the server
            try
            {
                using (Packet packet = new((int)ClientPackets.ResendUsername))
                {
                    packet.PutInt(1);
                    packet.PutString(nickname);
                }
                // Check if the nickname is valid
                
                // Receive the packet
                using (Packet packet = client.receivePacket())
                {
                    int packetType = packet.ReadInt();
                    if (packetType == (int)ServerPackets.RegistrationFailed)
                    {
                        int result = packet.ReadInt();
                        if (result == 1)
                        {
                            // If the nickname is valid, go to the game page
                            ClientGame clientGame = new ClientGame();
                            this.Content = clientGame;
                        }
                        else
                        {
                            // If the nickname is invalid, show the error message
                            MessageBox.Show("Nickname is invalid!");
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }
}