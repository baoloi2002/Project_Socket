using Project_Socket.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
            int clientId = packet.ReadInt();
            int answer = packet.ReadInt();
            int rd = packet.ReadInt();
            if (MatchManager._ID == fromClient && rd == MatchManager.currentRound)
            {
                MatchManager.HandleAnswer(clientId, answer);
            }
        }
        public static void HandleUsername(int clientId, string username)
        {
            string sms = "Success";
            // Check if the length of the nickname is less than or equal to 10 characters
            if (username.Length > 10 || username.Length == 0)
            {
                sms = "Length Wrong!";
                ServerSender.RegistrationFailed(clientId, sms);
                return;
            }
            foreach (char c in username)
            {
                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    sms = "Using char in [a..zA..Z0..9_].";
                    ServerSender.RegistrationFailed(clientId, sms);
                    return;
                }
            }
            foreach(ClientItem clientItem in Server.clients.Values)
            if (clientItem != null &&clientItem.player != null && clientItem.player.Name == username)
                {
                    sms = "Name is used by other";
                    ServerSender.RegistrationFailed(clientId, sms);
                    return;
                }
            Server.AcceptPlayerIntoGame(clientId, username);
        }
    }
}
