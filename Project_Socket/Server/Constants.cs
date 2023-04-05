using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Socket.Server
{
    public class Constants
    {
        public static string SERVER_IP = "127.0.0.1";
        public static int PORT = 1234;
        public static int DATA_BUFFER_SIZE = 1024;

        public static int MAX_PLAYER = 10;
        public static int MIN_PLAYER = 2;

        public static int TIME_PER_ROUND = 15; //second
        public const int TICKS_PER_SEC = 30;
        public const float MS_PER_TICK = 1000f / TICKS_PER_SEC;

        public static int MAX_LENGTH_NAME = 10;

        public static int START_TIMER = 10;//second

    }
}
