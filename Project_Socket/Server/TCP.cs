using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Project_Socket.Server
{
    internal class TCP
    {
        public TcpClient socket;
        private NetworkStream _stream;
        private Packet _data;
        private Byte[] _buffer;
        private readonly int _id;

        public TCP(int id)
        {
            _id = id;
        }

        // Connect to the client via TCP socket
        public void Connect(TcpClient tcpSocket)
        {
            socket = tcpSocket;
            socket.ReceiveBufferSize = Constants.DATA_BUFFER_SIZE;
            socket.SendBufferSize = Constants.DATA_BUFFER_SIZE;

            _stream = socket.GetStream();

            _data = new Packet();
            _buffer = new Byte[Constants.DATA_BUFFER_SIZE];

            _stream.BeginRead(_buffer, 0, Constants.DATA_BUFFER_SIZE, OnReceivedData, null);

            ServerSender.WelcomePlayer(_id, "Welcome to the game!");
        }

        // Send data to the server via TCP
        public void SendData(Packet packet)
        {
            try
            {
                if (socket != null) _stream.BeginWrite(packet.Array, 0, packet.Length, null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to send data to player {_id} via TCP: {e}");
            }
        }

        // Read data from the stream
        private void OnReceivedData(IAsyncResult result)
        {
            try
            {
                int byteLength = _stream.EndRead(result);
                if (byteLength <= 0)
                {
                    //Server.DisconnectClient(_id);
                    return;
                }

                Byte[] streamedData = new Byte[byteLength];
                Array.Copy(_buffer, streamedData, byteLength);

                if (HandleData(streamedData)) _data.Reset();
                else _data.Revert();

                // Continue reading the next arriving packets
                _stream.BeginRead(_buffer, 0, Constants.DATA_BUFFER_SIZE, OnReceivedData, null);
            }
            catch (Exception e)
            {
                // If any error occurs while reading the data from the stream, then disconnect the client
                Console.WriteLine($"Error receiving TCP data: {e}");
                Disconnect();
            }
        }

        // Extract the data in order, and call the matching handler methods
        public bool HandleData(Byte[] data)
        {
            int packetLength = 0;
            _data.AssignBytes(data);

            // The beginning of the packet is always the length of that packet
            if (_data.UnreadLength >= 4)
            {
                packetLength = _data.ReadInt();
                if (packetLength <= 0) return true; // The packet is empty
            }

            while (packetLength > 0 && packetLength <= _data.UnreadLength)
            {
                Byte[] packetBytes = _data.ReadBytes(packetLength);
                ThreadManager.ExecuteWithPriority(() =>
                {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packetId = packet.ReadInt(); // The first integer of the packet indicates the enum of the method to call
                        Server.HandlePacket(packetId, _id, packet); // Call appropriate method to handle the packet
                    }
                });

                packetLength = 0;
                if (_data.UnreadLength >= 4)
                {
                    // The length of the packet exceeds the length written at the beginning, meaning there is another packet arriving in order
                    packetLength = _data.ReadInt();
                    if (packetLength <= 0) return true;
                }
            }

            if (packetLength <= 0) return true; // Recycle packets
            return false; // The next packet hasn't arrived fully, so we delay by temporarily unread the length bytes
        }

        public void Disconnect()
        {
            if (socket == null) return;
            socket.Close();
            _stream = null;
            _data = null;
            _buffer = null;
            socket = null;
        }
    }
}
