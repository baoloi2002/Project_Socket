using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Socket.Server
{
    internal class ServerHandler
    {
        public static void HandshakeServer(int fromClient, Packet packet)
        {
            int clientId = packet.ReadInt();
            string username = packet.ReadString();
            var endpoint = Server.clients[fromClient].TCP.socket.Client.RemoteEndPoint;

            Console.WriteLine($"Receive client {clientId}: handshake server");
            // if (GameManager.IsGameStarted) {
            //     ServerSender.MatchInProgress(fromClient);
            //     return;
            // }

            // if (IsValidUsername(fromClient, username)) {
            //     Server.clients[fromClient].ConstructPlayer(username);
            //     Console.WriteLine($"{endpoint} with username {username} connected successfully and is now player {fromClient}.");
            // }

            // if (fromClient != clientId)
            // {
            //     Console.WriteLine($"Player \"{username}\" (ID: {fromClient}) has assumed the wrong client ID ({clientId})!");
            // }
        }

        public static void ResendUsername(int fromClient, Packet packet)
        {
            Console.WriteLine($"Receive client {fromClient}: resend username");

            int clientId = packet.ReadInt();
            string username = packet.ReadString();
            var endpoint = Server.clients[fromClient].TCP.socket.Client.RemoteEndPoint;

            HandleUsername(fromClient, username);
        }

        public static void GiveAnswer(int fromClient, Packet packet)
        {
            Console.WriteLine($"Receive client {fromClient}: give answer");

            int clientId = packet.ReadInt();
            string character = packet.ReadString();
            string keyword = packet.ReadString();

            //MatchManager.HandleAnswer(fromClient, character, keyword);
        }

        public static void HandleUsername(int clientId, string username)
        {

        }
    }
}
