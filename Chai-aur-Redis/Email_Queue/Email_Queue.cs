using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

namespace Chai_aur_Redis.Email_Queue
{
    public static class EmailQueueEndpoints
    {
        private const string QueueKey = "queue:emails";

        public static IEndpointRouteBuilder MapEmailQueueEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/emails")
                              .WithTags("5. Email Queue (Redis Lists)");

            // Push email to queue (LPUSH)
            group.MapPost("", AddEmailToQueueAsync)
                 .WithName("AddEmailToQueue")
                 .WithSummary("Push email to queue (LPUSH)")
                 .WithDescription("Pushes a new email job to the left side of the Redis list 'queue:emails'.")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Pop and process one email from queue (RPOP)
            group.MapGet("/process-one", ProcessOneEmailAsync)
                 .WithName("ProcessOneEmail")
                 .WithSummary("Pop and process one email (RPOP)")
                 .WithDescription("Pops the oldest email job from the right side of the Redis list (FIFO).")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Check queue length (LLEN)
            group.MapGet("/length", GetQueueLengthAsync)
                 .WithName("GetEmailQueueLength")
                 .WithSummary("Get queue length (LLEN)")
                 .WithDescription("Returns the current number of pending email jobs in the queue.")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Peek at pending emails in queue without removing (LRANGE)
            group.MapGet("/peek", PeekQueueAsync)
                 .WithName("PeekEmailQueue")
                 .WithSummary("Peek pending emails (LRANGE)")
                 .WithDescription("Inspects the pending email jobs in the queue without removing them.")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Clear email queue (DEL)
            group.MapDelete("", ClearQueueAsync)
                 .WithName("ClearEmailQueue")
                 .WithSummary("Clear email queue (DEL)")
                 .WithDescription("Deletes the 'queue:emails' list key from Redis.")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            return routes;
        }

        // Add email to Redis List (LPUSH)
        private static async Task<IResult> AddEmailToQueueAsync(
            [FromBody] EmailRequest? request,
            IConnectionMultiplexer redis)
        {
            if (string.IsNullOrWhiteSpace(request?.To))
            {
                return Results.BadRequest(new
                {
                    success = false,
                    message = "Recipient 'to' email address is required."
                });
            }

            try
            {
                var db = redis.GetDatabase();

                var job = new EmailJob(
                    Id: Guid.NewGuid().ToString("N")[..8],
                    To: request.To.Trim(),
                    Subject: request.Subject?.Trim() ?? "No subject",
                    Body: request.Body?.Trim() ?? "No content",
                    CreatedAt: DateTime.UtcNow
                );

                var serialized = JsonSerializer.Serialize(job);

                // LPUSH: Add job to the front of the list
                var queueLength = await db.ListLeftPushAsync(QueueKey, serialized);

                return Results.Ok(new
                {
                    success = true,
                    queued = true,
                    queueLength,
                    job
                });
            }
            catch (Exception ex)
            {
                return Results.Json(new
                {
                    success = false,
                    error = ex.Message,
                    hint = "Ensure Redis is running (e.g., 'docker compose up -d')"
                }, statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        }

        // Process one email from Redis List (RPOP)
        private static async Task<IResult> ProcessOneEmailAsync(
            IConnectionMultiplexer redis)
        {
            try
            {
                var db = redis.GetDatabase();

                // RPOP: Pop oldest job from the end of the list (FIFO queue)
                var rawJob = await db.ListRightPopAsync(QueueKey);

                if (rawJob.IsNullOrEmpty)
                {
                    return Results.Ok(new
                    {
                        success = true,
                        processed = false,
                        message = "No jobs in the queue."
                    });
                }

                var job = JsonSerializer.Deserialize<EmailJob>(rawJob.ToString());

                return Results.Ok(new
                {
                    success = true,
                    processed = true,
                    message = $"Processed email to '{job?.To}'.",
                    job
                });
            }
            catch (Exception ex)
            {
                return Results.Json(new
                {
                    success = false,
                    error = ex.Message,
                    hint = "Ensure Redis is running (e.g., 'docker compose up -d')"
                }, statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        }

        // Get total count of pending items in queue (LLEN)
        private static async Task<IResult> GetQueueLengthAsync(
            IConnectionMultiplexer redis)
        {
            try
            {
                var db = redis.GetDatabase();
                var length = await db.ListLengthAsync(QueueKey);

                return Results.Ok(new
                {
                    success = true,
                    queue = QueueKey,
                    count = length
                });
            }
            catch (Exception ex)
            {
                return Results.Json(new
                {
                    success = false,
                    error = ex.Message,
                    hint = "Ensure Redis is running (e.g., 'docker compose up -d')"
                }, statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        }

        // Peek pending emails without removing them (LRANGE)
        private static async Task<IResult> PeekQueueAsync(
            IConnectionMultiplexer redis,
            [FromQuery] int count = 5)
        {
            try
            {
                var db = redis.GetDatabase();
                var rawItems = await db.ListRangeAsync(QueueKey, 0, Math.Max(0, count - 1));

                var items = rawItems
                    .Select(r => JsonSerializer.Deserialize<EmailJob>(r.ToString()))
                    .Where(x => x != null)
                    .ToList();

                return Results.Ok(new
                {
                    success = true,
                    queue = QueueKey,
                    totalReturned = items.Count,
                    pendingJobs = items
                });
            }
            catch (Exception ex)
            {
                return Results.Json(new
                {
                    success = false,
                    error = ex.Message,
                    hint = "Ensure Redis is running (e.g., 'docker compose up -d')"
                }, statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        }

        // Clear the entire queue (DEL)
        private static async Task<IResult> ClearQueueAsync(
            IConnectionMultiplexer redis)
        {
            try
            {
                var db = redis.GetDatabase();
                var deleted = await db.KeyDeleteAsync(QueueKey);

                return Results.Ok(new
                {
                    success = true,
                    cleared = deleted,
                    message = deleted ? "Email queue cleared successfully." : "Email queue was already empty."
                });
            }
            catch (Exception ex)
            {
                return Results.Json(new
                {
                    success = false,
                    error = ex.Message,
                    hint = "Ensure Redis is running (e.g., 'docker compose up -d')"
                }, statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        }
    }

    public record EmailRequest(
        string To,
        string? Subject,
        string? Body
    );

    public record EmailJob(
        string Id,
        string To,
        string Subject,
        string Body,
        DateTime CreatedAt
    );
}
