using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Socket.Server
{
    public enum MatchState
    {
        INIT,
        START_ROUND,
        BEGIN_TURN,
        DETERMINE_NEXT_PLAYER,
        WAIT_ANSWER,
        VERIFY_ANSWER,
        END_TURN,
        END_ROUND,
        END
    }

    public class MatchManager
    {
        public static void Start()
        {

        }

        public static void Update() 
        {

        }

    }
}
