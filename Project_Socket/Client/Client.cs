using Project_Socket.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Project_Socket.Client
{
    internal class Client
    {
        private static TcpClient client;
        private static NetworkStream stream;
        private static byte[] buffer = new byte[1024];

        public static int ID;

        public delegate void PacketHandler(Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers;
        public static void HandlePacket(int id, Packet packet) => packetHandlers[id](packet);

        public static void Start()
        {
            packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.WelcomePlayer, Client.RecieveID },            
        };
        }

        public static void Connect(string server, int port)
        {
            try
            {
                client = new TcpClient(server, port);
                stream = client.GetStream();
                stream.BeginRead(buffer, 0, buffer.Length, OnReceivedData, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error connecting to server: " + e.Message);
            }
        }

        private static void OnReceivedData(IAsyncResult result)
        {
            try
            {
                int byteLength = stream.EndRead(result);
                if (byteLength <= 0)
                {
                    // Connection closed by server
                    Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(buffer, data, byteLength);

                // Handle the incoming data using the same packet format as the server
                HandleData(data);

                // Read the next packet
                stream.BeginRead(buffer, 0, buffer.Length, OnReceivedData, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error receiving data from server: " + e.Message);
                Disconnect();
            }
        }

        private static void HandleData(byte[] data)
        {
            // Parse the incoming data using the same packet format as the server
            int packetLength = 0;
            Packet packet = new Packet();
            packet.AssignBytes(data);

            if (packet.UnreadLength >= 4)
            {
                packetLength = packet.ReadInt();
                if (packetLength <= 0) return; // The packet is empty
            }

            while (packetLength > 0 && packetLength <= packet.UnreadLength)
            {
                byte[] packetBytes = packet.ReadBytes(packetLength);

                // Handle the packet by calling the appropriate method based on the packet ID
                ThreadManager.ExecuteWithPriority(() =>
                {
                    using (Packet pt = new Packet(packetBytes))
                    {
                        int packetId = pt.ReadInt(); // The first integer of the packet indicates the enum of the method to call
                        Client.HandlePacket(packetId, pt); // Call appropriate method to handle the packet
                    }
                });

                // Read the next packet
                packetLength = 0;
                if (packet.UnreadLength >= 4)
                {
                    packetLength = packet.ReadInt();
                    if (packetLength <= 0) return; // The packet is empty
                }
            }

            if (packetLength <= 0) return; // Recycle packets
            else packet.Revert();
        }

        public static void RecieveID(Packet packet)
        {
            ID = packet.ReadInt();
            MessageBox.Show("Nhan duoc ID: " + ID.ToString());
        }

        public static void Disconnect()
        {
            if (client != null) client.Close();
            client = null;
            stream = null;
        }

        public static void sendDataToServer(Packet packet)
        {
            try
            {
                stream.BeginWrite(packet.Array, 0, packet.Length, null, null);
            }
            catch (Exception e)
            {
                //CHET
            }
        }

        public static void SendUsername(string username)
        {
            using (Packet packet = new Packet((int)ClientPackets.ResendUsername)) {
                packet.PutInt(ID);
                packet.PutString(username); 
                packet.InsertLength();


                sendDataToServer(packet);
            }
        }
    }
}
