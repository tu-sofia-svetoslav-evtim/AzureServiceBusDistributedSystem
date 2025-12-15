using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Admin.Api.Models;

namespace Admin.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        // Emulator Connection String
        private const string ConnStr = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";
        private const string QueueName = "course-queue";

        [HttpPost]
        public async Task<IActionResult> CreateCourse(string name, int capacity)
        {
            var course = new Course { Name = name, Capacity = capacity };
            var json = JsonSerializer.Serialize(course);

            await using var client = new ServiceBusClient(ConnStr);
            var sender = client.CreateSender(QueueName);
            var message = new ServiceBusMessage(json);

            await sender.SendMessageAsync(message);

            return Ok($"[Admin] Course '{name}' creation event published.");
        }
    }
}