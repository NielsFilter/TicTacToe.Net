using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Blazored.SessionStorage;
using Newtonsoft.Json;
using TicTacToe.Bots;
using TicTacToe.Game;

namespace TicTacToe.UI.Web.Services
{
    public class QLearningService
    {
        private const int SESSION_STORAGE_EXPIRY_HOURS = 24;
        private readonly HttpClient _httpClient;
        private readonly ISessionStorageService _sessionStorage;
        private string _botName;


        public QLearningService(
            HttpClient httpClient,
            ISessionStorageService sessionStorage)
        {
            _httpClient = httpClient;
            _sessionStorage = sessionStorage;
            _botName = "Q-Learn bot";
        }

        public string GetBotName() => _botName;


        private Dictionary<string, string> qbots = new()
        {
            ["new"] = "Q Noobot",
            ["average"] = "Q Average",
            ["community"] = "Q Train me",
            ["solid"] = "Q Machine"
        };
        
        public async Task<IPlayer> LoadQLearnBot(string botName)
        {
            var level = botName.Split('-').Last().ToLower();
            if (!qbots.Keys.Contains(level, StringComparer.OrdinalIgnoreCase))
            {
                level = "solid";
            }
            
            _botName = qbots[level];
            
            var policy = await LoadQLearnPolicy(level);
            
            var bot = level switch
            {
                "new" => new QLearningBot(null, true, 10, 5, 1, turnDelay: 300, predefinedPolicy: policy),
                "partial" => new QLearningBot(null, true, 5, turnDelay: 300, predefinedPolicy: policy),
                "community" => new WebQLearnCommunityBot(_httpClient, policy),
                _ => new QLearningBot(null, true, 0, 0, 0, turnDelay: 300, predefinedPolicy: policy)
            };

            return bot;
        }

        private async Task<Dictionary<string, double>?> LoadQLearnPolicy(string level)
        {
            if (level == "new")
            {
                return null;
            }

            try
            {
                var policy = await TryLoadFromLocalStorage(level);
                if (policy != null)
                {
                    return policy;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            // Unable to load from local store. Go get policy content from the server
            var policyContent = level switch
            {
                "partial" => await _httpClient.GetStringAsync(@"api\qlearn\partial"),
                "community" => await _httpClient.GetStringAsync(@"api\qlearn\community"),
                _ => await _httpClient.GetStringAsync(@"api\qlearn\solid")
            };

            var policyFromServer = JsonConvert.DeserializeObject<Dictionary<string, double>>(policyContent);
            if (policyFromServer == null)
            {
                throw new InvalidOperationException("Expected a policy");
            }

            var sessionStorageItem = new SessionStorageItem<Dictionary<string, double>>()
            {
                TimeStamp = DateTime.UtcNow,
                Item = policyFromServer
            };
            // store policy to local storage 
            await _sessionStorage.SetItemAsync($"qlearn-{level}", sessionStorageItem);

            return policyFromServer;
        }

        private async Task<Dictionary<string, double>?> TryLoadFromLocalStorage(string level)
        {
            if (level == "community")
            {
                // let's not cache the community policy. If others are also training the bot. Best to always load it from the server
                return null;
            }
            
            try
            {
                var policyContentStorage = await _sessionStorage.GetItemAsync<SessionStorageItem<Dictionary<string, double>>>($"qlearn-{level}");
                if (policyContentStorage != null)
                {
                    if (policyContentStorage.TimeStamp.AddHours(SESSION_STORAGE_EXPIRY_HOURS) < DateTime.UtcNow)
                    {
                        // storage has expired
                        await _sessionStorage.RemoveItemAsync($"qlearn-{level}");
                        return null;
                    }
                    
                    return policyContentStorage.Item;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }
        
        public static string FirstCharToUpper(string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };
    }
}