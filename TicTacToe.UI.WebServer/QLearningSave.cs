using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TicTacToe.UI.WebServer
{
    public static class QLearnSaveModel
    {
        [FunctionName("QLearnSaveModel")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "qlearn/community")] HttpRequest req, ILogger log, ExecutionContext ctx)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            
            var policyStr = await new StreamReader(req.Body).ReadToEndAsync();
            
            // just to make sure it's a valid policy
            var _ = JsonConvert.DeserializeObject<Dictionary<string, double>>(policyStr);
            var isSuccess = await SaveCommunityPolicyAsync(ctx, policyStr, log);

            return isSuccess
                ? new AcceptedResult()
                : new InternalServerErrorResult();
        }
        
        private static async Task<bool> SaveCommunityPolicyAsync(ExecutionContext ctx, string policy, ILogger log)
        {
            // yip, I know this is not very resilient, since multiple clients may be trying to train and overwrite the same model.
            // Ideally we should be sending a "delta" of the policy, add it to a queue and have another process pull the deltas and 
            // resolve each delta synchronously. That's not my goal here, so I'll just hard overwrite full policy
            try
            {
                var filePath = Path.Combine(ctx.FunctionAppDirectory, "QLearnModels", "community");
                await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                await using var sw = new StreamWriter(fs);
                await sw.WriteAsync(policy);
                return true;
            }
            catch (Exception e)
            {
                log.LogError(e, "Unable to save community policy");
                return false;
            }
        }
    }
}