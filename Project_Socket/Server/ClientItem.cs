using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace Project_Socket.Server
{
    internal class ClientItem
    {
        public int ID = 0;
        public Player player;
        public TCP TCP;

        public ClientItem(int clientId) { 
            ID = clientId;
            TCP = new TCP(ID);
        }
    }
}
