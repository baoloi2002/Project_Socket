using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private static void SendTCPDataToAll(int exceptClient, Packet packet)
        {
            packet.InsertLength();
            for (int i = 1; i <= Constants.MAX_PLAYER; i++)
            {
                if (i != exceptClient)
                {
                    Server.clients[i].TCP.SendData(packet);
                }
            }
        }

        private static void SendTCPToAllInMatch(Packet packet)
        {
            packet.InsertLength();
            for (int i = 1; i <= Constants.MAX_PLAYER; i++)
            {
                if (Server.clients[i].player != null && Server.clients[i].player.inGame)
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
                packet.PutString(msg);
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
        /*
        public static void SendPlayerIntoGame(int toClient, Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.SendPlayerIntoGame))
            {
                packet.PutString(player.Name);
                packet.PutInt(player.Id);
                SendTCPData(toClient, packet);

                Console.WriteLine($"Server sends to {toClient}: send player into game");
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
                        packet.PutInt(client.player.order);
                        packet.PutInt(client.player.Id);
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
                //packet.PutInt(Constants.START_TIMER);
                SendTCPToAllInMatch(packet);

                Console.WriteLine($"Server sends to all: Start countdown");
            }
        }

        public static void StartRound(KeyValuePair<string, string> question, string currentKeyword, int roundNumber)
        {
            using (Packet packet = new Packet((int)ServerPackets.StartRound))
            {
                packet.PutInt(roundNumber);
                packet.PutString(currentKeyword);
                packet.PutString(question.Value);
                SendTCPToAllInMatch(packet);

                Console.WriteLine($"Server sends to all: Start round {roundNumber}");
            }
        }

        public static void StartTurn(int turnNumber)
        {
            using (Packet packet = new Packet((int)ServerPackets.StartTurn))
            {
                packet.PutInt(turnNumber);
                SendTCPToAllInMatch(packet);

                Console.WriteLine($"Server sends to all: Start turn {turnNumber}");
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

        public static void WaitForNextPlayer()
        {
            using (Packet packet = new Packet((int)ServerPackets.WaitForNextPlayer))
            {
                SendTCPToAllInMatch(packet);
                Console.WriteLine($"Server sends to all: wait for next player");
            }
        }

        public static void PickNextPlayer(Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.PickNextPlayer))
            {
                packet.PutInt(player.Id);
                packet.PutString(player.Name);
                SendTCPToAllInMatch(packet);
                Console.WriteLine($"Server sends to all: pick next player {player.Id}");
            }
        }

        public static void VerifyAnswer(Player player, bool correctCharacter, string character, bool correctKeyword, string keyword, bool isDisqualified)
        {
            using (Packet packet = new Packet((int)ServerPackets.VerifyAnswer))
            {
                packet.PutInt(player.Id);
                packet.PutString(player.Name);
                packet.PutBool(correctCharacter);
                packet.PutString(character);
                packet.PutBool(correctKeyword);
                packet.PutString(keyword);
                packet.PutBool(isDisqualified);
                SendTCPDataToAll(packet);

                Console.WriteLine($"Server sends to all: Verify answer by {player.Id}, character is {character} ({correctCharacter.ToString()}), keyword is {keyword} ({correctKeyword.ToString()}, he is disqualified {isDisqualified.ToString()})");
            }
        }

        public static void ShowResult()
        {
            using (Packet packet = new Packet((int)ServerPackets.ShowResult))
            {

                List<Player> players = GameManager.GetAllPlayers();
                packet.PutInt(MatchManager.currentRound);
                packet.PutInt(players.Count);

                foreach (Player player in players)
                {
                    packet.PutInt(player.Id);
                    packet.PutString(player.Name);
                    packet.PutBool(player.iskilled);
                }
                SendTCPToAllInMatch(packet);
            }
        }

        public static void EndTurn(int currentTurn)
        {
            using (Packet packet = new Packet((int)ServerPackets.EndTurn))
            {
                packet.PutInt(currentTurn);
                SendTCPToAllInMatch(packet);
                Console.WriteLine($"Server sends to all: end turn {currentTurn}");
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

        public static void EndGame()
        {
            using (Packet packet = new Packet((int)ServerPackets.EndGame))
            {
                SendTCPToAllInMatch(packet);
                Console.WriteLine($"Server sends to all: end game");
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

        public static void UpdateRoundInfo(string currentKeyword, int currentRound, HashSet<char> unlockedCharacters)
        {
            using (Packet packet = new Packet((int)ServerPackets.UpdateRoundInfo))
            {
                // Update the info of all players in the match
                List<Player> players = GameManager.GetAllPlayers();
                packet.PutInt(currentRound);
                packet.PutString(currentKeyword);

                packet.PutInt(unlockedCharacters.Count);
                foreach (char c in unlockedCharacters)
                {
                    packet.PutString(c.ToString());
                }

                packet.PutInt(players.Count);
                foreach (Player player in players)
                {
                    packet.PutInt(player.ID);
                    packet.PutString(player.Name);
                    packet.PutBool(player.isDisqualified);
                    packet.PutInt(player.score);
                }

                SendTCPToAllInMatch(packet);

                Console.WriteLine($"Server sends to all: update round info");
            }
        }

        public static void PlayerLeave(int clientId, string username)
        {
            using (Packet packet = new Packet((int)ServerPackets.PlayerLeave))
            {
                packet.PutInt(clientId);
                packet.PutString(username);
                SendTCPDataToAll(packet);

                Console.WriteLine($"Server sends to all: player {clientId} leaves");
            }
        }
        */
    }
}
