using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Project_Socket
{
    internal class ThreadManager
    {
        private static readonly List<Action> _executePriority = new List<Action>();
        private static readonly List<Action> _executeDuplicatePriority = new List<Action>();
        private static bool _newPriorityAdded = false;

        public static void Update()
        {
            if (_newPriorityAdded)
            {
                _executeDuplicatePriority.Clear();
                lock (_executePriority)
                {
                    _executeDuplicatePriority.AddRange(_executePriority);
                    _executePriority.Clear();
                    _newPriorityAdded = false;
                }

                for (int i = 0; i < _executeDuplicatePriority.Count; i++)
                {
                    _executeDuplicatePriority[i]();
                }
            }
        }

        public static void ExecuteWithPriority(Action _action)
        {
            lock (_executePriority)
            {
                _executePriority.Add(_action);
                _newPriorityAdded = true;
            }
        }
    }
}
