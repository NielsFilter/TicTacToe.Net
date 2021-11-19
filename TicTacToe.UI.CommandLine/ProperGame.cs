using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicTacToe.Bots;
using TicTacToe.Game;

namespace TicTacToe.UI.CommandLine
{
    public class ProperGame
    {
        private readonly List<string> _gameResults = new();
        public async Task PlayAsync()
        {
            //********* SETUP GAME *********//
            var numberOfGames = 10000;
            var shouldRotate = true;
            var shouldDrawBoard = true;
            var pauseAfterEachGame = true;
            var players = new List<IPlayer>()
            {
                new QLearningBot( @"C:\_temp\qlearning\mypolicy", false, 0, 0, customName: "qlearn"),
                //new MiniMaxBot()
                new RandomMoveBot()
                //new QLearningBot( @"C:\_temp\qlearning\New folder\mypolicy", false, 0, 0, customName: "opponent"),
                //new ConsoleHumanPlayer()
            };

            for (var i = 0; i < numberOfGames; i++)
            {
                var playGame = new ConsolePlayGame(shouldDrawBoard);
                await playGame.StartGameAsync(3, players[0], players[1]);

                if (playGame.Winner == 0)
                {
                    _gameResults.Add("Draw");
                }
                else
                {
                    // record the winning type
                    var winningPlayer = players[playGame.Winner - 1];
                    _gameResults.Add(winningPlayer.GetName());
                }

                if (shouldRotate)
                {
                    // switch order of players around
                    players.Reverse();   
                }

                if (pauseAfterEachGame)
                {
                    Console.Read();
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