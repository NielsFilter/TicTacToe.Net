using System;
using TicTacToe.Game;

namespace TicTacToe.UI.CommandLine
{
    public class ConsolePlayGame : PlayGame
    {
        private readonly bool _shouldDraw;

        public ConsolePlayGame(bool shouldDraw = true)
        {
            _shouldDraw = shouldDraw;
        }
        
        public int Winner;

        protected override void GameEnded(ResultState state)
        {
            switch (state)
            {
                case ResultState.Draw:
                    Winner = 0;
                    PrintLine("Game ended in a Draw");
                    break;
                case ResultState.Player1Win:
                    Winner = 1;
                    PrintLine("Player 1 wins");
                    break;
                case ResultState.Player2Win:
                    Winner = 2;
                    PrintLine("Player 2 wins");
                    break;
                default:
                    throw new InvalidOperationException("Game can't be done and in progress. Something went wrong...");
            }
        }

        protected override void RedrawBoard(Board board)
        {
            if (!_shouldDraw)
            {
                return;
            }
            
            ClearConsole();
            var defaultColor = Console.ForegroundColor;
            try
            {
                var posIndex = 0;
                for (var rowIndex = 0; rowIndex < board.Size; rowIndex++)
                {
                    for (var colIndex = 0; colIndex < board.Size; colIndex++)
                    {
                        var value = board.Positions[posIndex];
                        string printValue;
                        switch (value)
                        {
                            case Constants.PLAYER_1:
                                Console.ForegroundColor = ConsoleColor.Magenta;
                                printValue = Constants.PLAYER_1_SYMBOL;
                                break;
                            case Constants.PLAYER_2:
                                Console.ForegroundColor = ConsoleColor.Green;
                                printValue = Constants.PLAYER_2_SYMBOL;
                                break;
                            default:
                                // Nothing picked on that spot yet
                                printValue = " ";
                                break;
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

        protected override void IllegalMove(int move)
        {
            PrintLine($"Move {move} is not legal. Try again");
        }

        private void PrintLine(string message)
        {
            if (_shouldDraw)
            {
                Console.WriteLine(message);   
            }
        }

        private void Print(string message)
        {
            if (_shouldDraw)
            {
                Console.Write(message);   
            }
        }

        private void ClearConsole()
        {
            if (_shouldDraw)
            {
                Console.Clear();
            }
        }
    }
}