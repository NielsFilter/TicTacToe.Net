using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TicTacToe.UI.WebServer
{
    public static class QLearningModelFunction
    {
        [FunctionName("QLearningModel")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "qlearn/{level}")] HttpRequest req,
            string level,
            ILogger log,
            ExecutionContext ctx)
        {
            log.LogInformation($"Request to download Q-Learn model {level}");

            var legalLevels = new[]
            {
                "partial",
                "solid",
                "community"
            };
            
            if (string.IsNullOrWhiteSpace(level) || !legalLevels.Contains(level, StringComparer.OrdinalIgnoreCase))
            {
                // sent junk in. You mess with the bull you the horns
                level = "solid";
            }

            var policyData = await LoadPolicyAsync(ctx, level, log);
            if (policyData == null)
            {
                return new InternalServerErrorResult();
            }
            return new OkObjectResult(policyData);
        }

        private static async Task<string> LoadPolicyAsync(ExecutionContext ctx, string fileName, ILogger log)
        { 
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    var filePath = Path.Combine(ctx.FunctionAppDirectory, "QLearnModels", fileName);
                    await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var sr = new StreamReader(fs);
                    return await sr.ReadToEndAsync();
                }
                catch (Exception e)
                {
                    log.LogError(e, "Failed to read q learning model");
                    
                    // the file is opened in fileshare readWrite, so even if someone else is reading / writing to the files we
                    // shouldn't get problems. I'm just over-engineering things here to give it a random back-off and retry.
                    var delay = new Random().Next(0, 3000);
                    await Task.Delay(delay);
                }
            }

            return null;
        }
    }
}
