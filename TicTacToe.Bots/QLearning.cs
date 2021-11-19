using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TicTacToe.Game;

namespace TicTacToe.Bots
{
    public class QLearningBot : IPlayer
    {
        private const double LEARNING_RATE = 0.2d; // alpha
        private const double DISCOUNT_RATE = 0.9d; // decay gamma
        
        protected readonly Dictionary<string, double> Policy;
        private readonly int _minExplorationRate;
        private readonly int _decreaseExplorationAfter;
        private readonly bool _shouldTrain;
        private readonly string _botName;
        private readonly int _decreaseExplorationCounter;
        private int _gamesPlayed;
        private int _explorationRate; // epsilon greedy

        public QLearningBot(
            string? policyFilePath = null,
            bool shouldTrain = false,
            int explorationRate = 30,
            int minExplorationRate = 5,
            int decreaseExplorationAfter = 1000,
            int decreaseExplorationCounter = 1,
            string customName = "",
            Dictionary<string, double>? predefinedPolicy = null)
        {
            _shouldTrain = shouldTrain;
            Policy = LoadPolicy(policyFilePath, predefinedPolicy);
            _explorationRate = explorationRate;
            _minExplorationRate = minExplorationRate;
            _decreaseExplorationAfter = decreaseExplorationAfter;
            _decreaseExplorationCounter = decreaseExplorationCounter;
            _botName = string.IsNullOrWhiteSpace(customName)
                ? Type.ToString()
                : customName;
        }

        public string GetName() => _botName;

        public PlayerTypes Type => PlayerTypes.QLearning;

        public Task<int> MakeMove(GameState state)
        {
            var myNumber = state.GetCurrentPlayersNumber();
            var rnd = new Random();
            if (rnd.Next(1, 100) <= _explorationRate)
            {
                // explore 
                var allAvailableMoves = state.Board.AvailablePositions;
                var randomIndex = rnd.Next(0, allAvailableMoves.Count - 1);
                return Task.FromResult(allAvailableMoves.ElementAt(randomIndex));   
            }

            // exploit
            var topScore = double.MinValue;
            var topSpot = -1;
            var boardState = state.Board.Positions.ToArray();
            foreach (var spot in state.Board.AvailablePositions)
            {
                int[] copiedBoardState = boardState.ToArray();
                copiedBoardState[spot] = myNumber;
                var nextMoveState = $"{myNumber}{string.Join("", copiedBoardState)}";
                if (!Policy.TryGetValue(nextMoveState, out var score))
                {
                    score = 0;
                }

                if (score > topScore)
                {
                    topScore = score;
                    topSpot = spot;
                }
            }

            return Task.FromResult(topSpot);
        }

        public void GameEnded(GameState state, ResultState result, int myNumber)
        {
            if (_shouldTrain)
            {
                GiveRewardsForGame(state, result, myNumber);   
            }

            PolicyUpdatedAsync(CancellationToken.None);
        }

        protected virtual Task PolicyUpdatedAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void GiveRewardsForGame(GameState state, ResultState result, int myNumber)
        {
            var opponentNumber = myNumber == Constants.PLAYER_1
                ? Constants.PLAYER_2
                : Constants.PLAYER_1;
            
            var startedFirst = myNumber == Constants.PLAYER_1;

            int myReward;
            int opponentReward;
            if (result == ResultState.Draw)
            {
                //draw       
                myReward = startedFirst ? 20 : 50;
                opponentReward = startedFirst ? 50 : 20;
            }
            else
            {
                var isWin =
                    result == ResultState.Player1Win && myNumber == Constants.PLAYER_1 ||
                    result == ResultState.Player2Win && myNumber == Constants.PLAYER_2;

                if (isWin)
                {
                    // win
                    myReward = 100;
                    opponentReward = -100;
                }
                else
                {
                    // lose
                    myReward = -100;
                    opponentReward = 100;
                }
            }

            FeedReward(state.AllStates, myReward, myNumber);
            FeedReward(state.AllStates, opponentReward, opponentNumber);
            
            AdjustExplorationRate();
        }

        private void AdjustExplorationRate()
        {
            if(_decreaseExplorationAfter <= 0)
            {
                return;
            }

            // The more trained qLearn bot becomes, the more exploitative it becomes
            _gamesPlayed++;
            if (_gamesPlayed % _decreaseExplorationAfter == _decreaseExplorationCounter)
            {
                _explorationRate -= 1;
                if (_explorationRate < _minExplorationRate)
                {
                    _explorationRate = _minExplorationRate;
                }
            }
        }

        private Dictionary<string, double> LoadPolicy(string? policyFilePath, Dictionary<string, double>? predefinedPolicy)
        {
            if (string.IsNullOrWhiteSpace(policyFilePath))
            {
                return predefinedPolicy ?? new Dictionary<string, double>();
            }
            
            try
            {
                var policyStr = File.ReadAllText(policyFilePath);
                if (string.IsNullOrWhiteSpace(policyStr))
                {
                    return new Dictionary<string, double>();
                }
                
                var loadedPolicy = JsonConvert.DeserializeObject<Dictionary<string, double>>(policyStr);
                if (loadedPolicy == null)
                {
                    return new Dictionary<string, double>();
                }
                
                return loadedPolicy;
            }
            catch (Exception)
            {
                return new Dictionary<string, double>();
            }
        }

        public async Task SavePolicyAsync(string policyFilePath)
        {
           var serializedPolicy = JsonConvert.SerializeObject(Policy);
           await File.WriteAllTextAsync(policyFilePath, serializedPolicy);
        }

        private void FeedReward(List<string> allStates, int reward, int number)
        {
            for (var i = allStates.Count - 1; i >= 0; i--)
            {
                var state = $"{number}{allStates[i]}";
                if (!Policy.ContainsKey(state))
                {
                    Policy[state] = 0;
                }
                var qScore = Policy[state] + LEARNING_RATE * (reward * DISCOUNT_RATE - Policy[state]);
                Policy[state] = qScore;
            }
        }
    }
}