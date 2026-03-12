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

        public string Model { get; set; } = "gpt-3.5-turbo";

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
            string promptToExplainIndexes = "The board is represented as a list of 9 indexes, starting top left, going right and then continuing to the second row and last the third. ";
            string exampleIndex = "For example the board state O000X0000 represents player 'O' having places top left, player 'X' in the middle and the rest of the spaces are open";
            var requestBody = new
            {
                model = Model,
                messages = new[]
                {
                    new { role = "system", content = 
@"You are an expert Tic-Tac-Toe player. Your goal is to win, or force a draw if winning is impossible.

You will be given the current board state represented as a 3x3 grid, where each cell shows its index (0-8) and the current occupant ('X', 'O', or '-' for empty).

Before making a move, you MUST analyze the board in a <reasoning> block. Follow this strategic hierarchy:
1. Win: If you have two in a row, play the third to win.
2. Block: If the opponent has two in a row, play the third to block them.
3. Fork: Create an opportunity where you have two ways to win.
4. Block Fork: Block the opponent from creating a fork.
5. Center: Play the center (index 4) if available.
6. Corner: Play an empty corner.
7. Side: Play an empty side.

After your reasoning, output your final chosen move as a single integer response. 
Example: 4" 
                    },
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
            int myPlayerNumber = state.GetCurrentPlayersNumber();
            string mySymbol = myPlayerNumber == 1 ? "X" : "O";
            

            sb.AppendLine($"You are playing as: {mySymbol}");
            sb.AppendLine($"Current board state:");
            for (int i = 0; i < 9; i++)
            {
                int val = state.Board.Positions[i];
                string symbol = val == 0 ? "0" : (val == 1 ? "X" : "O");
                sb.Append(symbol);
            }
            for (int i = 0; i < 9; i++)
            {
                int val = state.Board.Positions[i];
                string symbol = val == 0 ? i.ToString() : (val == 1 ? "X" : "O");
                sb.Append(i + $": {symbol} ");
                if (i % 3 == 2) sb.AppendLine("------------------");
                else sb.Append("| ");
            }
            
            sb.AppendLine("Available moves (indexes): " + string.Join(", ", state.Board.AvailablePositions));
            sb.AppendLine("Analyze the board and provide your best move.");
            
            return sb.ToString();
        }
    }
}
