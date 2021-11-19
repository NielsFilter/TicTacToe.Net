using System.Collections.Generic;
using System.Threading.Tasks;
using TicTacToe.Bots;
using TicTacToe.Game;

namespace TicTacToe.UI.CommandLine
{
    public class TrainingGame
    {
        public async Task PlayAsync()
        {
            var learningPolicyFilePath = @"C:\_temp\qlearning\New folder\solid_qlearn_30";
            var savePolicyFilePath = @"C:\_temp\qlearning\New folder\solid_qlearn";
            
            var numberOfGamesInCycle = 10;
            var numberOfCycles = 1;
            var shouldRotate = true;

            const string qLearnTrain = "qLearnTrain";
            var qLearningBot = new QLearningBot(learningPolicyFilePath, true, customName: qLearnTrain);
            var qLearningBot2 = new QLearningBot(learningPolicyFilePath, true, 30,  customName: "opponent");
            var randomMoveBot = new RandomMoveBot();
            var miniMaxBot = new MiniMaxBot();
            var humanPlayer = new ConsoleHumanPlayer();
            
            var players = new List<IPlayer>()
            {
                qLearningBot,
                miniMaxBot
                //randomMoveBot
                //qLearningBot2
                //humanPlayer
            };
            
            var resultSet = new QLearningResultSet();
            for (int i = 0; i < numberOfCycles; i++)
            {
                var result = new QLearningResult();
                resultSet.Results[i] = result;

                for (var j = 0; j < numberOfGamesInCycle; j++)
                {
                    var playGame = new ConsolePlayGame(true);
                    await playGame.StartGameAsync(3, players[0], players[1]);

                    if (playGame.Winner == 0)
                    {
                        result.Draw();
                    }
                    else
                    {
                        // record the winning type
                        var winningPlayer = players[playGame.Winner - 1];
                        var qLearningWon = winningPlayer.GetName() == qLearnTrain;

                        if (qLearningWon)
                        {
                            result.Won();
                        }
                        else
                        {
                            result.Lost();
                        }
                    }

                    if (shouldRotate)
                    {
                        // switch order of players around
                        players.Reverse();
                    }
                }
   
                await qLearningBot.SavePolicyAsync(savePolicyFilePath + $"_{((i + 1) * numberOfGamesInCycle)}");
            }
        }
    }
}