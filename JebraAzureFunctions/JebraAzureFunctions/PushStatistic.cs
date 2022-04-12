using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using JebraAzureFunctions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace JebraAzureFunctions
{
    public static class PushStatistic
    {
        [FunctionName("PushStatistic")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "course_id", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The course_id to gather input about a user with.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string course_id = req.Query["course_id"];
            /*
             * Steps:
             * - Get subject_id
             * - Grab all users associated with that course_id
             * - For Each user:
             *  - Use params to get all stage events related to that user in that course.
             *  - Count the number of correct and incorrect questions.
             *  - Sum the inflicted_hp
             *  - Insert into the collected data into the statistic table with the current date.
             */

            //Get subject
            //System.Diagnostics.Debug.WriteLine(Tools.GetSubjectIdFromCourseId(int.Parse(course_id)).GetAwaiter().GetResult());

            //I know its hacky but we are running out of time :(
            string subjectIdS = Tools.GetSubjectIdFromCourseId(int.Parse(course_id)).GetAwaiter().GetResult();
            subjectIdS = subjectIdS.Substring(1, subjectIdS.Length - 2);// Remove [ ]
            int subject_id = JsonConvert.DeserializeObject<JustASubjectId>(subjectIdS).subject_id;

            //Get users
            dynamic allUsers = JsonConvert.DeserializeObject(Tools.GetAllUsersInCourse(int.Parse(course_id)).GetAwaiter().GetResult());

            List<StatisticModel> toPush = new List<StatisticModel>();


            //For each user:
            foreach (var user in allUsers)
            {
                //res = obj.ToString();
                StatisticModel stat = new StatisticModel();

                //Get all stage events for this user in that course:
                string stageEventsCommand = @$"
                    SELECT stage_event.inflicted_hp, stage_event.was_correct FROM stage_event 
                    INNER JOIN stage_event_join ON stage_event.id = stage_event_join.stage_event_id
                    WHERE stage_event_join.course_id = {course_id} and stage_event_join.origin_user_id = {user.user_id};
                ";

                dynamic eventData = JsonConvert.DeserializeObject(Tools.ExecuteQueryAsync(stageEventsCommand).GetAwaiter().GetResult());
                
                int correctAttempt = 0;
                int incorrectAttempt = 0;
                int score = 0;
                foreach(var e in eventData)
                {
                    System.Diagnostics.Debug.WriteLine($"Event: {e}");
                    System.Diagnostics.Debug.WriteLine($"Event was_correct type: {e.was_correct.ToObject<bool>().GetType()}");
                    score += e.inflicted_hp.ToObject<int>();
                    if (e.was_correct.ToObject<bool>())
                    {
                        correctAttempt++;
                    }
                    else
                    {
                        incorrectAttempt++;
                    }                
                }

                System.Diagnostics.Debug.WriteLine($"User: {user}");
                System.Diagnostics.Debug.WriteLine($"Score: {score}");
                System.Diagnostics.Debug.WriteLine($"subject_id: {subject_id}");
                System.Diagnostics.Debug.WriteLine($"user.id: {user.user_id}");
                System.Diagnostics.Debug.WriteLine($"correct_attempt: {correctAttempt}");
                System.Diagnostics.Debug.WriteLine($"incorrect_attempt: {incorrectAttempt}");
                System.Diagnostics.Debug.WriteLine($"date: {DateTime.UtcNow.ToString("MM-dd-yyyy")}");
                System.Diagnostics.Debug.WriteLine("=========================================");

                stat.score = score;
                stat.subject_id = subject_id;
                stat.user_id = user.user_id;
                stat.correct_attempt = correctAttempt;
                stat.incorrect_attempt = incorrectAttempt;
                stat.game_date = DateTime.UtcNow.ToString("MM-dd-yyyy");//May need to change date format to meet Azure's expectations
                toPush.Add(stat);
            }

            string insertCommand = "";
            //Statistic: id score subject_id user_id correct_attempt incorrect_attempt game_date

            //Build insert command
            foreach(StatisticModel stat in toPush)
            {
                insertCommand += $@"
                    INSERT INTO statistic (score, subject_id, user_id, correct_attempt, incorrect_attempt, game_date)
                    VALUES
                    ({stat.score}, {stat.subject_id}, {stat.user_id}, {stat.correct_attempt}, {stat.incorrect_attempt}, '{stat.game_date}');
                ";
                System.Diagnostics.Debug.WriteLine($"Insert: {insertCommand}");
                insertCommand += " ";
            }

            //Send insert command.
            //Tools.ExecuteNonQueryAsync(insertCommand).GetAwaiter().GetResult();

            //return new OkObjectResult(subjectRes);
            return new OkObjectResult(subject_id);
        }
    }
}

public class JustASubjectId
{
    public int subject_id { get; set; }
}
