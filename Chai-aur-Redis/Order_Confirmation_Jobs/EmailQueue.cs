using StackExchange.Redis;
using System.Text.Json;

namespace Chai_aur_Redis.Order_Confirmation_Jobs
{
    public class EmailQueue
    {
        private readonly IDatabase _db;
        public const string QueueKey = "queue:worker:emails";

        public EmailQueue(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        // Add Welcome Email Job to Redis Queue
        public async Task<string> AddWelcomeEmailAsync(WelcomeEmailRequest request)
        {
            var job = new EmailJob
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                Name = "send-welcome-email",
                Data = new Dictionary<string, string>
                {
                    ["To"] = request.To,
                    ["Name"] = request.Name ?? "Learner",
                    ["Subject"] = "Welcome to Chai aur Code!"
                },
                Attempts = 3,
                CreatedAt = DateTime.UtcNow
            };

            await _db.ListLeftPushAsync(
                QueueKey,
                JsonSerializer.Serialize(job)
            );

            return job.Id;
        }

        // Add Order Confirmation Job to Redis Queue
        public async Task<string> AddOrderConfirmationAsync(OrderConfirmationRequest request)
        {
            var job = new EmailJob
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                Name = "send-order-confirmation",
                Data = new Dictionary<string, string>
                {
                    ["To"] = request.To,
                    ["OrderId"] = request.OrderId,
                    ["Amount"] = request.Amount.ToString("F2"),
                    ["Subject"] = $"Order Confirmation #{request.OrderId}"
                },
                Attempts = 3,
                CreatedAt = DateTime.UtcNow
            };

            await _db.ListLeftPushAsync(
                QueueKey,
                JsonSerializer.Serialize(job)
            );

            return job.Id;
        }

        // Check pending jobs count in queue
        public async Task<long> GetQueueLengthAsync()
        {
            return await _db.ListLengthAsync(QueueKey);
        }
    }

    public class WelcomeEmailRequest
    {
        public string To { get; set; } = string.Empty;
        public string? Name { get; set; }
    }

    public class OrderConfirmationRequest
    {
        public string To { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class EmailJob
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, string> Data { get; set; } = new();
        public int Attempts { get; set; } = 3;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
