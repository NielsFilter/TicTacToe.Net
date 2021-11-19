using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using TicTacToe.Bots;

namespace TicTacToe.UI.Web.Services
{
    public class WebQLearnCommunityBot : QLearningBot
    {
        private readonly HttpClient _httpClient;

        public WebQLearnCommunityBot(
            HttpClient httpClient,
            Dictionary<string, double>? policy) : base(null, true, 10, predefinedPolicy: policy)
        {
            _httpClient = httpClient;
        }

        protected override async Task PolicyUpdatedAsync(CancellationToken cancellationToken)
        {
            await SavePolicyAsync(Policy, cancellationToken);
        }

        private async Task SavePolicyAsync(Dictionary<string, double> policy, CancellationToken cancellationToken)
        {
            try
            {
                await _httpClient.PostAsJsonAsync(@"api\qlearn\community", policy, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}