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
        public static int currentRound = 0;
        public static int currentTurn = 0;

        private static Dictionary<string, string> _questions;
        private static HashSet<string> _pickedQuestions;
        private static MatchState _matchState = MatchState.END;
        private static float _waitTimer = 1;
        private static bool _isWaiting = false;
        private static bool _isLocked = false;
        private static bool _isKeywordRevealed = false;
        private static string _currentKeyword = "";
        private static Player _currentPlayer;
        private static bool _isAnswered = false;

        private static int _highestOrder = 0;
        private static KeyValuePair<string, string> _currentQuestion;
        private static Action _timerCallback;
        private static HashSet<char> _unlockedCharacters;
        private static DateTime _lastTick = DateTime.Now;

        public static void Start()
        {
            _questions = new Dictionary<string, string>();
            _unlockedCharacters = new HashSet<char>();
            _pickedQuestions = new HashSet<string>();
            _pickedQuestions.Add("dummy");
            currentRound = 0;
            ChangeState(MatchState.INIT);
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
                case MatchState.INIT:
                    break;

                case MatchState.BEGIN_TURN:
                    break;

                case MatchState.START_ROUND:
                    break;

                case MatchState.DETERMINE_NEXT_PLAYER:
                    break;

                case MatchState.WAIT_ANSWER:
                    if (_waitTimer <= 0 && !_isAnswered) HandleAnswer(_currentPlayer.ID, "", "");
                    break;

                case MatchState.VERIFY_ANSWER:
                    break;

                case MatchState.END_TURN:
                    break;

                case MatchState.END_ROUND:
                    break;

                case MatchState.END:
                    break;
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

                case MatchState.BEGIN_TURN:
                    break;

                case MatchState.DETERMINE_NEXT_PLAYER:
                    break;

                case MatchState.WAIT_ANSWER:
                    break;

                case MatchState.VERIFY_ANSWER:
                    break;

                case MatchState.END_TURN:
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
                    // Load the data
                    string currentDirectory = Directory.GetCurrentDirectory();
                    var path = currentDirectory + @"\database.txt";
                    string[] lines = System.IO.File.ReadAllLines(path);
                    int questionCount = int.Parse(lines[0]);
                    for (int i = 1; i <= questionCount; ++i)
                    {
                        string keyword = lines[2 * i - 1];
                        string clue = lines[2 * i];
                        _questions.Add(keyword.ToUpper(), clue);
                    }
                    ServerSender.SetupGame();
                    SetTimer(5, () => ChangeState(MatchState.START_ROUND), true);

                    break;

                case MatchState.START_ROUND:
                    // Choose a random question
                    _currentQuestion = new KeyValuePair<string, string>("dummy", "nothing");
                    Random random = new Random();
                    while (_pickedQuestions.Contains(_currentQuestion.Key))
                    {
                        int index = random.Next(_questions.Count);
                        _currentQuestion = _questions.ElementAt(index);
                    }
                    Console.WriteLine("Keyword for this round: " + _currentQuestion.Key);
                    _pickedQuestions.Add(_currentQuestion.Key);
                    _currentKeyword = new string('_', _currentQuestion.Key.Length);
                    _unlockedCharacters.Clear();

                    _isKeywordRevealed = false;
                    currentTurn = 0;
                    currentRound++;

                    GameManager.ResetPlayersForNextRound();

                    // Recalculate match score of each player
                    ServerSender.UpdateRoundInfo(_currentKeyword, currentRound, _unlockedCharacters);

                    // Show the question to the players
                    ServerSender.StartRound(_currentQuestion, _currentKeyword, currentRound);
                    SetTimer(5, () =>
                    {
                        ChangeState(MatchState.BEGIN_TURN);
                    }, true);
                    break;

                case MatchState.BEGIN_TURN:
                    currentTurn++;
                    _highestOrder = 0;
                    ServerSender.StartTurn(currentTurn);
                    SetTimer(3, () =>
                    {
                        ChangeState(MatchState.DETERMINE_NEXT_PLAYER);
                    }, true);
                    break;

                case MatchState.DETERMINE_NEXT_PLAYER:
                    _currentPlayer = GameManager.DetermineNextPlayer(_highestOrder);
                    if (_currentPlayer != null)
                    {
                        _highestOrder = _currentPlayer.order;
                        ServerSender.WaitForNextPlayer();
                        SetTimer(2, () =>
                        {
                            ServerSender.PickNextPlayer(_currentPlayer);
                            ChangeState(MatchState.WAIT_ANSWER);
                        }, true);
                    }
                    else
                    {
                        ChangeState(MatchState.END_TURN);
                    }

                    break;

                case MatchState.WAIT_ANSWER:
                    _isAnswered = false;
                    SetTimer(Constants.TIME_PER_ROUND + 5, () => { });
                    break;

                case MatchState.VERIFY_ANSWER:
                    // Recalculate match score of each player
                    ServerSender.UpdateRoundInfo(_currentKeyword, currentRound, _unlockedCharacters);

                    SetTimer(5, () =>
                    {
                        if (_isKeywordRevealed) ChangeState(MatchState.END_ROUND);
                        else ChangeState(MatchState.DETERMINE_NEXT_PLAYER);
                    }, true);
                    break;

                case MatchState.END_TURN:
                    ServerSender.UpdateRoundInfo(_currentKeyword, currentRound, _unlockedCharacters);
                    ServerSender.EndTurn(currentTurn);
                    if (currentTurn < Constants.MAX_TURN)
                    {
                        SetTimer(3, () =>
                        {
                            ChangeState(MatchState.BEGIN_TURN);
                        }, true);
                    }
                    else
                    {
                        CompleteKeyword();
                        ChangeState(MatchState.END_ROUND);
                    }

                    break;

                case MatchState.END_ROUND:
                    ServerSender.UpdateRoundInfo(_currentKeyword, currentRound, _unlockedCharacters);
                    ServerSender.EndRound(currentRound);
                    if (currentRound < Constants.MAX_ROUND)
                    {
                        SetTimer(3, () => ChangeState(MatchState.START_ROUND), true);
                    }
                    else ChangeState(MatchState.END);

                    break;

                case MatchState.END:
                    GameManager.EndGame();
                    break;
            }
        }

        public static void HandleAnswer(int clientId, string character, string keyword)
        {
            if (_matchState != MatchState.WAIT_ANSWER) return;

            bool correctCharacter = true;
            Console.WriteLine($"Player {clientId} guessed character $${character}$$");
            char c = character[0];
            if (c == '_') correctCharacter = false;
            if (!_currentQuestion.Key.Contains(c)) correctCharacter = false;

            if (char.IsAsciiLetterOrDigit(c)) _unlockedCharacters.Add(c);

            if (correctCharacter)
            {
                _currentPlayer.AddScore(Constants.SCORE_CHARACTER_CORRECT);
                UpdateKeyword();
            }
            else
            {
                _currentPlayer.AddScore(Constants.SCORE_CHARACTER_WRONG);
            }

            bool guessedKeyword = keyword != "_";
            bool correctKeyword = true;
            bool isDisqualified = false;

            if (string.Compare(keyword.ToUpper(), _currentQuestion.Key.ToUpper()) != 0) correctKeyword = false;

            if (correctKeyword)
            {
                _currentPlayer.AddScore(Constants.SCORE_KEYWORD_CORRECT);
                CompleteKeyword();
            }
            else
            {
                if (guessedKeyword)
                {
                    _currentPlayer.AddScore(Constants.SCORE_KEYWORD_WRONG);
                    _currentPlayer.isDisqualified = true;
                    isDisqualified = true;
                }
            }

            _isAnswered = true;
            ServerSender.VerifyAnswer(_currentPlayer, correctCharacter, character, correctKeyword, keyword, isDisqualified);
            ChangeState(MatchState.VERIFY_ANSWER);
        }

        private static void UpdateKeyword()
        {
            string newKeyword = "";
            bool fullyUnlocked = true;
            for (int i = 0; i < _currentQuestion.Key.Length; ++i)
            {
                if (_unlockedCharacters.Contains(_currentQuestion.Key[i])) newKeyword += _currentQuestion.Key[i];
                else
                {
                    newKeyword += "_";
                    fullyUnlocked = false;
                }
            }
            _currentKeyword = newKeyword;
            if (fullyUnlocked) CompleteKeyword();
        }

        private static void CompleteKeyword()
        {
            _isKeywordRevealed = true;
            _currentKeyword = _currentQuestion.Key;
        }
    }
}
