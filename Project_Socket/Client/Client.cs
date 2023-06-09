﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;

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
        public static int _round = -1;

        public static void HandlePacket(int id, Packet packet) => packetHandlers[id](packet);

        public static int currentNumberOfQuestion = 0;
        public static int NumberOfQuestion = 0;

        public static void Start()
        {
            playerList = new List<Player>();
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ServerPackets.WelcomePlayer, Client.RecieveID },

                {(int)ServerPackets.RegistrationFailed, Client.RegistrationFailed },
                {(int)ServerPackets.RegistrationSuccessful, Client.RegistrationSuccessful },

                {(int)ServerPackets.UpdatePlayerOrder, Client.UpdatePlayerOrder },
                {(int)ServerPackets.CountdownStartGame, Client.CountdownStartGame },

                {(int)ServerPackets.SetupGame, Client.SetupGame },
                {(int)ServerPackets.StartRound, Client.StartRound },
                {(int)ServerPackets.SendQuestion, Client.ReceiveSendQuestion },
                {(int)ServerPackets.SendAnswer, Client.ReceiveSendAnswer },
                {(int)ServerPackets.SkipQuiz, Client.ReceiveSkipQuiz },
                {(int)ServerPackets.EndRound, Client.EndRound },
                {(int)ServerPackets.EndGame, Client.ReceiveEndGame },

                {(int)ServerPackets.NumberOfQuestion, Client.ReceiveNumberOfQuestion },
                {(int)ServerPackets.YOUWIN, Client.YOUWIN },
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
                    Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(buffer, data, byteLength);

                if (HandleData(data)) _data.Reset();
                else _data.Revert();

                stream.BeginRead(buffer, 0, Constants.DATA_BUFFER_SIZE, OnReceivedData, null);
            }
            catch (Exception e)
            {
                Disconnect();
            }
        }

        private static bool HandleData(byte[] data)
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
                    using (Packet pt = new Packet(packetBytes))
                    {
                        int packetId = pt.ReadInt(); 
                        Client.HandlePacket(packetId, pt); 
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
                packet.PutInt(_round);
                packet.InsertLength();

                sendDataToServer(packet);
            }
        }

        public static void SendSkip()
        {
            using (Packet packet = new Packet((int)ClientPackets.GiveAnswer))
            {
                packet.PutInt(ID);
                packet.PutInt(5);
                packet.PutInt(_round);
                packet.InsertLength();

                sendDataToServer(packet);
            }
        }

        #endregion SendThings

        #region ReceiveThings

        public static void UpdatePlayerOrder(Packet packet)
        {
            List<Player> nw = new List<Player>();
            int num = packet.ReadInt();
            for (int i = 0; i < num; ++i)
            {
                int id = packet.ReadInt();
                string name = packet.ReadString();
                int order = packet.ReadInt();
                bool iskilled = packet.ReadBool();
                nw.Add(new Player(id, name, order, iskilled));
            }
            playerList = nw;
            int bestID = -1, bestOrder = -1;
            foreach (Player player in playerList)
            {
                if (player.iskilled) continue;
                if (player.Order > bestOrder)
                {
                    bestOrder = player.Order;
                    bestID = player.Id;
                }
            }

            if (bestID == ID)
                ClientGame.isTurn = true;
            else
                ClientGame.isTurn = false;
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
            question.choices = new string[] { c0, c1, c2, c3 };
            question.answer = -1;
            ClientGame._Timer = Constants.TIME_PER_ROUND + 2;

            currentNumberOfQuestion += 1;
        }

        public static bool isLose;

        public static void ReceiveSendAnswer(Packet packet)
        {
            question.answer = packet.ReadInt();
            if (ClientGame.isTurn)
                isLose = question.answer != ClientGame.clientAnswer;
            ClientGame._Timer = 0;
        }

        public static void StartRound(Packet packet)
        {
            if (isLose)
            {
                ClientGame.gameEnd = true;
                isLose = false;
            }
            ClientGame._Timer = 0;
            int tmp = packet.ReadInt();
            if (tmp != null && tmp > _round)
            {
                _round = tmp;
            }
        }

        public static void CountdownStartGame(Packet packet)
        {
            ClientGame._Timer = Constants.START_TIMER;
        }

        public static void ReceiveSkipQuiz(Packet packet)
        {
            ClientGame._Timer = Constants.TIME_PER_ROUND + 2;
            isLose = false;
            currentNumberOfQuestion -= 1;
        }

        public static void EndRound(Packet packet)
        {
            ClientGame._Timer = 0;
            int tmp = packet.ReadInt();
            if (tmp != null && tmp > _round)
            {
                _round = tmp;
            }
        }

        public static void ReceiveNumberOfQuestion(Packet packet)
        {     
            currentNumberOfQuestion = 0;
            NumberOfQuestion = packet.ReadInt();
        }

        public static void YOUWIN(Packet packet)
        {
            ClientGame.isWin = true;
            ClientGame.gameEnd = true;
        }
        public static void SetupGame(Packet packet)
        {
            question = null;
            isLose = false;
            ClientGame.isSkip = false;
            ClientGame.isSetup = true;
            ClientGame.isWin = false;
            ClientGame.gameEnd = false;
            ClientGame.isTurn = false;
        }
        public static void ReceiveEndGame(Packet packet)
        {
            if (isLose)
            {
                ClientGame.gameEnd = true;
                isLose = false;
            }
        }

        #endregion ReceiveThings
    }
}