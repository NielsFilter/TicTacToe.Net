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

        public WebLlmBot(HttpClient httpClient, Action<bool> onlineStateChanged)
        {
            _httpClient = httpClient;
            _onlineStateChanged = onlineStateChanged;
        }

        public PlayerTypes Type => PlayerTypes.LlmBot;

        public async Task<int> MakeMove(GameState state)
        {
            var isOnline = true;
            while (true)
            {
                for (var i = 0; i < 3; i++)
                {
                    try
                    {
                        var move = await AskServerLlmAsync(state);
                        if (!isOnline)
                        {
                            _onlineStateChanged(true);   
                        }

                        return move;
                    }
                    catch (JsonSerializationException)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        if (isOnline)
                        {
                            isOnline = false;
                            _onlineStateChanged(isOnline);
                        }

                        // wait a second and retry
                        await Task.Delay(1000);
                    }
                }

                // wait a bit before trying to connect again
                await Task.Delay(5_000);
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
            using var response = await _httpClient.PostAsync("/api/llm", content);
            var moveResponse = await response.Content.ReadAsStringAsync();
            return int.Parse(moveResponse);
        }
    }
}
