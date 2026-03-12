using System;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Game;

namespace TicTacToe.UI.Web.Services
{
    public class WebLlmBot : IPlayer
    {
        private readonly HttpClient _httpClient;
        private readonly Action<bool> _onlineStateChanged;

        public string Model { get; set; } = "gpt-3.5-turbo";

        public WebLlmBot(HttpClient httpClient, Action<bool> onlineStateChanged, string model = "gpt-3.5-turbo")
        {
            _httpClient = httpClient;
            _onlineStateChanged = onlineStateChanged;
            Model = model;
        }

        public PlayerTypes Type => PlayerTypes.LlmBot;

        public async Task<int> MakeMove(GameState state)
        {
            try
            {
                var move = await AskServerLlmAsync(state);
                _onlineStateChanged(true);
                return move;
            }
            catch (Exception)
            {
                _onlineStateChanged(false);
                throw;
            }
        }

        private async Task<int> AskServerLlmAsync(GameState state)
        {
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            var serializedState = JsonConvert.SerializeObject(state, settings);
            using var content = new StringContent(serializedState, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync($"/api/llm/{Model}", content);
            
            var moveResponse = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Server returned {response.StatusCode}: {moveResponse}");
            }

            return int.Parse(moveResponse);
        }
    }
}
