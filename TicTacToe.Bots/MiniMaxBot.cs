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
            
            var topMove = availableMoves.First();
            double? bestMove = null;

            // go through all available moves and get a minmax score for each. Then chose the best move.
            foreach (var move in availableMoves)
            {
                var score = GetMinMaxScoreForMovesRecursive(myNumber, move, clonedState.Clone(), 1);
                if (score <= bestMove)
                {
                    continue;
                }

                // new best move. Capture the move and the score
                bestMove = score;
                topMove = move;
            }

            return topMove;
        }

        private double GetMinMaxScoreForMovesRecursive(int myNumber, int move, GameState state, int depth)
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
            var scores = new double[nextAvailableMoves.Count];
            
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
        private static double DrawScore() => 0;
        private static double LoseScore(int depth) => -10 + depth;
        private static double WinScore(int depth) => 10 - depth;
        
        private static double WinOrLoseScore(int myNumber, int winningPlayer, int depth)
        {
            return myNumber == winningPlayer
                ? WinScore(depth)
                : LoseScore(depth);
        }
    }
}