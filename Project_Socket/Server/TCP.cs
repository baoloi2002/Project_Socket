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
        private byte[] _buffer;
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
            _buffer = new byte[Constants.DATA_BUFFER_SIZE];

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
                //Failed to send data to player {_id} via TCP: {e}
            }
        }

        private void OnReceivedData(IAsyncResult result)
        {
            try
            {
                int byteLength = _stream.EndRead(result);
                if (byteLength <= 0)
                {
                    Server.DisconnectClient(_id);
                    return;
                }

                byte[] streamedData = new byte[byteLength];
                Array.Copy(_buffer, streamedData, byteLength);

                if (HandleData(streamedData)) _data.Reset();
                else _data.Revert();

                _stream.BeginRead(_buffer, 0, Constants.DATA_BUFFER_SIZE, OnReceivedData, null);
            }
            catch (Exception e)
            {
                Disconnect();
            }
        }
        public bool HandleData(byte[] data)
        {
            int packetLength = 0;
            _data.AssignBytes(data);

            if (_data.UnreadLength >= 4)
            {
                packetLength = _data.ReadInt();
                if (packetLength <= 0) return true;
            }

            while (packetLength > 0 && packetLength <= _data.UnreadLength)
            {
                byte[] packetBytes = _data.ReadBytes(packetLength);

                ThreadManager.ExecuteWithPriority(() =>
                {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packetId = packet.ReadInt(); 
                        Server.HandlePacket(packetId, _id, packet);
                    }
                });

                packetLength = 0;
                if (_data.UnreadLength >= 4)
                {                 
                    packetLength = _data.ReadInt();
                    if (packetLength <= 0) return true;
                }
            }

            if (packetLength <= 0) return true; 
            return false; 
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
