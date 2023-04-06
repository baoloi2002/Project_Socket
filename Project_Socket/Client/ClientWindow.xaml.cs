using System;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using Project_Socket;
using Project_Socket.Server;

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

        string serverIP = "127.0.0.1";
        int serverPort = 1234;
        string nickname = "Bob";

        public ClientWindow()
        {
            InitializeComponent();
            Client.Start();

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();
            Client.Connect(serverIP, serverPort);
        }

        private void MainThread()
        {
            DateTime nextLoop = DateTime.Now;

            while (nextLoop < DateTime.Now)
            {
                ThreadManager.Update();
                nextLoop = nextLoop.AddMilliseconds(Constants.MS_PER_TICK); // Calculate at what point in time the next tick should be executed

                if (nextLoop > DateTime.Now)
                {
                    // If the execution time for the next tick is in the future, aka the server is NOT running behind
                    Thread.Sleep(nextLoop - DateTime.Now); // Let the thread sleep until it's needed again.
                }
            }

        }
        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            Client.SendUsername(nickname);

        }
    }
}