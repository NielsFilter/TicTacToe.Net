using System;
using TicTacToe.Game;

namespace TicTacToe.UI.CommandLine
{
    public class ConsolePlayGame : PlayGame
    {
        public int Winner;

        protected override void GameEnded(ResultState state)
        {
            if (state == ResultState.Draw)
            {
                Winner = 0;
                PrintLine("Game ended in a Draw");   
            }
            else if (state == ResultState.Player1Win)
            {
                Winner = 1;
                PrintLine("Player 1 wins");
            }
            else if (state == ResultState.Player2Win)
            {
                Winner = 2;
                PrintLine("Player 2 wins");
            }
            else
            {
                throw new InvalidOperationException("Game can't be done and in progress. Something went wrong...");
            }
        }

        protected override void RedrawBoard(Board board)
        {
            Console.Clear();
            var defaultColor = Console.ForegroundColor;
            try
            {
                int posIndex = 0;
                for (var rowIndex = 0; rowIndex < board.Size; rowIndex++)
                {
                    for (var colIndex = 0; colIndex < board.Size; colIndex++)
                    {
                        var value = board.Positions[posIndex];
                        string printValue;
                        if (value == Constants.PLAYER_1)
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            printValue = Constants.PLAYER_1_SYMBOL;
                        }
                        else if (value == Constants.PLAYER_2)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            printValue = Constants.PLAYER_2_SYMBOL;
                        }
                        else
                        {
                            // Nothing picked on that spot yet
                            printValue = " ";
                        }

                        Print($"{printValue}");
                        Console.ForegroundColor = defaultColor;
                        
                        if (colIndex < board.Size - 1)
                        {
                            Print("|");
                        }

                        posIndex++;
                    }

                    Print(Environment.NewLine);
                    if (rowIndex < board.Size - 1)
                    {
                        PrintLine(new string('-', board.Size + board.Size - 1));
                    }
                }
            }
            finally
            {
                // set the color back again
                Console.ForegroundColor = defaultColor;
            }
        }

        private void PrintLine(string message)
        {
            Console.WriteLine(message);
        }

        private void Print(string message)
        {
            Console.Write(message);
        }

        protected override void IllegalMove(int move)
        {
            Console.WriteLine($"Move {move} is not legal. Try again");
        }
    }
}