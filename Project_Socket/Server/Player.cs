using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Project_Socket.Server
{
    internal class Player
    {
        public int Id;
        public DateTime joinedDate;
        public bool iskilled = false;
        public string Name { get; set; }
        public PackIconKind Icon { get; set; }
        public int Order { get; set; }

        public Player(int id, string username)
        {
            this.Id = id;
            this.Name = username;
            this.Icon = PackIconKind.Account;
        }
        public void ResetForNextRound()
        {
            iskilled = false;
        }
    }
}

