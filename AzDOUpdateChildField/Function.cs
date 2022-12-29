using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Azure.Security.KeyVault.Secrets;
using AzDOUpdateChildField.Classes;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.VisualStudio.Services.WebApi.Patch;

namespace AzDOUpdateChildField
{
    public class Function
    {

        private readonly SecretClient _secretClient;

        public Function(SecretClient secretClient)
        {
            _secretClient = secretClient;
        }

        [FunctionName(nameof(UpdateChildFieldsWhenParentFieldChanges))]
        public async Task<IActionResult> UpdateChildFieldsWhenParentFieldChanges(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            var body = await GetBody<WebhookBody>(req);

            using (var workItemConnection = await Connect<WorkItemTrackingHttpClient>())
            {
                var wi = await workItemConnection.GetWorkItemAsync(body.resource.workItemId, new List<string>() { "Custom.Sciforma", "Custom.StudyName" }, expand: Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItemExpand.Relations);

                foreach (var c in wi.Relations.Where(a => a.Rel == "System.LinkTypes.Hierarchy-Forward"))
                {
                    var id = int.Parse(c.Url.Split("/").Last());
                    var child = await workItemConnection.GetWorkItemAsync(id, new List<string>() { "Custom.Sciforma", "Custom.StudyName" });
                   
                    //base work done, need logic to detect if child wi type has custom fields as available options

                    JsonPatchDocument patch = new JsonPatchDocument();

                    if (wi.Fields["Custom.Sciforma"].ToString() != child.Fields["Custom.Sciforma"].ToString())
                    {
                        patch.Add(new JsonPatchOperation()
                        {
                            Operation = Operation.Add,
                            Path = "/fields/Custom.Sciforma",
                            Value = wi.Fields["Custom.Sciforma"].ToString()
                        });
                    }

                    if (wi.Fields["Custom.StudyName"].ToString() != child.Fields["Custom.StudyName"].ToString())
                    {
                        patch.Add(new JsonPatchOperation()
                        {
                            Operation = Operation.Add,
                            Path = "/fields/Custom.StudyName",
                            Value = wi.Fields["Custom.StudyName"].ToString()
                        });
                    }

                    if (patch.Any())
                        await workItemConnection.UpdateWorkItemAsync(patch, id);

                }
            }
            return new OkResult();
        }

        public async Task<T> Connect<T>() where T : VssHttpClientBase
        {
            VssBasicCredential cred = new VssBasicCredential(new NetworkCredential("", (await _secretClient.GetSecretAsync("azure-devops-pat")).Value.Value));

            VssConnection connection = new VssConnection(new Uri(Environment.GetEnvironmentVariable("AzDOBaseUrl")), new VssCredentials(cred));

            return connection.GetClient<T>();
        }

        public async Task<T> GetBody<T>(HttpRequest req)
        {
            return JsonConvert.DeserializeObject<T>(await new StreamReader(req.Body).ReadToEndAsync());
        }
    }
}
