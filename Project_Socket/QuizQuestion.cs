using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Socket
{
    public class QuizQuestion
    {
        public string question { get; set; }
        public string[] choices { get; set; }
        public int answer { get; set; }
        public bool isCorrect(int choice)
        {
            if (choice == answer)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
