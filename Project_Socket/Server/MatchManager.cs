using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Project_Socket.Server
{
    public enum MatchState
    {
        INIT,
        START_ROUND,
        WAIT_ANSWER,
        VERIFY_ANSWER,
        SKIP_QUIZ,
        END_ROUND,
        END
    }

    public class MatchManager
    {
        public static QuizQuestion[] quizList;
        public static int curQuiz = -1;
        public static Dictionary<int, bool> isUsedSkill;
        public static float _waitTimer = 3;
        private static bool _isWaiting = false;
        private static bool _isLocked = false;
        private static Player _currentPlayer;
        private static DateTime _lastTick = DateTime.Now;
        private static MatchState _matchState = MatchState.END;
        private static Action _timerCallback;
        private static bool _isAnswered = false;
        public static int currentRound;
        public static int currentTurn, _Turn;

        public static void Start()
        {
            curQuiz = -1;
            currentRound = 0;
            isUsedSkill = new Dictionary<int, bool>();
            ChangeState(MatchState.INIT);
            _Turn = 0;
        }

        public static void Update() 
        {
            if (_waitTimer > 0)
            {
                _waitTimer -= (float)(DateTime.Now - _lastTick).Milliseconds / 1000;
                _lastTick = DateTime.Now;
                if (_waitTimer <= 0)
                {
                    if (_timerCallback != null)
                    {
                        _timerCallback.Invoke();
                    }
                    _isWaiting = false;
                    _isLocked = false;
                }
                if (_isLocked) return;
            }

            if (GameManager.GetPlayerCount() <= 1)
            {
                Console.WriteLine("Too few players to continue the game!");
                GameManager.EndGame();
            }

            switch (_matchState)
            {                
                case MatchState.WAIT_ANSWER:
                    if (_waitTimer <= 0 && !_isAnswered) HandleAnswer(_currentPlayer.Id, -1);
                    break;
                default: break;
            }
        }

        private static void SetTimer(int timer, Action callback, bool exclusive = false)
        {
            _waitTimer = timer;
            _isWaiting = true;
            _lastTick = DateTime.Now;
            _isLocked = exclusive;
            _timerCallback = callback;
        }

        public static void HandleAnswer(int clientId, int ans)
        {
            if (_matchState != MatchState.WAIT_ANSWER) return;
            if (ans == 5 && isUsedSkill.ContainsKey(clientId)) return; // SKIP TO NEXT 
            if (ans == 5)
            {
                isUsedSkill[clientId] = true;
                // Do something
                if (_waitTimer > 0)
                    return;
                ChangeState(MatchState.SKIP_QUIZ);
            }
            if (quizList[curQuiz].answer != ans)
            {
                _currentPlayer.iskilled = true;
                Server.clients[clientId].player.Lose();
                // Do something
                ChangeState(MatchState.VERIFY_ANSWER);                
            }
            else
            {
                // Do something
                ChangeState(MatchState.VERIFY_ANSWER);
            }
            ChangeState(MatchState.VERIFY_ANSWER);
        }

        private static int PlayerCount()
        {
            int count = 0;
            foreach(ClientItem client in Server.clients.Values)
            {
                if (client.player != null && !client.player.iskilled)
                {
                    ++count;
                }
            }
            return count;
        }
        private static void ChangeState(MatchState matchState)
        {
            Console.WriteLine($"Changing the match state from {_matchState.ToString()} to {matchState.ToString()}");
            if (_matchState == matchState) return;

            // Exit out of old stage
            switch (_matchState)
            {
                case MatchState.INIT:
                    break;

                case MatchState.START_ROUND:
                    break;

                case MatchState.WAIT_ANSWER:
                    break;

                case MatchState.VERIFY_ANSWER:
                    break;

                case MatchState.SKIP_QUIZ:
                    break;

                case MatchState.END_ROUND:
                    break;

                case MatchState.END:
                    break;
            }

            _matchState = matchState;

            // Enter new stage
            switch (_matchState)
            {
                case MatchState.INIT:
                    currentRound = 0;
                    quizList = LoadQuestions("QuizList.json");
                    curQuiz = -1;
                    ServerSender.SetupGame();
                    SetTimer(5, () => ChangeState(MatchState.START_ROUND), true);
                    break;

                case MatchState.START_ROUND:
                    currentRound += 1;
                    // Choose a random question
                    curQuiz += 1;
                    if (curQuiz == quizList.Length)
                    {                       
                        ChangeState(MatchState.END);
                        break;
                    }
                    // Only 1 player left
                    if (PlayerCount() == 1)
                    {
                        ChangeState(MatchState.END);
                        break;
                    }

                    ServerSender.StartRound(currentRound);
                    _currentPlayer = GameManager.DetermineNextPlayer();

                    // Send order to all player
                    ServerSender.UpdatePlayerOrder();

                    // Send Question to all player
                    ServerSender.SendQuestion(quizList[curQuiz]);

                    SetTimer(3, () =>
                    {
                        ChangeState(MatchState.WAIT_ANSWER);
                    }, true);
                    break;

                case MatchState.WAIT_ANSWER:
                    _isAnswered = false;
                    SetTimer(Constants.TIME_PER_ROUND + 5, () => { });
                    break;

                case MatchState.VERIFY_ANSWER:
                    // Send Answer to all player
                    ServerSender.SendAnswer(quizList[curQuiz].answer);
                    ChangeState(MatchState.END_ROUND); 

                    break;

                case MatchState.SKIP_QUIZ:
                    ServerSender.SkipQuiz(_currentPlayer);
                    _currentPlayer = GameManager.DetermineNextPlayer();
                    // Send order to all player
                    ServerSender.UpdatePlayerOrder();

                    SetTimer(3, () =>
                    {
                        ChangeState(MatchState.WAIT_ANSWER);
                    }, true);
                    break;

                case MatchState.END_ROUND:
                    // Send order to all player
                    ServerSender.UpdatePlayerOrder();
                    ServerSender.EndRound(currentRound);
                    ChangeState(MatchState.START_ROUND);
                    break;

                case MatchState.END:
                    GameManager.EndGame();
                    break;
            }
        }
        public static QuizQuestion[] LoadQuestions(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);
            QuizQuestion[] questions = JsonSerializer.Deserialize<QuizQuestion[]>(jsonString);
            Random random = new Random();
            for (int i = questions.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                QuizQuestion tmp = questions[j];
                questions[j] = questions[i];
                questions[i] = tmp;
            }
            return questions;
        }
    }
}
