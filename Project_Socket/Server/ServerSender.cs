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

        private static void SendTCPDataToAll(Packet packet)
        {
            packet.InsertLength();
            for (int i = 1; i <= Constants.MAX_PLAYER; i++)
            {
                Server.clients[i].TCP.SendData(packet);
            }
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

        // A client booted up the game and connected to the server
        public static void WelcomePlayer(int toClient, string msg)
        {
            using (Packet packet = new Packet((int)ServerPackets.WelcomePlayer))
            {
                packet.PutInt(toClient);

                SendTCPData(toClient, packet);
                Console.WriteLine($"Server sends to {toClient}: Welcome player");
            }
        }

        // The client inputs invalid username
        public static void RegistrationFailed(int toClient, string msg)
        {
            using (Packet packet = new Packet((int)ServerPackets.RegistrationFailed))
            {
                packet.PutString(msg);
                packet.PutInt(toClient);

                SendTCPData(toClient, packet);
                Console.WriteLine($"Server sends to {toClient}: Registration failed");
            }
        }

        // Everything is okay, the player is sent directly into the game
        public static void RegistrationSuccessful(int toClient, Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.RegistrationSuccessful))
            {
                packet.PutInt(toClient);
                SendTCPData(toClient, packet);

                Console.WriteLine($"Server sends to {toClient}: registration successful");
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
                        packet.PutString(client.player.Name);
                        packet.PutInt(client.player.Order);                        
                        packet.PutBool(client.player.iskilled);
                    }
                }

                SendTCPToAllInMatch(packet);
                Console.WriteLine($"Server sends to all: Update player order");
            }
        }

        public static void CountdownStartGame()
        {
            using (Packet packet = new Packet((int)ServerPackets.CountdownStartGame))
            {
                packet.PutInt(Constants.START_TIMER);
                SendTCPToAllInMatch(packet);

                Console.WriteLine($"Server sends to all: Start countdown");
            }
        }

        public static void StartRound(int roundNumber)
        {
            using (Packet packet = new Packet((int)ServerPackets.StartRound))
            {
                packet.PutInt(roundNumber);
                SendTCPToAllInMatch(packet);

                Console.WriteLine($"Server sends to all: Start round {roundNumber}");
            }
        }

        public static void SetupGame()
        {
            using (Packet packet = new Packet((int)ServerPackets.SetupGame))
            {
                SendTCPToAllInMatch(packet);
                Console.WriteLine($"Server sends to all: Setup game");
            }
        }

        public static void EndRound(int roundNumber)
        {
            using (Packet packet = new Packet((int)ServerPackets.EndRound))
            {
                packet.PutInt(roundNumber);
                SendTCPToAllInMatch(packet);
                Console.WriteLine($"Server sends to all: end round {roundNumber}");
            }
        }

        public static void RemovePlayerFromGame(int clientId)
        {
            using (Packet packet = new Packet((int)ServerPackets.RemovePlayerFromGame))
            {
                packet.PutInt(clientId);
                SendTCPToAllInMatch(packet);

                Console.WriteLine($"Server sends to all: remove player {clientId} from game");
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
        public static void SkipQuiz(Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.SkipQuiz))
            {
                packet.PutInt(player.Id);
                packet.PutString(player.Name);
                SendTCPToAllInMatch(packet);
            }
        }
    }
}
