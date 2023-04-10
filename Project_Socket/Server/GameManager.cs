using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Project_Socket.Server
{
    internal class GameManager
    {
        public static bool IsGameStarted = false;
        public static bool IsTimerStarted = false;

        public static float _startTime = 1;
        private static DateTime _lastTick = DateTime.Now;

        public static void Update()
        {         
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
            

            ThreadManager.Update();
        }
        
        public static void ReorderPlayer()
        {            
            foreach(ClientItem client in Server.clients.Values)
            {
                if (client.player != null)
                {
                    ++client.player.Order;
                }
            }
            ServerSender.UpdatePlayerOrder();
        }
        

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

        static Player lastTakeP;

        public static Player DetermineNextPlayer()
        {
            if (MatchManager._Round == MatchManager.currentRound) return lastTakeP;
            MatchManager._Round = MatchManager.currentRound;
            int mi = int.MaxValue;
            int mx = int.MinValue;
            Player player = null;
            foreach (ClientItem client in Server.clients.Values)
            {
                if (client.player != null && !client.player.iskilled)
                {
                    if (client.player.Order < mi)
                    {
                        player = client.player;
                        mi = client.player.Order;
                    }
                    if (client.player.Order > mx)
                    {
                        mx = client.player.Order;
                    }

                }
            }
            if (player != null)
            {
                player.Order = mx + 1;
            }
            lastTakeP = player;
            MatchManager._ID = player.Id;
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
            foreach (ClientItem client in Server.clients.Values)
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
            foreach (ClientItem client in Server.clients.Values)
            {
                if (client.player != null)
                {
                    client.player.ResetForNextRound();
                }
            }
            HandleStartGame();
        }
        

    }
}
