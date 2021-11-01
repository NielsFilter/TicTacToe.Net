using System;
using System.Threading.Tasks;
using TicTacToe.Bots;

namespace TicTacToe.UI.CommandLine
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var game = new ProperGame();
            await game.PlayAsync(100, true, new RandomMoveBot(500), new ConsoleHumanPlayer());
            
            Console.ReadLine();
        }
    }
}