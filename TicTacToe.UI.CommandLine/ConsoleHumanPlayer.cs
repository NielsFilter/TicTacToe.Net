using System;
using System.Threading.Tasks;
using TicTacToe.Game;

namespace TicTacToe.UI.CommandLine
{
    public class ConsoleHumanPlayer : HumanPlayer
    {
        public override Task<int> MakeMove(GameState state)
        {
            while (true)
            {
                try
                {
                    Console.Write($"Player {state.GetCurrentPlayersNumber()} ({state.GetCurrentPlayersSymbol()}) Make your move 1 - 9: ");
                    var value = Console.ReadLine();
                    if (value != null)
                    {
                        return Task.FromResult(ProcessInput(value, state.Board.Size));
                    }
                    
                    throw new InvalidOperationException($"Must enter a value from 1 - {Math.Pow(state.Board.Size, 2)} ");
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private int ProcessInput(string input, int boardSize)
        {
            var numPositions = (int) Math.Pow(boardSize, 2);
            if (!int.TryParse(input, out var pos) || pos < 1 || pos > numPositions)
            {
                throw new InvalidOperationException($"Expected format is an int from 1 to {numPositions}");
            }

            if (boardSize == 3)
            {
                // easier to use numpad to match blocks:            
                // 7 8 9
                // 4 5 6
                // 1 2 3
                return numPositions - pos - 2;
            }
            
            // Since we're not using board size of 3, the numpad doesn't really work. So let's use 1 - x
            return pos - 1;
        }
    }
}