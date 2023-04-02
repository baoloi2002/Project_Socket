using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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

            _tcpListener = new TcpListener(IPAddress.Any, Constants.PORT);
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(OnTCPConnected, null);

            Console.WriteLine($"Server started on port {Constants.PORT}.");
        }

        private static void OnTCPConnected(IAsyncResult result)
        {
            TcpClient client = _tcpListener.EndAcceptTcpClient(result);
            _tcpListener.BeginAcceptTcpClient(OnTCPConnected, null);
            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= Constants.MAX_PLAYER; i++)
            {
                if (clients[i].TCP.socket == null)
                {
                    clients[i].TCP.Connect(client);
                    return;
                }
            }

            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server full!");
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

            Console.WriteLine("Initialized packets.");
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
