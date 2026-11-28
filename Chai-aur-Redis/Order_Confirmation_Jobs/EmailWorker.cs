using StackExchange.Redis;
using System.Text.Json;

namespace Chai_aur_Redis.Order_Confirmation_Jobs
{
    public class EmailWorker : BackgroundService
    {
        private readonly IDatabase _db;
        private readonly ILogger<EmailWorker> _logger;
        private const string QueueKey = EmailQueue.QueueKey;

        public EmailWorker(
            IConnectionMultiplexer redis,
            ILogger<EmailWorker> logger)
        {
            _db = redis.GetDatabase();
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(">>> [EmailWorker] Started listening for background email jobs on '{QueueKey}'...", QueueKey);

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // RPOP: Pop the oldest job from the queue (FIFO)
                    var rawJob = await _db.ListRightPopAsync(QueueKey);

                    if (rawJob.IsNullOrEmpty)
                    {
                        // Queue is empty, wait 1 second before next poll to avoid CPU spin
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    var job = JsonSerializer.Deserialize<EmailJob>(rawJob.ToString(), jsonOptions);

                    if (job == null)
                    {
                        continue;
                    }

                    _logger.LogInformation(
                        ">>> [EmailWorker] Processing job {JobId} ({JobName}) for recipient '{Recipient}'...",
                        job.Id,
                        job.Name,
                        job.Data.GetValueOrDefault("To", "Unknown")
                    );

                    bool completed = false;

                    for (int attempt = 1; attempt <= job.Attempts; attempt++)
                    {
                        try
                        {
                            // Simulate sending email (SMTP / SendGrid call)
                            await Task.Delay(1200, stoppingToken);

                            completed = true;

                            _logger.LogInformation(
                                ">>> [EmailWorker] SUCCESS: Job {JobId} ({JobName}) sent successfully on attempt {Attempt}!",
                                job.Id,
                                job.Name,
                                attempt
                            );

                            break;
                        }
                        catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                        {
                            _logger.LogWarning(
                                ex,
                                ">>> [EmailWorker] WARNING: Job {JobId} failed on attempt {Attempt}/{MaxAttempts}.",
                                job.Id,
                                attempt,
                                job.Attempts
                            );

                            if (attempt < job.Attempts)
                            {
                                // Exponential backoff delay (1s, 2s, 4s...)
                                int delay = 1000 * (int)Math.Pow(2, attempt - 1);
                                await Task.Delay(delay, stoppingToken);
                            }
                        }
                    }

                    if (!completed)
                    {
                        _logger.LogError(
                            ">>> [EmailWorker] FAILED: Job {JobId} permanently failed after {Attempts} attempts!",
                            job.Id,
                            job.Attempts
                        );
                    }
                }
                catch (RedisConnectionException ex)
                {
                    _logger.LogWarning(">>> [EmailWorker] Redis connection unavailable. Retrying in 3 seconds... ({Message})", ex.Message);
                    await Task.Delay(3000, stoppingToken);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, ">>> [EmailWorker] Unexpected error in worker loop. Retrying in 2 seconds...");
                    await Task.Delay(2000, stoppingToken);
                }
            }

            _logger.LogInformation(">>> [EmailWorker] Stopping background worker...");
        }
    }
}
