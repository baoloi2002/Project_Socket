using System;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using MaterialDesignThemes.Wpf;
using Project_Socket;
using Project_Socket.Server;
using System.Windows.Navigation;
using System.Timers;

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
        private string serverIP = "127.0.0.1";
        private int serverPort = 1234;
        public static bool isAnnounce = false;
        private System.Timers.Timer timer; // add a timer field

        public ClientWindow()
        {
            InitializeComponent();
            Client.Start();
            Client.Connect(serverIP, serverPort);

            // initialize the timer with a 1-second interval
            timer = new System.Timers.Timer(100);
            timer.Elapsed += Timer_Elapsed; // register the event handler
            timer.Start(); // start the timer
        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // this code will be executed every 1 second
            // you can put your desired logic here
            // for example, updating a label with the current time
            Dispatcher.Invoke(() =>
            {
                ThreadManager.Update();
                if (isAnnounce)
                {
                    isAnnounce = false;
                    if (Client.isRegSuccess)
                    {
                        ClientGame clientGame = new ClientGame();
                        clientGame.Show();
                        this.Close();
                        timer.Stop();
                    }
                    else
                    {
                        MessageBox.Show("Username wrong");
                    }
                }
            });
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (Client.ID == -1)
            {
                MessageBox.Show("No SLOT TO CONNECT");                
            }
            else
            {
                Client.nickname = txtNickname.Text;
                Client.SendUsername();
            }
        }
    }
}