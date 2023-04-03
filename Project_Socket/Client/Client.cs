using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace Project_Socket.Client
{
    internal class Client
    {
        public TcpClient _tcpClient;
        private NetworkStream _stream;
        private Packet _data;
        private Byte[] _buffer;

        private readonly int _id;

        private static string serverIP = "127.0.0.1";
        private static int serverPort = 6295;


        public void ClientConnect(string ip, int port)
        {
            _tcpClient = new TcpClient();
            _tcpClient.Connect(ip, port);

        }

        public void sendPacket(Packet packet) 
        {
            try
            {
                // Get the network stream for the client
                _stream = _tcpClient.GetStream();

                // Send the packet to the server
                _stream.BeginWrite(packet.Array, 0, packet.Length, null, null);
                _stream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending answer: " + ex.Message);
            }
        }

        public Packet receivePacket() 
        {

            _data = new Packet();
            _buffer = new Byte[1024];
            try
            {
                // Open network stream
                _stream = _tcpClient.GetStream();

                // Receive packet from server
                _stream.BeginRead(_buffer, 1024, 1, null, null);
                _stream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something go wrong: " + ex.Message);
            }
            _data = new Packet(_buffer);
            return _data;
        }

    }
}
