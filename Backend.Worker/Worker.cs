using Azure.Messaging.ServiceBus;
using Backend.Worker.Models;
using System.Text.Json;

namespace Backend.Worker
{
    public class Worker : BackgroundService
    {
        private const string ConnStr = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

        // This List acts as the Consumer's database
        private static List<CourseState> _courseDatabase = new List<CourseState>();

        private readonly ILogger<Worker> _logger;
        private ServiceBusClient _client;
        private ServiceBusProcessor _courseProcessor;
        private ServiceBusProcessor _enrollmentProcessor;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            _client = new ServiceBusClient(ConnStr);

            // Listen to Queue 1 (Admin Events)
            _courseProcessor = _client.CreateProcessor("course-queue", new ServiceBusProcessorOptions());

            // Listen to Queue 2 (Student Events)
            _enrollmentProcessor = _client.CreateProcessor("enrollment-queue", new ServiceBusProcessorOptions());
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // --- HANDLER 1: NEW COURSES ---
            _courseProcessor.ProcessMessageAsync += async args =>
            {
                var json = args.Message.Body.ToString();
                var newCourse = JsonSerializer.Deserialize<CourseState>(json);

                if (!_courseDatabase.Any(c => c.Name == newCourse.Name))
                {
                    _courseDatabase.Add(newCourse);
                    _logger.LogInformation($"[ADMIN EVENT] Course Created: {newCourse.Name} (Cap: {newCourse.Capacity})");
                }
                await args.CompleteMessageAsync(args.Message);
            };
            _courseProcessor.ProcessErrorAsync += ErrorHandler;

            // --- HANDLER 2: ENROLLMENTS ---
            _enrollmentProcessor.ProcessMessageAsync += async args =>
            {
                var json = args.Message.Body.ToString();
                var request = JsonSerializer.Deserialize<EnrollmentRequest>(json);

                _logger.LogInformation($"[STUDENT EVENT] Processing {request.StudentEmail} for {request.CourseName}...");

                var course = _courseDatabase.FirstOrDefault(c => c.Name == request.CourseName);

                if (course == null)
                {
                    _logger.LogError($"FAILED: Course '{request.CourseName}' does not exist yet!");
                }
                else if (course.EnrolledStudents.Count >= course.Capacity)
                {
                    _logger.LogWarning($"FAILED: Course '{request.CourseName}' is FULL.");
                }
                else
                {
                    if (!course.EnrolledStudents.Contains(request.StudentEmail))
                    {
                        course.EnrolledStudents.Add(request.StudentEmail);
                        _logger.LogInformation($"SUCCESS: Enrolled {request.StudentEmail}. Seats left: {course.Capacity - course.EnrolledStudents.Count}");
                    }
                }
                await args.CompleteMessageAsync(args.Message);
            };
            _enrollmentProcessor.ProcessErrorAsync += ErrorHandler;

            // Start both processors
            await _courseProcessor.StartProcessingAsync(stoppingToken);
            await _enrollmentProcessor.StartProcessingAsync(stoppingToken);

            // Keep the worker alive
            await Task.Delay(-1, stoppingToken);
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}