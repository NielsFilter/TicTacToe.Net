using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TicTacToe.Game;

namespace TicTacToe.UI.WebServer
{
    public static class LlmFunction
    {
        [FunctionName("LlmHost")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "llm/{model}")] HttpRequest req, string model,
            ILogger log)
        {
            log.LogInformation("Request received to make a Llm bot move");

            GameState gameState;
            string requestBody = "";
            try
            {
                requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                
                requestBody = requestBody.Replace("TicTacToe.UI.Web.Services.WebLlmBot, TicTacToe.UI.Web", "TicTacToe.Bots.LlmBot, TicTacToe.Bots");
                requestBody = requestBody.Replace("TicTacToe.UI.Web.Services.WebHumanPlayer, TicTacToe.UI.Web", "TicTacToe.Bots.RandomMoveBot, TicTacToe.Bots");
                
                gameState = JsonConvert.DeserializeObject<GameState>(requestBody, new JsonSerializerSettings()
                {
                    ContractResolver = new PrivateResolver(),
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                    TypeNameHandling = TypeNameHandling.All
                });

                if (gameState == null)
                {
                    return new BadRequestObjectResult("Unable to deserialize the Game state payload");
                }
                
                log.LogInformation("Llm bot model is {0}", model);

                if (gameState.Player1 is TicTacToe.Bots.LlmBot bot1){
                    log.LogInformation("Player1 is LlmBot");
                    var requestStr = bot1.GenerateRequestString(gameState);
                    log.LogInformation(requestStr);
                    bot1.Model = model;
                } 
                if (gameState.Player2 is TicTacToe.Bots.LlmBot bot2){
                    log.LogInformation("Player2 is LlmBot");
                    var requestStr = bot2.GenerateRequestString(gameState);
                    log.LogInformation(requestStr);
                     bot2.Model = model;
                }

                var player = gameState.Player1.Type == gameState.PlayersTurn.Type
                    ? gameState.Player1
                    : gameState.Player2;
                
                SetPlayersTurn(gameState, player);
            }
            catch (Exception e)
            {
                log.LogError(e, "Unable to deserialize game state: ", requestBody);
                return new BadRequestObjectResult("Game state payload is invalid. " + e.Message);
            }

            try{
                        var move = await gameState.PlayersTurn.MakeMove(gameState);
                        log.LogInformation("Llm bot move made. Returned move {0}", move);
                        return new OkObjectResult(move);
            } catch(Exception e)
            {
                log.LogError(e, "Unable to make move. Msg : {0}", e.Message);
                return new BadRequestObjectResult(e.Message);
            }
            

        }

        private static void SetPlayersTurn(GameState gameState, IPlayer player)
        {
            var playersTurnProperty = typeof(GameState).GetProperty(nameof(gameState.PlayersTurn));
            if (playersTurnProperty == null)
            {
                throw new InvalidOperationException("That's strange. Where did the property go to?");
            }

            playersTurnProperty.SetValue(gameState, player);
        }
    }
}
