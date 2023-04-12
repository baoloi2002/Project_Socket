using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Project_Socket.Server
{
    internal class Server
    {
        public static Dictionary<int, ClientItem> clients = new Dictionary<int, ClientItem>();
        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener _tcpListener;

        public static void Start()
        {
            InitializeServerData();

            //_tcpListener = new TcpListener(IPAddress.Any, Constants.PORT);
            _tcpListener = new TcpListener(IPAddress.Parse(Constants.SERVER_IP), Constants.PORT);
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(OnTCPConnected, null);
        }

        private static void OnTCPConnected(IAsyncResult result)
        {
            TcpClient client = _tcpListener.EndAcceptTcpClient(result);
            _tcpListener.BeginAcceptTcpClient(OnTCPConnected, null);
            for (int i = 1; i <= Constants.MAX_PLAYER; i++)
            {
                if (clients[i].TCP.socket == null)
                {
                    //MessageBox.Show(i.ToString());
                    clients[i].TCP.Connect(client);
                    return;
                }
            }
        }

        public static void HandlePacket(int id, int clientId, Packet packet) => packetHandlers[id](clientId, packet);

        private static void InitializeServerData()
        {
            for (int i = 1; i <= Constants.MAX_PLAYER; i++)
            {
                clients.Add(i, new ClientItem(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ClientPackets.ResendUsername, ServerHandler.ResendUsername },
            { (int)ClientPackets.GiveAnswer, ServerHandler.GiveAnswer }
        };

        }
        public static void AcceptPlayerIntoGame(int clientId, string username)
        {
            Player player = clients[clientId].ConstructPlayer(clientId, username);

            ServerSender.RegistrationSuccessful(clientId, clients[clientId].player);

            // The order should have changes, resend this to every client
            GameManager.ReorderPlayer();
            GameManager.HandleStartGame();
        }

        public static void RemovePlayerFromGame(int clientId)
        {

            foreach (ClientItem client in clients.Values)
            {
                if (client.ID == clientId)
                {
                    if (client.player != null)
                    {
                        client.player = null;
                        ServerSender.RemovePlayerFromGame(clientId);
                    }
                }
            }

            GameManager.ReorderPlayer();
            GameManager.HandleStartGame();
        }

        public static void DisconnectClient(int clientId)
        {
            clients[clientId].Disconnect();
        }

        public static void Stop()
        {
            _tcpListener.Stop();
        }
    }
}
