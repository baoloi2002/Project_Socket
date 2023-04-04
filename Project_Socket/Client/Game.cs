namespace Project_Socket.Client
{
    internal class Game
    {
        // Define quiz game cycle


        // Define quiz game variables
        public int questionNumber = 0;

        public int score = 0;


        public void Wait()
        {
            return;
        }


        public void DoAction()
        {
            int actionChoice = 0;
            // Receive questions from server

            // If player turn do action
            switch (actionChoice)
            {
                case 1:
                    // Choose answer
                    // Wait for button click
                    // Get button name
                    // Check button
                    break;

                case 2:
                    // Skip turn
                    // Give server skip signal
                    break;

                case 3:
                    // Timeout and disqualified
                    // Give server timeout signal
                    break;

                default:
                    // Do nothing
                    break;
            }


        }
    }
}