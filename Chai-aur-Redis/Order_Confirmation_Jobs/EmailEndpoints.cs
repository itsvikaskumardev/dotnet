using Microsoft.AspNetCore.Mvc;

namespace Chai_aur_Redis.Order_Confirmation_Jobs
{
    public static class EmailEndpoints
    {
        public static IEndpointRouteBuilder MapOrderConfirmationEndpoints(
            this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/jobs")
                              .WithTags("6. Background Worker Jobs");

            // Add Welcome Email Job
            group.MapPost("/welcome-email", AddWelcomeEmailJobAsync)
                 .WithName("AddWelcomeEmailJob")
                 .WithSummary("Enqueue a welcome email background job")
                 .WithDescription("Pushes a welcome email task into the Redis worker queue. EmailWorker picks it up automatically in the background.")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Add Order Confirmation Email Job
            group.MapPost("/order-confirmation", AddOrderConfirmationJobAsync)
                 .WithName("AddOrderConfirmationJob")
                 .WithSummary("Enqueue an order confirmation background job")
                 .WithDescription("Pushes an order confirmation email task into the Redis worker queue for background processing.")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Check Worker Queue Length
            group.MapGet("/queue-length", GetQueueLengthAsync)
                 .WithName("GetJobQueueLength")
                 .WithSummary("Check pending background jobs count")
                 .WithDescription("Returns how many jobs are currently queued in Redis waiting for EmailWorker.")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            return routes;
        }

        private static async Task<IResult> AddWelcomeEmailJobAsync(
            [FromBody] WelcomeEmailRequest? request,
            EmailQueue emailQueue)
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
                var jobId = await emailQueue.AddWelcomeEmailAsync(request);

                return Results.Ok(new
                {
                    success = true,
                    message = "Welcome email job added to background queue",
                    jobId,
                    recipient = request.To,
                    workerStatus = "EmailWorker will process this asynchronously"
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

        private static async Task<IResult> AddOrderConfirmationJobAsync(
            [FromBody] OrderConfirmationRequest? request,
            EmailQueue emailQueue)
        {
            if (string.IsNullOrWhiteSpace(request?.To) || string.IsNullOrWhiteSpace(request?.OrderId))
            {
                return Results.BadRequest(new
                {
                    success = false,
                    message = "Both 'to' email and 'orderId' are required."
                });
            }

            try
            {
                var jobId = await emailQueue.AddOrderConfirmationAsync(request);

                return Results.Ok(new
                {
                    success = true,
                    message = "Order confirmation job added to background queue",
                    jobId,
                    orderId = request.OrderId,
                    recipient = request.To,
                    amount = request.Amount,
                    workerStatus = "EmailWorker will process this asynchronously"
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

        private static async Task<IResult> GetQueueLengthAsync(EmailQueue emailQueue)
        {
            try
            {
                var count = await emailQueue.GetQueueLengthAsync();

                return Results.Ok(new
                {
                    success = true,
                    queue = EmailQueue.QueueKey,
                    pendingJobs = count
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
}
