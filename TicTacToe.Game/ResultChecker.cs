using System.Collections.Generic;
using System.Linq;

namespace TicTacToe.Game
{
    public class ResultChecker
    {
        private readonly List<int[]> _rowIndexes;
        private readonly List<int[]> _colIndexes;
        private readonly List<int[]> _diagIndexes;

        public ResultChecker(int boardSize)
        {
            if (boardSize == 0)
            {
                boardSize = 3;
            }

            // Well this is where things get a touch awkward. I went with spots being ints rather than tuples.
            // I found it easier to work with in most cases and more memory efficient. But as  most decisions,
            // you gain in one place and pay for it in another. This is where we have to pay back a bit.
            //
            // Let's break up the int array into arrays representing rows cols & diags. This way it makes it easy to
            // Check for a winner as you can just check if all in that array are the same player.

            _rowIndexes = new List<int[]>();
            _colIndexes = new List<int[]>();
            _diagIndexes = new List<int[]>();

            var forwardSlashDiag = new int[boardSize];
            var backSlashDiag = new int[boardSize];
            for (var i = 0; i < boardSize; i++)
            {
                var row = new int[boardSize];
                var col = new int[boardSize];
                for (var j = 0; j < boardSize; j++)
                {
                    // e.g. with board size of 3, the first row & cols will look like this:
                    // row: [i: 0] => 0,1,2
                    // col: [i: 0] => 0,3,6
                    row[j] = (i * boardSize) + j;
                    col[j] = (j * boardSize) + i;
                }

                // e.g. with board size of 3
                // back slash(\): 0,4,8
                // forward slash(/): 6,4,2 
                backSlashDiag[i] = i * boardSize + i;
                forwardSlashDiag[i] = (i * boardSize) + (boardSize - 1) - i;

                _colIndexes.Add(col);
                _rowIndexes.Add(row);
            }

            _diagIndexes.Add(backSlashDiag);
            _diagIndexes.Add(forwardSlashDiag);
        }
        
        public ResultState GetResult(GameState gameState)
        {
            // Let's check the rows, columns and diagonals for a winner.
            var resultState = CheckRowsForWinner(gameState); 
            if (resultState != ResultState.InProgress)
            {
                return resultState;
            }

            resultState = CheckColsForWinner(gameState);
            if (resultState != ResultState.InProgress)
            {
                return resultState;
            }

            resultState = CheckDiagsForWinner(gameState);
            if (resultState != ResultState.InProgress)
            {
                return resultState;
            }
            
            // Checked cols, rows & diags and nobody has won.
            // If all spots are occupied the it's a draw, if not, then game is still on 
            return !gameState.Board.AvailablePositions.Any()
                ? ResultState.Draw
                : ResultState.InProgress;
        }

        /// <summary>
        /// Check all the diagonals and if all the diagonals are occupied by the same player number, return a win for that player
        /// </summary>
        /// <returns><see cref="ResultState.Player1Win"/> or <see cref="ResultState.Player2Win"/> if someone has won.
        /// Otherwise returns <see cref="ResultState.InProgress"/></returns>
        private ResultState CheckDiagsForWinner(GameState gameState)
        {
            foreach (var diag in _diagIndexes)
            {
                if (diag.All(x => gameState.Board.Positions[x] == Constants.PLAYER_1))
                {
                    return ResultState.Player1Win;
                }

                if (diag.All(x => gameState.Board.Positions[x] == Constants.PLAYER_2))
                {
                    return ResultState.Player2Win;
                }
            }

            return ResultState.InProgress;
        }

        /// <summary>
        /// Check all the cols and if all the cols are occupied by the same player number, return a win for that player
        /// </summary>
        /// <returns><see cref="ResultState.Player1Win"/> or <see cref="ResultState.Player2Win"/> if someone has won.
        /// Otherwise returns <see cref="ResultState.InProgress"/></returns>
        private ResultState CheckColsForWinner(GameState gameState)
        {
            foreach (var col in _colIndexes)
            {
                if (col.All(x => gameState.Board.Positions[x] == Constants.PLAYER_1))
                {
                    return ResultState.Player1Win;
                }

                if (col.All(x => gameState.Board.Positions[x] == Constants.PLAYER_2))
                {
                    return ResultState.Player2Win;
                }
            }

            return ResultState.InProgress;
        }
        
        /// <summary>
        /// Check all the rows and if all the rows are occupied by the same player number, return a win for that player
        /// </summary>
        /// <returns><see cref="ResultState.Player1Win"/> or <see cref="ResultState.Player2Win"/> if someone has won.
        /// Otherwise returns <see cref="ResultState.InProgress"/></returns>
        private ResultState CheckRowsForWinner(GameState gameState)
        {
            foreach (var row in _rowIndexes)
            {
                if (row.All(x => gameState.Board.Positions[x] == Constants.PLAYER_1))
                {
                    return ResultState.Player1Win;
                }

                if (row.All(x => gameState.Board.Positions[x] == Constants.PLAYER_2))
                {
                    return ResultState.Player2Win;
                }
            }

            return ResultState.InProgress;
        }
    }
}