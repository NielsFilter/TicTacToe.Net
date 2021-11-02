using System.Collections.Generic;
using System.Linq;

namespace TicTacToe.Game
{
    public class GameState
    {
        private readonly IPlayer _player1;
        private readonly IPlayer _player2;
        
        public IPlayer PlayersTurn { get; private set; }
        public List<string> AllStates { get; private init; }
        public Board Board { get; private init; }
        
        public GameState(
            int boardSize,
            IPlayer player1,
            IPlayer player2)
        {
            _player1 = player1;
            _player2 = player2;
            
            PlayersTurn = player1;
            Board = new Board(boardSize);
            AllStates = new List<string>
            {
                Board.GetState()
            };
        }

        /// <summary>
        /// A move was made, let's apply the move and update the game state accordingly
        /// </summary>
        public void ApplyMove(int move)
        {
            var availableSpots = Board.AvailablePositions;
            if (availableSpots.All(x => x != move))
            {
                throw new IllegalMoveException();
            }
            
            // update the board to reflect the move made
            CaptureBoardState(move);
            
            // update player's turn
            UpdatePlayersTurn();
        }

        private void CaptureBoardState(int move)
        {
            var playerNumber = GetCurrentPlayersNumber();
            Board.ApplyMove(move, playerNumber);
            AllStates.Add(Board.GetState());
        }

        public void GameEnded(ResultState result)
        {
            // Calling GameEnded on the players themselves, allows opportunity to train based on how the game concluded
            _player1.GameEnded(this, result, Constants.PLAYER_1);
            _player2.GameEnded(this, result, Constants.PLAYER_2);
        }
        
        private void UpdatePlayersTurn() =>
            PlayersTurn =
                PlayersTurn == _player1
                    ? _player2
                    : _player1;

        public int GetCurrentPlayersNumber() =>
            PlayersTurn == _player1
                ? Constants.PLAYER_1
                : Constants.PLAYER_2;
        
        public string GetCurrentPlayersSymbol() =>
            PlayersTurn == _player1
                ? Constants.PLAYER_1_SYMBOL
                : Constants.PLAYER_2_SYMBOL;
        
        #region Clone
        
        /// <summary>
        /// Some bots may require copies of the game state to calculate or predict best move
        /// Use Clone as a way to get a copy of the current state without mutating the existing one.
        /// </summary>
        /// <returns>Deep copy of the current game state</returns>
        public GameState Clone()
        {
            var clonedState = new GameState(Board.Size, _player1, _player2)
            {
                PlayersTurn = PlayersTurn,
                AllStates = new List<string>(AllStates),
                Board = Board.Clone()
            };

            return clonedState;
        }
        
        #endregion
    }
}