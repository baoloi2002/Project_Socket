using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Socket.Server
{
    internal class GameManager
    {
        public static bool IsGameStarted = false;
        public static bool IsTimerStarted = false;

        private static float _startTime = 1;
        private static DateTime _lastTick = DateTime.Now;

        public static void Update()
        {
            foreach (ClientItem client in Server.clients.Values)
            {
                if (client.player != null)
                {
                    //client.player.Update();
                }
            }
            /*
            if (IsTimerStarted)
            {
                _startTime -= (float)(DateTime.Now - _lastTick).Milliseconds / 1000;
                _lastTick = DateTime.Now;
                if (_startTime <= 0)
                {
                    // start the game
                    Console.WriteLine("The game is now started.");
                    IsTimerStarted = false;
                    StartGame();
                }
            }

            if (IsGameStarted)
            {
                MatchManager.Update();
            }
            */

            ThreadManager.Update();
        }
        /*
        public static void ReorderPlayer()
        {
            int accu = 1;
            foreach (Client client in Server.clients.Values.OrderByDescending(client =>
            {
                if (client.player != null) return client.player.joinedDate;
                return DateTime.Now;
            }))
            {
                if (client.player != null)
                    client.player.order = accu++;
            }
            ServerSender.UpdatePlayerOrder();
        }
        */

        public static int GetPlayerCount()
        {
            int playerCount = 0;
            foreach (ClientItem client in Server.clients.Values)
            {
                if (client.player != null) playerCount++;
            }
            return playerCount;
        }

        public static List<Player> GetAllPlayers()
        {
            List<Player> players = new List<Player>();
            foreach (ClientItem client in Server.clients.Values)
            {
                if (client.player != null)
                {
                    players.Add(client.player);
                }
            }
            return players;
        }
        /*
        public static Player DetermineNextPlayer(int highestOrder)
        {
            int minDist = int.MaxValue;
            Player player = null;
            foreach (Client client in Server.clients.Values)
            {
                if (client.player != null && !client.player.isDisqualified && client.player.order > highestOrder && client.player.order - highestOrder < minDist)
                {
                    minDist = client.player.order - highestOrder;
                    player = client.player;
                }
            }
            return player;
        }

        public static void HandleStartGame()
        {
            int playerCount = GetPlayerCount();

            if (playerCount >= 2)
            {
                _startTime = Constants.START_TIMER;
                _lastTick = DateTime.Now;
                IsTimerStarted = true;

                ServerSender.CountdownStartGame();
            }
            else
            {
                IsTimerStarted = false;
                _startTime = 1;
            }
        }

        public static void StartGame()
        {
            IsGameStarted = true;
            MatchManager.Start();
            ResetPlayerStats();
        }

        public static void ResetPlayerStats()
        {
            foreach (Client client in Server.clients.Values)
            {
                if (client.player != null && client.player.inGame)
                {
                    client.player.ResetPlayer();
                }
            }
        }

        public static void ResetPlayersForNextRound()
        {
            foreach (Client client in Server.clients.Values)
            {
                if (client.player != null && client.player.inGame)
                {
                    client.player.ResetForNextRound();
                }
            }
        }

        public static void StopGame()
        {
            IsGameStarted = false;
        }

        public static void EndGame()
        {
            IsGameStarted = false;
            foreach (Client client in Server.clients.Values)
            {
                if (client.player != null && client.player.inGame)
                {
                    Server.RemovePlayerFromGame(client.ID);
                }
            }
        }
        */

    }
}
