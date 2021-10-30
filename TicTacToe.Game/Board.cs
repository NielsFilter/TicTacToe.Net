using System;
using System.Collections.Generic;
using System.Linq;

namespace TicTacToe.Game
{
    public class Board
    {
        public int Size { get; }
        public int[] Positions { get; private init; }
        public List<int> AvailablePositions { get; private set; } = new();
        
        public Board(int boardSize)
        {
            if (boardSize is < 3 or > 5)
            {
                boardSize = 3;
            }

            Size = boardSize;
            Positions = new int[(int) Math.Pow(Size, 2)];
            ResetBoard();
        }

        private void ResetBoard()
        {
            AvailablePositions = Enumerable.Range(0, Positions.Length).ToList();
            for (var posIndex = 0; posIndex < Positions.Length; posIndex++)
            {
                // set all positions to 0 (unused space)
                Positions[posIndex] = 0;
            }
        }

        public void ApplyMove(int move, int player)
        {
            // update the board to reflect player's move
            Positions[move] = player;
            AvailablePositions.Remove(move);
        }

        public string GetState()
        {
            return string.Join("", Positions);
        }

        #region Clone
        
        /// <summary>
        /// Some bots may require copies of the board to calculate or predict best move.
        /// Use Clone as a way to get a copy of the current board state without mutating the existing one.
        /// </summary>
        /// <returns>Deep copy of the current board state</returns>
        public Board Clone()
        {
            var clonedBoard = new Board(Size)
            {
                Positions = Positions.ToArray(),
                AvailablePositions = new List<int>(AvailablePositions)
            };

            return clonedBoard;
        }

        #endregion
    }
}