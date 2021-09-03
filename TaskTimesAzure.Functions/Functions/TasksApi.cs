using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using TaskTimesAzure.Common.Models;
using TaskTimesAzure.Common.Responses;
using TaskTimesAzure.Functions.Entities;

namespace TaskTimesAzure.Functions.Functions
{
    public static class TasksApi
    {
        [FunctionName(nameof(CreatedTasks))]
        public static async Task<IActionResult> CreatedTasks(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tasks")] HttpRequest req,
            [Table("tasks", Connection = "AzureWebJobsStorage")] CloudTable tasksTable,
            ILogger log)
        {
            log.LogInformation("Received a new Employee");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Tasks tasks = JsonConvert.DeserializeObject<Tasks>(requestBody);

            if (string.IsNullOrEmpty(tasks?.IdEmployee.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {

                    Message = "The request must have a employee id."

                });
            }

            TasksEntity tasksEntity = new TasksEntity
            {
                IdEmployee = tasks.IdEmployee,
                DateHour = DateTime.UtcNow,
                Type = tasks.Type,
                Consolidate = false,
                PartitionKey = "TASK",
                RowKey = Guid.NewGuid().ToString(),
                ETag = "*"

            };

            TableOperation addOperation = TableOperation.Insert(tasksEntity);
            await tasksTable.ExecuteAsync(addOperation);

            string message = "new employee store in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {

                IdEmployee = tasks.IdEmployee,
                DateHour = DateTime.UtcNow,
                Message = message,
                Result = tasksEntity


            });


        }

        [FunctionName(nameof(UpdateTasks))]
        public static async Task<IActionResult> UpdateTasks(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "tasks/{Id}")] HttpRequest req,
            [Table("tasks", Connection = "AzureWebJobsStorage")] CloudTable tasksTable,
            string Id,
            ILogger log)
        {
            log.LogInformation($"Update for IdEmployee: {Id}, in tasksTable.");

           
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Tasks tasks = JsonConvert.DeserializeObject<Tasks>(requestBody);

            // Validate Employee Id 
            TableOperation findOperation = TableOperation.Retrieve<TasksEntity>("TASK", Id);
            TableResult findResult = await tasksTable.ExecuteAsync(findOperation);

            if(findResult.Result == null)

            {
                return new BadRequestObjectResult(new Response
                {

                    Message = " id employee not found ."

                });
            }

            //Update Employee id 

            TasksEntity tasksEntity = (TasksEntity)findResult.Result;
            tasksEntity.DateHour = tasks.DateHour;
            tasksEntity.Type = tasks.Type;

            if(!string.IsNullOrEmpty(tasks.IdEmployee.ToString()))
            {
                tasksEntity.Type = tasks.Type;
            }

            TableOperation addOperation = TableOperation.Replace(tasksEntity);
            await tasksTable.ExecuteAsync(addOperation);

            string message = $"Update: {Id} update in taskTable.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {

                IdEmployee = tasks.IdEmployee,
                Message = message,
                Result = tasksEntity


            });




        }

        [FunctionName(nameof(GetAllTasks))]
        public static async Task<IActionResult> GetAllTasks(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tasks")] HttpRequest req,
            [Table("tasks", Connection = "AzureWebJobsStorage")] CloudTable tasksTable,
            ILogger log)
        {
            log.LogInformation("Get all tasksEmployee received.");

            TableQuery<TasksEntity> query = new TableQuery<TasksEntity>();
            TableQuerySegment<TasksEntity> tasks = await tasksTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieved all tasksEmployee.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
              
                Message = message,
                Result = tasks

            });
        }

        [FunctionName(nameof(GetTasksById))]
        public static IActionResult GetTasksById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tasks/{Id}")] HttpRequest req,
            [Table("tasks", "TASK", "{Id}", Connection = "AzureWebJobsStorage")] TasksEntity tasksEntity,
            string id,
            ILogger log)
        {
            log.LogInformation($"Get todo by idEmployee: {id}, received.");

            if (tasksEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    
                    Message = "TasksEmployee not found."
                });
            }

            string message = $"TasksEmployee: {tasksEntity.RowKey}, retrieved.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                Message = message,
                Result = tasksEntity
            });
        }

        [FunctionName(nameof(DeleteTasks))]
        public static async Task<IActionResult> DeleteTasks(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "tasks/{Id}")] HttpRequest req,
            [Table("tasks", "TASK", "{Id}", Connection = "AzureWebJobsStorage")] TasksEntity tasksEntity,
            [Table("tasks", Connection = "AzureWebJobsStorage")] CloudTable tasksTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Delete tasksEmployee: {id}, received.");

            if (tasksEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                 
                    Message = "TasksEmployee not found."
                });
            }

            await tasksTable.ExecuteAsync(TableOperation.Delete(tasksEntity));
            string message = $"TasksEmployee: {tasksEntity.RowKey}, deleted.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {

                Message = message,
                Result = tasksEntity
            });
        }
    }
}
    

    


    



