using StackExchange.Redis;

namespace Chai_aur_Redis.OTP_FLow_TTL
{
    public static class OTPFLowEndpoints
    {
        private const int OtpExpirySeconds = 30;

        public static IEndpointRouteBuilder MapOTPFlowEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/otp")
                              .WithTags("OTP Flow");

            // Generate OTP with TTL (30 seconds)
            group.MapPost("/generate", GenerateOTPAsync)
                 .WithName("GenerateOTP")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Verify OTP
            group.MapPost("/verify", VerifyOTPAsync)
                 .WithName("VerifyOTP")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Check OTP remaining TTL
            group.MapGet("/{phoneNumber}/ttl", GetOtpTtlAsync)
                 .WithName("GetOTPTTL")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Query parameter variant: /api/otp/phone/ttl?phoneNumber=...
            group.MapGet("/phone/ttl", (IConnectionMultiplexer redis, string? phoneNumber) => 
                GetOtpTtlAsync(redis, phoneNumber ?? string.Empty))
                 .ExcludeFromDescription();

            // Delete OTP manually
            group.MapDelete("/{phoneNumber}", DeleteOTPAsync)
                 .WithName("DeleteOTP")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            group.MapDelete("", (IConnectionMultiplexer redis, DeleteOtpRequest? request) => 
                DeleteOTPAsync(redis, request?.PhoneNumber ?? string.Empty))
                 .ExcludeFromDescription();

            return routes;
        }

        private static string GetOtpKey(string phoneNumber) => $"otp:{phoneNumber.Trim()}";

        private static async Task<IResult> GenerateOTPAsync(IConnectionMultiplexer redis, OtpPhoneNumberRequest? request)
        {
            if (string.IsNullOrWhiteSpace(request?.PhoneNumber))
            {
                return Results.BadRequest(new
                {
                    success = false,
                    message = "Phone number is required."
                });
            }

            try
            {
                var db = redis.GetDatabase();

                // Generate a random 6-digit OTP (100000 - 999999)
                var otp = Random.Shared.Next(100000, 999999).ToString();
                var key = GetOtpKey(request.PhoneNumber);

                // Set OTP in Redis with 30-second expiry (TTL)
                await db.StringSetAsync(key, otp, TimeSpan.FromSeconds(OtpExpirySeconds));

                return Results.Ok(new
                {
                    success = true,
                    message = "OTP generated successfully.",
                    phoneNumber = request.PhoneNumber,
                    otp, // Returned for easy testing during development
                    expiresInSeconds = OtpExpirySeconds
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

        private static async Task<IResult> VerifyOTPAsync(IConnectionMultiplexer redis, VerifyOtpRequest? request)
        {
            if (string.IsNullOrWhiteSpace(request?.PhoneNumber) || string.IsNullOrWhiteSpace(request?.Otp))
            {
                return Results.BadRequest(new
                {
                    success = false,
                    message = "Both phone number and OTP are required."
                });
            }

            try
            {
                var db = redis.GetDatabase();
                var key = GetOtpKey(request.PhoneNumber);

                var savedOtp = await db.StringGetAsync(key);

                if (savedOtp.IsNullOrEmpty)
                {
                    return Results.BadRequest(new
                    {
                        success = false,
                        message = "OTP expired or not found."
                    });
                }

                if (savedOtp.ToString() != request.Otp.Trim())
                {
                    return Results.BadRequest(new
                    {
                        success = false,
                        message = "Invalid OTP."
                    });
                }

                // Remove OTP after verification so it cannot be reused
                await db.KeyDeleteAsync(key);

                return Results.Ok(new
                {
                    success = true,
                    message = "OTP verified successfully."
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

        private static async Task<IResult> GetOtpTtlAsync(IConnectionMultiplexer redis, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return Results.BadRequest(new
                {
                    success = false,
                    message = "Phone number is required."
                });
            }

            try
            {
                var db = redis.GetDatabase();
                var key = GetOtpKey(phoneNumber);

                var ttl = await db.KeyTimeToLiveAsync(key);

                if (ttl == null)
                {
                    return Results.Ok(new
                    {
                        success = true,
                        exists = false,
                        message = "OTP does not exist or has expired.",
                        ttlSeconds = -1
                    });
                }

                return Results.Ok(new
                {
                    success = true,
                    exists = true,
                    phoneNumber,
                    ttlSeconds = (int)ttl.Value.TotalSeconds
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

        private static async Task<IResult> DeleteOTPAsync(IConnectionMultiplexer redis, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return Results.BadRequest(new
                {
                    success = false,
                    message = "Phone number is required."
                });
            }

            try
            {
                var db = redis.GetDatabase();
                var key = GetOtpKey(phoneNumber);

                var deleted = await db.KeyDeleteAsync(key);

                return Results.Ok(new
                {
                    success = true,
                    message = deleted ? "OTP deleted successfully." : "OTP not found.",
                    deleted
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

    public record OtpPhoneNumberRequest(string PhoneNumber);
    public record VerifyOtpRequest(string PhoneNumber, string Otp);
    public record DeleteOtpRequest(string PhoneNumber);
}
