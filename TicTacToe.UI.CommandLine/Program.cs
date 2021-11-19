using System;
using System.Threading.Tasks;
using TicTacToe.Bots;

namespace TicTacToe.UI.CommandLine
{
    class Program
    {
        static async Task Main(string[] args)
        {
           // var game = new ProperGame();
            var game = new TrainingGame();
            await game.PlayAsync();
            
            Console.ReadLine();
        }
    }
}