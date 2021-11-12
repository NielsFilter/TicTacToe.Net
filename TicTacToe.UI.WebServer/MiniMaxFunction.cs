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
    public static class MiniMaxFunction
    {
        [FunctionName("MiniMax")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Request received to make a minimax bot move");

            GameState gameState;
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                // #hackAttack. On the web, we have a web human player and a "fake" minimax bot who simply delegates the
                // "thinking" / "processing" to the server. So now we're on the server let's change the players playing to:
                // - a random bot (this can be anything really)
                // - and an "actual" miniMax bot who will calculate the next best move
                requestBody = requestBody.Replace("TicTacToe.UI.Web.Services.WebMiniMaxBot, TicTacToe.UI.Web", "TicTacToe.Bots.MiniMaxBot, TicTacToe.Bots");
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
                
                // Here's another spanner ready to be patched by some hacking... When we deserialize the state, we "recreate"
                // the state object (and it's children). In a "normal" GameState, the PlayersTurn is a reference to either
                // player 1 or 2. But since we've recreated these objects, it's a different object to both.
                // Probably a better way around this is to have an identifier of sorts on each player and overriding equality
                // operator which will then return "true" when we ask "is player1 equal to current playersTurn" (even if they
                // are different reference types.
                // I decided I'm just going to figure out if miniMax was 1 or 2 and set the instance myself...
                
                // Is miniMax is player 1 or 2? We know in this game 2 minimax bots won't compete, so just check the "type"
                var player = gameState.Player1.Type == gameState.PlayersTurn.Type
                    ? gameState.Player1
                    : gameState.Player2;
                
                SetPlayersTurn(gameState, player);
            }
            catch (Exception e)
            {
                log.LogError(e, "Unable to deserialize game state");
                return new BadRequestObjectResult("Game state payload is invalid");
            }

            var move = await gameState.PlayersTurn.MakeMove(gameState);
            log.LogInformation("MiniMax bot move made. Returned move {0}", move);

            return new OkObjectResult(move);
        }

        private static void SetPlayersTurn(GameState gameState, IPlayer player)
        {
            // I didn't want to make the setter publicly available just for our wonderful little hack, so I'd rather
            // keep the objects used for all bots clean and use reflection for our dirty workaround
            var playersTurnProperty = typeof(GameState).GetProperty(nameof(gameState.PlayersTurn));
            if (playersTurnProperty == null)
            {
                throw new InvalidOperationException("That's strange. Where did the property go to?");
            }

            playersTurnProperty.SetValue(gameState, player);
        }
    }
}
