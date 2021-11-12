using System;
using System.Linq;
using System.Threading.Tasks;
using TicTacToe.Game;

namespace TicTacToe.Bots
{
    public class MiniMaxBot : IPlayer
    {
        private readonly int? _turnDelay;
        private readonly ResultChecker _resultChecker;
        private const double WEIGHT = 1d;

        public MiniMaxBot(int boardSize = 3, int? turnDelay = null)
        {
            _turnDelay = turnDelay;
            _resultChecker = new ResultChecker(boardSize);
        }

        public PlayerTypes Type => PlayerTypes.MiniMax;

        public virtual async Task<int> MakeMove(GameState state)
        {
            await Task.Yield();

            var myNumber = state.GetCurrentPlayersNumber();

            var clonedGameState = state.Clone();

            var topMove = await ChooseNextBestMove(myNumber, clonedGameState);

            if (_turnDelay is > 0)
            {
                await Task.Delay(_turnDelay.Value);
            }

            return topMove;
        }

        public void GameEnded(GameState state, ResultState result, int myNumber) { }

        private Task<int> ChooseNextBestMove(int myNumber, GameState state)
        {
            var availableMoves = state.Board.AvailablePositions;
            var topMove = availableMoves.First();
            double? bestMove = null;

            foreach (var move in availableMoves)
            {
                var score = GetMinMaxScoreForMovesRecursive(myNumber, move, state.Clone(), 1);
                if (state.GetCurrentPlayersNumber() == myNumber)
                {
                    if (bestMove == null || score > bestMove.Value)
                    {
                        bestMove = score;
                        topMove = move;
                    }
                }
            }

            if (bestMove == null)
            {
                throw new InvalidOperationException("Something went wrong");
            }
            return Task.FromResult(topMove);
        }

        private double GetMinMaxScoreForMovesRecursive(int myNumber, int move, GameState state, int depth)
        {
            state.ApplyMove(move);
            var result = _resultChecker.GetResult(state);
            switch (result)
            {
                case ResultState.Draw:
                    return Draw();
                case ResultState.Player1Win:
                    return CheckWinOrLose(myNumber, Constants.PLAYER_1, depth);
                case ResultState.Player2Win:
                    return CheckWinOrLose(myNumber, Constants.PLAYER_2, depth);
            }

            var nextAvailableMoves = state.Board.AvailablePositions;
            depth++;

            var scores = new double[nextAvailableMoves.Count];
            var isMaximisingPlayerTurn = state.GetCurrentPlayersNumber() == myNumber;


            for (var i = 0; i < nextAvailableMoves.Count; i++)
            {
                var score = GetMinMaxScoreForMovesRecursive(myNumber, nextAvailableMoves[i], state.Clone(), depth);
                scores[i] = score;
            }

            return isMaximisingPlayerTurn ? scores.Max() : scores.Min();
        }

        private double CheckWinOrLose(int myNumber, int winningPlayer, int depth)
        {
            return myNumber == winningPlayer
                ? Win(depth)
                : Lose(depth);
        }

        private double Draw() => 0;
        private double Lose(int depth) => -10 + (depth * WEIGHT);
        private double Win(int depth) => 10 - (depth * WEIGHT);
    }
}