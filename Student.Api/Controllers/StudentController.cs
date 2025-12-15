using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Student.Api.Models;

namespace Student.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class StudentController : ControllerBase
	{
		private const string ConnStr = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";
		private const string QueueName = "enrollment-queue";

		[HttpPost]
		public async Task<IActionResult> Apply(string email, string courseName)
		{
			var request = new EnrollmentRequest { StudentEmail = email, CourseName = courseName };
			var json = JsonSerializer.Serialize(request);

			await using var client = new ServiceBusClient(ConnStr);
			var sender = client.CreateSender(QueueName);
			var message = new ServiceBusMessage(json);

			await sender.SendMessageAsync(message);

			return Accepted($"[Student] Application for '{courseName}' queued.");
		}
	}
}