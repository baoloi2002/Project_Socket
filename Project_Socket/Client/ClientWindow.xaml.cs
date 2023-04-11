using System;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using MaterialDesignThemes.Wpf;
using Project_Socket;
using Project_Socket.Server;
using System.Windows.Navigation;
using System.Timers;


namespace Project_Socket.Client
{
    public partial class ClientWindow : Window
    {
        // default
        private string serverIP = "127.0.0.1";
        private int serverPort = 1234;

        public static bool isAnnounce = false;
        public static System.Timers.Timer timer; // add a timer field

        public ClientWindow()
        {
            InitializeComponent();
            Client.Start();

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
            //if (Client.ID == -1)
            //{
            //    MessageBox.Show("No SLOT TO CONNECT");                
            //}
            //else
            //{
            string[] temp = serverAddress.Text.Split(':');
            serverIP = temp[0];
            serverPort = int.Parse(temp[1]);
                Client.Connect(serverIP, serverPort);
                Client.nickname = txtNickname.Text;
                Client.SendUsername();
            //}
        }
    }
}