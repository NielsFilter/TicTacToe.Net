using System;
using TicTacToe.Game;

namespace TicTacToe.UI.Web.Services
{
    public class WebAppGame : PlayGame
    {
        private readonly Action<Board> _redrawBoardAction;
        private readonly Action<ResultState> _gameEndedAction;

        public WebAppGame(Action<Board> redrawBoardAction, Action<ResultState> gameEndedAction)
        {
            _redrawBoardAction = redrawBoardAction;
            _gameEndedAction = gameEndedAction;
        }

        protected override void GameEnded(ResultState state) => _gameEndedAction(state);
        protected override void RedrawBoard(Board board) => _redrawBoardAction(board);
        protected override void IllegalMove(int move) { }
    }
}