using System;
using System.Linq;
using System.Threading.Tasks;
using TicTacToe.Game;

namespace TicTacToe.Bots
{
    public class RandomMoveBot : IPlayer
    {
        private readonly int? _turnDelay;

        public RandomMoveBot(int? turnDelay = null)
        {
            _turnDelay = turnDelay;
        }

        public PlayerTypes Type => PlayerTypes.RandomBot;

        public async Task<int> MakeMove(GameState state)
        {
            var allAvailableMoves = state.Board.AvailablePositions;
            var rnd = new Random();
            var randomIndex = rnd.Next(0, allAvailableMoves.Count - 1);
            
            if (_turnDelay is > 0)
            {
                // This delay is nice since the bot plays to quick after a human, it's easy to miss their move 
                await Task.Delay(_turnDelay.Value);   
            }
            
            return allAvailableMoves.ElementAt(randomIndex);
        }

        public void GameEnded(GameState state, ResultState result, int myNumber) { }
    }
}