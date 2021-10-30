using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicTacToe.Game;

namespace TicTacToe.UI.CommandLine
{
    public class ProperGame
    {
        private readonly List<string> _gameResults = new();
        public async Task PlayAsync(int numberOfGames, bool shouldRotate, IPlayer player1, IPlayer player2)
        {
            var players = new List<IPlayer>()
            {
                player1,
                player2
            };

            for (var i = 0; i < numberOfGames; i++)
            {
                var playGame = new ConsolePlayGame();
                await playGame.StartGameAsync(4, players[0], players[1]);

                if (playGame.Winner == 0)
                {
                    _gameResults.Add("Draw");
                }

                var winningPlayer = players[playGame.Winner - 1];
                
                // record the winning type
                _gameResults.Add(winningPlayer.GetName());

                if (shouldRotate)
                {
                    // switch order of players around
                    players.Reverse();   
                }
            }
            
            var player1Name= players[0].GetName();
            var player2Name= players[1].GetName();
            
            Console.WriteLine($"Draws:\t{_gameResults.Count(x => x == "Draw")}");
            Console.WriteLine($"{player1Name}:\t{_gameResults.Count(x => x == player1Name)}");
            Console.WriteLine($"{player2Name}:\t{_gameResults.Count(x => x == player2Name)}");
            Console.WriteLine();
        }
    }
}