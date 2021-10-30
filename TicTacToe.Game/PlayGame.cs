using System.Threading.Tasks;

namespace TicTacToe.Game
{
    /// <summary>
    /// This manages the game flow, requesting players to make a move and requesting the state to update
    /// </summary>
    public abstract class PlayGame
    {
        protected abstract void GameEnded(ResultState state);
        protected abstract void RedrawBoard(Board board);
        protected abstract void IllegalMove(int move);
        
        public GameState? GameState { get; private set; }
        public async Task StartGameAsync(int boardSize, IPlayer player1, IPlayer player2)
        {
            GameState = new GameState(boardSize, player1, player2);
            var gameplay = new ResultChecker(boardSize);
            
            RedrawBoard(GameState.Board);

            while (true)
            {
                var result = gameplay.GetResultState(GameState);
                if (result != ResultState.InProgress)
                {
                    GameEnded(result);
                    GameState.GameEnded(result);
                    break;
                }

                var move = await GameState.PlayersTurn.MakeMove(GameState);
                try
                {
                    GameState.ApplyMove(move);
                    RedrawBoard(GameState.Board);
                }
                catch (IllegalMoveException)
                {
                    IllegalMove(move);
                }
            }
        }
    }
}