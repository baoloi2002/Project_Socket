using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Windows;

namespace Project_Socket.Client
{
    internal class Client
    {
        private static TcpClient client;
        private static NetworkStream stream;
        private static Packet _data;
        private static byte[] buffer;

        public static int ID = 0;
        public static string nickname;
        public static bool isRegSuccess = false;

        public delegate void PacketHandler(Packet _packet);

        public static Dictionary<int, PacketHandler> packetHandlers;
        public static List<Player> playerList;

        public static QuizQuestion question;
        

        public static void HandlePacket(int id, Packet packet) => packetHandlers[id](packet);



        public static void Start()
        {
            playerList = new List<Player>();
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ServerPackets.WelcomePlayer, Client.RecieveID },
                {(int)ServerPackets.PlayerLeave, Client.ReceivePlayerLeave },

                {(int)ServerPackets.RegistrationFailed, Client.RegistrationFailed },
                {(int)ServerPackets.RegistrationSuccessful, Client.RegistrationSuccessful },

                {(int)ServerPackets.SendPlayerIntoGame, Client.ReceiveSendPlayerIntoGame },
                {(int)ServerPackets.RemovePlayerFromGame, Client.ReceiveRemovePlayerFromGame },
                {(int)ServerPackets.UpdatePlayerOrder, Client.UpdatePlayerOrder },
                {(int)ServerPackets.CountdownStartGame, Client.CountdownStartGame },

                {(int)ServerPackets.SetupGame, Client.SetupGame },
                {(int)ServerPackets.StartRound, Client.StartRound },
                {(int)ServerPackets.SendQuestion, Client.ReceiveSendQuestion },
                {(int)ServerPackets.SendAnswer, Client.ReceiveSendAnswer },
                {(int)ServerPackets.SkipQuiz, Client.ReceiveSkipQuiz },
                {(int)ServerPackets.WaitForNextPlayer, Client.ReceiveWaitForNextPlayer },
                {(int)ServerPackets.PickNextPlayer, Client.ReceivePickNextPlayer },
                {(int)ServerPackets.VerifyAnswer, Client.ReceiveVerifyAnswer },
                {(int)ServerPackets.ShowResult, Client.ReceiveShowResult },
                {(int)ServerPackets.EndRound, Client.EndRound },
                {(int)ServerPackets.EndGame, Client.ReceiveEndGame },
                {(int)ServerPackets.UpdateRoundInfo, Client.ReceiveUpdateRoundInfo },
            };
        }


        public static void Connect(string server, int port)
        {
            try
            {
                client = new TcpClient(server, port);
                client.ReceiveBufferSize = Constants.DATA_BUFFER_SIZE;
                client.SendBufferSize = Constants.DATA_BUFFER_SIZE;

                stream = client.GetStream();
                _data = new Packet();
                buffer = new byte[Constants.DATA_BUFFER_SIZE];

                stream.BeginRead(buffer, 0, Constants.DATA_BUFFER_SIZE, OnReceivedData, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error connecting to server: " + e.Message);
                ID = -1;
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
                if (HandleData(data)) _data.Reset();
                else _data.Revert();

                // Read the next packet
                stream.BeginRead(buffer, 0, Constants.DATA_BUFFER_SIZE, OnReceivedData, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error receiving data from server: " + e.Message);
                Disconnect();
            }
        }

        private static bool HandleData(byte[] data)
        {
            // Parse the incoming data using the same packet format as the server
            int packetLength = 0;
            _data.AssignBytes(data);

            if (_data.UnreadLength >= 4)
            {
                packetLength = _data.ReadInt();
                if (packetLength <= 0) return true; // The packet is empty
            }

            while (packetLength > 0 && packetLength <= _data.UnreadLength)
            {
                byte[] packetBytes = _data.ReadBytes(packetLength);
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
                if (_data.UnreadLength >= 4)
                {
                    packetLength = _data.ReadInt();
                    if (packetLength <= 0) return true; // The packet is empty
                }
            }

            if (packetLength <= 0) return true; // Recycle packets
            return false;
        }

        public static void Disconnect()
        {
            if (client != null) return;
            client.Close();
            client = null;
            stream = null;
            _data = null;
            buffer = null;
        }

        public static void sendDataToServer(Packet packet)
        {
            try
            {
                if (client != null) stream.BeginWrite(packet.Array, 0, packet.Length, null, null);
            }
            catch (Exception e)
            {
                // Die
            }
        }

        public static void receiveDataFromServer(Packet packet)
        {
            try
            {
                stream.BeginRead(buffer, 0, buffer.Length, OnReceivedData, null);
            }
            catch (Exception e)
            {
                // Die 
            }
        }

        #region SendThings

        public static void SendUsername()
        {
            using (Packet packet = new Packet((int)ClientPackets.ResendUsername))
            {
                packet.PutInt(ID);
                packet.PutString(nickname);
                packet.InsertLength();

                sendDataToServer(packet);
            }
        }

        public static void SendAnswer(int answer)
        {
            using (Packet packet = new Packet((int)ClientPackets.GiveAnswer))
            {
                packet.PutInt(ID);
                packet.PutInt(answer);
                packet.InsertLength();

                sendDataToServer(packet);
            }
        }

        public static void SendSkip(int skip)
        {
            using (Packet packet = new Packet((int)ClientPackets.Skip))
            {
                packet.PutInt(ID);
                packet.PutInt(skip);
                packet.InsertLength();

                sendDataToServer(packet);
            }
        }

        #endregion
        #region ReceiveThings
        public static void UpdatePlayerOrder(Packet packet)
        {
            List<Player> nw = new List<Player>();
            int num = packet.ReadInt();
            for (int i = 0; i < num; ++i)
            {
                string name = packet.ReadString();
                int order = packet.ReadInt();
                bool iskilled = packet.ReadBool();
                nw.Add(new Player(name, order, iskilled));
            }
            playerList = nw;
        }

        public static void RecieveID(Packet packet)
        {
            ID = packet.ReadInt();
        }        
        public static void RegistrationFailed(Packet packet)
        {
            nickname = "";
        }       
        public static void RegistrationSuccessful(Packet packet)
        {
            isRegSuccess = true;
            ClientWindow.isAnnounce = true;
        }
        public static void ReceiveSendQuestion(Packet packet)
        {
            question = new QuizQuestion();
            question.question = packet.ReadString();
            string c0 = packet.ReadString();
            string c1 = packet.ReadString();
            string c2 = packet.ReadString();
            string c3 = packet.ReadString();
            question.choices = new string[]{ c0, c1, c2, c3};
        }        
        public static void ReceiveSendAnswer(Packet packet)
        {
            question.answer = packet.ReadInt();
        }  
        public static void StartRound(Packet packet)
        {

        }
        public static void CountdownStartGame(Packet packet)
        {

        }
        public static void ReceiveSkipQuiz(Packet packet)
        {

        }

        public static void EndRound(Packet packet)
        {

        }

        public static void ReceivePlayerLeave(Packet packet) { }
        public static void ReceiveSendPlayerIntoGame(Packet packet) { }
        public static void ReceiveRemovePlayerFromGame(Packet packet) { }
        public static void SetupGame(Packet packet) { }
        public static void ReceiveWaitForNextPlayer(Packet packet) { }
        public static void ReceivePickNextPlayer(Packet packet) { }
        public static void ReceiveVerifyAnswer(Packet packet) { }
        public static void ReceiveShowResult(Packet packet) { }
        public static void ReceiveEndGame(Packet packet) { }
        public static void ReceiveUpdateRoundInfo(Packet packet) { }

        #endregion
    }
}