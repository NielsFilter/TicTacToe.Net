using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TicTacToe.Game;

namespace TicTacToe.Bots
{
    public class LlmBot : IPlayer
    {
        private static readonly HttpClient _staticHttpClient = new HttpClient();

        [JsonIgnore]
        public HttpClient? HttpClient { get; set; }
        
        [JsonIgnore]
        public string? EndpointUrl { get; set; }

        [JsonIgnore]
        public string? ApiKey { get; set; }

        public LlmBot()
        {
            EndpointUrl = Environment.GetEnvironmentVariable("LLM_ENDPOINT_URL") ?? "https://api.openai.com/v1/chat/completions";
            ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";
        }

        public PlayerTypes Type => PlayerTypes.LlmBot;

        public async Task<int> MakeMove(GameState state)
        {
            // If only 1 spot left, just return it
            if (state.Board.AvailablePositions.Count == 1)
            {
                return state.Board.AvailablePositions[0];
            }

            var prompt = GeneratePrompt(state);
            
            string systemRole = "You are an expert TicTacToe player. You must output ONLY a single integer corresponding to the index (0-8) of your chosen move. No explanation, no markdown, no other text. ";
            string promptToExplainIndexes = "The board is represented as a 3x3 grid, with the following indexes: Top left is 0, top right is 2, bottom left is 6, bottom right is 8.";
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = systemRole + promptToExplainIndexes },
                    new { role = "user", content = prompt }
                },
                temperature = 0.1
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            
            var client = HttpClient ?? _staticHttpClient;
            var endpoint = EndpointUrl;
            var apiKey = ApiKey;

            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Content = jsonContent;
            
            if (!string.IsNullOrEmpty(apiKey))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            }

            try
            {
                var response = await client.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var jObj = JObject.Parse(responseString);
                    var content = jObj?["choices"]?[0]?["message"]?["content"]?.ToString();
                    
                    if (content != null && int.TryParse(content.Trim(), out int move))
                    {
                        if (state.Board.AvailablePositions.Contains(move))
                        {
                            return move;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Fallback below
            }

            // Fallback move (random)
            var rnd = new Random();
            int index = rnd.Next(state.Board.AvailablePositions.Count);
            return state.Board.AvailablePositions[index];
        }

        private string GeneratePrompt(GameState state)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Current TicTacToe board state:");
            
            int myPlayerNumber = state.GetCurrentPlayersNumber();
            string mySymbol = myPlayerNumber == 1 ? "X" : "O";
            
            sb.AppendLine();
            for (int i = 0; i < 9; i++)
            {
                int val = state.Board.Positions[i];
                string symbol = val == 0 ? i.ToString() : (val == 1 ? "X" : "O");
                sb.Append(symbol);
                if (i % 3 == 2) sb.AppendLine();
                else sb.Append(" | ");
            }
            
            sb.AppendLine();
            sb.AppendLine($"You are playing as: {mySymbol}");
            sb.AppendLine("Available moves (indexes): " + string.Join(", ", state.Board.AvailablePositions));
            sb.AppendLine("Provide the index of the best move:");
            
            return sb.ToString();
        }
    }
}
