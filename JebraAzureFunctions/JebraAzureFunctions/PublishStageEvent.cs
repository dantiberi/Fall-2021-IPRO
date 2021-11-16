using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace JebraAzureFunctions.Models
{
    public static class PublishStageEvent
    {
        [FunctionName("PublishStageEvent")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "Event Requests" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBodyAttribute(contentType: "string", bodyType: typeof(PublishStageEventModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            /*
            int stage_id
            int course_id 
            int origin_user_id
            int question_id 
            int inflicted_hp 
            bool was_correct 
            string event_time 
            */

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;
            //ALTER TABLE [dbo].[stage_event_join] NOCHECK CONSTRAINT user_id_fk_on_stage_event_join

            // Add new stage event in separate transaction to prevent deadlock
            string newStageEventCommand = $@"
            INSERT INTO stage_event
            OUTPUT inserted.id
            VALUES ({data?.inflicted_hp}, {data?.was_correct}, '{data?.event_time}');
            ";
            string newStageEventResponse = Tools.ExecuteQueryAsync(newStageEventCommand).GetAwaiter().GetResult();
            //[{"id":2}]
            newStageEventResponse = newStageEventResponse.Substring(1, newStageEventResponse.Length - 2);
            //{"id":2}
            dynamic newStageEventData = JsonConvert.DeserializeObject(newStageEventResponse);
            int newStageEventId = newStageEventData?.id;

            string command = $@"
            INSERT INTO stage_event_join
            VALUES ({data?.stage_id}, {data?.course_id}, {data?.origin_user_id}, {data?.question_id}, {newStageEventId});
            ";
            await Tools.ExecuteNonQueryAsync(command);

            string responseMessage = "Request Sent";

            return new OkObjectResult(responseMessage);
        }
    }
}

