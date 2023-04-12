using Project_Socket.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Project_Socket.Server
{
    internal class ServerSender
    {
        private static void SendTCPData(int toClient, Packet packet)
        {
            packet.InsertLength();
            Server.clients[toClient].TCP.SendData(packet);
        }

        private static void SendTCPToAllInMatch(Packet packet)
        {
            packet.InsertLength();
            for (int i = 1; i <= Constants.MAX_PLAYER; i++)
            {
                if (Server.clients[i].player != null)
                {
                    Server.clients[i].TCP.SendData(packet);
                }
            }
        }

        public static void WelcomePlayer(int toClient, string msg)
        {
            using (Packet packet = new Packet((int)ServerPackets.WelcomePlayer))
            {
                packet.PutInt(toClient);

                SendTCPData(toClient, packet);
            }
        }

        public static void RegistrationFailed(int toClient, string msg)
        {
            using (Packet packet = new Packet((int)ServerPackets.RegistrationFailed))
            {
                packet.PutString(msg);
                packet.PutInt(toClient);

                SendTCPData(toClient, packet);
            }
        }
        public static void RegistrationSuccessful(int toClient, Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.RegistrationSuccessful))
            {
                packet.PutInt(toClient);
                SendTCPData(toClient, packet);
            }
        }
        
        public static void UpdatePlayerOrder()
        {
            using (Packet packet = new Packet((int)ServerPackets.UpdatePlayerOrder))
            {
                int counter = 0;
                foreach (ClientItem client in Server.clients.Values)
                {
                    if (client.player != null) counter++;
                }
                packet.PutInt(counter);

                foreach (ClientItem client in Server.clients.Values)
                {
                    if (client.player != null)
                    {
                        packet.PutInt(client.player.Id);
                        packet.PutString(client.player.Name);
                        packet.PutInt(client.player.Order);                        
                        packet.PutBool(client.player.iskilled);
                    }
                }

                SendTCPToAllInMatch(packet);
            }
        }

        public static void CountdownStartGame()
        {
            using (Packet packet = new Packet((int)ServerPackets.CountdownStartGame))
            {
                SendTCPToAllInMatch(packet);
            }
        }

        public static void StartRound(int roundNumber)
        {
            using (Packet packet = new Packet((int)ServerPackets.StartRound))
            {
                packet.PutInt(roundNumber);
                SendTCPToAllInMatch(packet);
            }
        }

        public static void SetupGame()
        {
            using (Packet packet = new Packet((int)ServerPackets.SetupGame))
            {
                SendTCPToAllInMatch(packet);
            }
        }

        public static void EndRound(int roundNumber)
        {
            using (Packet packet = new Packet((int)ServerPackets.EndRound))
            {
                packet.PutInt(roundNumber);
                SendTCPToAllInMatch(packet);
            }
        }

        public static void SendQuestion(QuizQuestion question)
        {
            using (Packet packet = new Packet((int)ServerPackets.SendQuestion))
            {
                packet.PutString(question.question);
                packet.PutString(question.choices[0]);
                packet.PutString(question.choices[1]);
                packet.PutString(question.choices[2]);
                packet.PutString(question.choices[3]);
                SendTCPToAllInMatch(packet);                
            }
        }

        public static void SendAnswer(int ans)
        {
            using (Packet packet = new Packet((int)ServerPackets.SendAnswer))
            {
                packet.PutInt(ans);
                SendTCPToAllInMatch(packet);
            }
        }
        public static void SkipQuiz()
        {
            using (Packet packet = new Packet((int)ServerPackets.SkipQuiz))
            {
                SendTCPToAllInMatch(packet);
            }
        }

        public static void NumberOfQuestion(int cnt)
        {
            using (Packet packet = new Packet((int)(ServerPackets.NumberOfQuestion)))
            {
                packet.PutInt(cnt);
                SendTCPToAllInMatch(packet);
            }
        }
        public static void SendYOUWIN()
        {
            using (Packet packet = new Packet((int)(ServerPackets.YOUWIN)))
            {                
                foreach(ClientItem client in Server.clients.Values)
                {
                    if (client != null &&  client.player != null && !client.player.iskilled)
                    {
                        SendTCPData(client.player.Id, packet);
                    }
                }
            }
        }
        public static void EndGame()
        {
            using (Packet packet = new Packet((int)(ServerPackets.EndGame)))
            {
                SendTCPToAllInMatch(packet);
            }
        }
    }
}
