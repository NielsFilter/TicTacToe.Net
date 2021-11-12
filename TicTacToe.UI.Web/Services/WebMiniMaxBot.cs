using System;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Bots;
using TicTacToe.Game;

namespace TicTacToe.UI.Web.Services
{
    /// <summary>
    /// So Blazor WebAssembly allows us to run our C# code on the client (No javascript makes me happy)
    /// MiniMax however is proving to be a tricky little bot since it require heavy processing (brute force)
    /// Problem is that Blazor WebAssembly doesn't have proper multi-threading support on the client which
    /// is causing the minimax bot not to be able to figure out what's going on...
    ///
    /// So the work around is to have this "Fake" web minimax bot, that will send up the state to the server
    /// Once on the server, the state is deserialized and the algorithm run there (which obviously supports multi-threading).
    /// The result is then sent back to this bot who makes the move.
    ///
    /// As a fallback, if the server is offline or something goes wrong, let the client work it out (it just means waiting really long)
    /// </summary>
    public class WebMiniMaxBot : MiniMaxBot
    {
        private readonly HttpClient _httpClient;

        public WebMiniMaxBot(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public override async Task<int> MakeMove(GameState state)
        {
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    return await AskServerMiniMaxAsync(state);
                }
                catch (Exception)
                {
                    // oh no, our server bot abandoned us...
                    // Retry, otherwise just process miniMax locally (#sameCodeOnClient #gottaLoveBlazor)
                    await Task.Delay(1000);
                }
            }

            // process minimax algorithm on the client as a fallback
            return await base.MakeMove(state);
        }

        private async Task<int> AskServerMiniMaxAsync(GameState state)
        {
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            var serializedState = JsonConvert.SerializeObject(state, settings);
            using var content = new StringContent(serializedState, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync("/api/minimax", content);
            var moveResponse = await response.Content.ReadAsStringAsync();
            return int.Parse(moveResponse);
        }
    }
}