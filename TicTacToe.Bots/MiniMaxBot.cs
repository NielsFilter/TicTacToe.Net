using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicTacToe.Game;

namespace TicTacToe.Bots
{
    public class MiniMaxBot : IPlayer
    {
        private readonly ResultChecker _resultChecker;

        public MiniMaxBot(int boardSize = 3)
        {
            _resultChecker = new ResultChecker(boardSize);
        }

        public PlayerTypes Type => PlayerTypes.MiniMax;

        public virtual async Task<int> MakeMove(GameState state)
        {
            await Task.Yield();

            var myNumber = state.GetCurrentPlayersNumber();
            var clonedState = state.Clone();
            var availableMoves = clonedState.Board.AvailablePositions;
            
            var scores = new Dictionary<int, int>();
            var topScore = int.MinValue;
            // go through all available moves and get a minmax score for each. Then chose the best move.
            foreach (var move in availableMoves)
            {
                var score = GetMinMaxScoreForMovesRecursive(myNumber, move, clonedState.Clone(), 1);
                scores[move] = score;
                
                if (score > topScore)
                {
                    topScore = score;
                }
            }

            // This isn't needed but to keep things interesting, if there are multiple spots with top scores, switch things
            // up by randomly choosing one rather than just the first each time.  
            var topMoves = scores
                .Where(x => x.Value == topScore)
                .Select(x=>x.Key)
                .ToArray();

            if (topMoves.Length == 1)
            {
                return topMoves[0];
            }
            
            var rndIndex = new Random().Next(0, topMoves.Length - 1);
            return topMoves[rndIndex];
        }

        private int GetMinMaxScoreForMovesRecursive(int myNumber, int move, GameState state, int depth)
        {
            // make the move on the cloned board
            state.ApplyMove(move);
            
            // check if the game is over. If it is, return the score
            var result = _resultChecker.GetResult(state);
            switch (result)
            {
                case ResultState.Draw:
                    return DrawScore();
                case ResultState.Player1Win:
                    return WinOrLoseScore(myNumber, Constants.PLAYER_1, depth);
                case ResultState.Player2Win:
                    return WinOrLoseScore(myNumber, Constants.PLAYER_2, depth);
            }

            // The game is not done yet. Now we'll look at all the next available spots and check all the possible scores.
            // We'll continue to do so recursively until we get a result for each position
            
            depth++;
            var nextAvailableMoves = state.Board.AvailablePositions;
            var scores = new int[nextAvailableMoves.Count];
            
            for (var i = 0; i < nextAvailableMoves.Count; i++)
            {
                var score = GetMinMaxScoreForMovesRecursive(myNumber, nextAvailableMoves[i], state.Clone(), depth);
                scores[i] = score;
            }

            // here we check if it's the maximizing player (minimax bot) turn. If so, pick the best score.
            // if it's not (i.e. the opponent's turn), assume that they will choose the best possible option (lowest score)
            var isMyTurn = state.GetCurrentPlayersNumber() == myNumber;
            return isMyTurn ? scores.Max() : scores.Min();
        }

        // Scoring:
        //  - Draw is meh... 0 points
        //  - A Loss is bad: -10. But a loss much later in the game is better than an immediate loss, so we add depth
        //  - A Win is great: +10. An immediate win trumps a later win. So we subtract the depth from the win score
        private static int DrawScore() => 0;
        private static int LoseScore(int depth) => -100 + depth;
        private static int WinScore(int depth) => 100 - depth;
        
        private static int WinOrLoseScore(int myNumber, int winningPlayer, int depth)
        {
            return myNumber == winningPlayer
                ? WinScore(depth)
                : LoseScore(depth);
        }
    }
}