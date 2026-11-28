using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

namespace Chai_aur_Redis.User_Json_Hash
{
    public static class UserEndpoints
    {
        public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/user")
                              .WithTags("4. User Storage (JSON & Hashes)");

            // --- JSON Endpoints ---
            // Save User as JSON (Redis String)
            group.MapPost("/{id}/json", SaveUserJsonAsync)
                 .WithName("SaveUserJson")
                 .WithSummary("Save user as JSON string")
                 .WithDescription("Serializes and saves a user object into Redis under key 'user:{id}:json'.")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Get User JSON
            group.MapGet("/{id}/json", GetUserJsonAsync)
                 .WithName("GetUserJson")
                 .WithSummary("Get user JSON string")
                 .WithDescription("Retrieves and deserializes user JSON stored in Redis.")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status404NotFound)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Delete User JSON
            group.MapDelete("/{id}/json", DeleteUserJsonAsync)
                 .WithName("DeleteUserJson")
                 .WithSummary("Delete user JSON")
                 .WithDescription("Deletes the 'user:{id}:json' key from Redis.")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // --- Hash Endpoints ---
            // Save User as Hash (Redis Hash)
            group.MapPost("/{id}/hash", SaveUserHashAsync)
                 .WithName("SaveUserHash")
                 .WithSummary("Save user as Redis Hash (HSET)")
                 .WithDescription("Stores key-value dictionary as fields in a Redis Hash under key 'user:{id}:hash'.")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Get Entire User Hash (HGETALL)
            group.MapGet("/{id}/hash", GetUserHashAsync)
                 .WithName("GetUserHash")
                 .WithSummary("Get entire user Hash (HGETALL)")
                 .WithDescription("Fetches all fields and values for the user Hash using HGETALL.")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status404NotFound)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Get Specific Field from User Hash (HGET)
            group.MapGet("/{id}/hash/{field}", GetUserHashFieldAsync)
                 .WithName("GetUserHashField")
                 .WithSummary("Get specific field from user Hash (HGET)")
                 .WithDescription("Fetches a single field from the user Hash using HGET.")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status404NotFound)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Delete User Hash
            group.MapDelete("/{id}/hash", DeleteUserHashAsync)
                 .WithName("DeleteUserHash")
                 .WithSummary("Delete user Hash")
                 .WithDescription("Deletes the 'user:{id}:hash' key from Redis.")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Delete both JSON and Hash for a user
            group.MapDelete("/{id}", DeleteUserAsync)
                 .WithName("DeleteUserAll")
                 .WithSummary("Delete both JSON & Hash for user")
                 .WithDescription("Removes both 'user:{id}:json' and 'user:{id}:hash' keys from Redis.")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            return routes;
        }

        // ==========================================
        // JSON Handlers (Redis Strings)
        // ==========================================

        private static async Task<IResult> SaveUserJsonAsync(
            string id,
            [FromBody] JsonElement request,
            IConnectionMultiplexer redis)
        {
            try
            {
                var db = redis.GetDatabase();
                var key = $"user:{id}:json";

                var jsonString = JsonSerializer.Serialize(request);
                await db.StringSetAsync(key, jsonString);// stroed in string in redis

                return Results.Ok(new
                {
                    success = true,
                    savedAs = "json",
                    key,
                    data = request
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

        private static async Task<IResult> GetUserJsonAsync(
            string id,
            IConnectionMultiplexer redis)
        {
            try
            {
                var db = redis.GetDatabase();
                var key = $"user:{id}:json";

                var raw = await db.StringGetAsync(key);

                if (raw.IsNullOrEmpty)
                {
                    return Results.NotFound(new
                    {
                        success = false,
                        message = $"User JSON key '{key}' was not found in Redis."
                    });
                }

                var user = JsonSerializer.Deserialize<JsonElement>(raw.ToString());

                return Results.Ok(new
                {
                    success = true,
                    key,
                    user
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

        private static async Task<IResult> DeleteUserJsonAsync(
            string id,
            IConnectionMultiplexer redis)
        {
            try
            {
                var db = redis.GetDatabase();
                var key = $"user:{id}:json";

                var deleted = await db.KeyDeleteAsync(key);

                return Results.Ok(new
                {
                    success = true,
                    key,
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

        // ==========================================
        // Hash Handlers (Redis Hashes)
        // ==========================================

        private static async Task<IResult> SaveUserHashAsync(
            string id,
            [FromBody] Dictionary<string, string> request,
            IConnectionMultiplexer redis)
        {
            try
            {
                var db = redis.GetDatabase();
                var key = $"user:{id}:hash";

                var entries = request
                    .Select(x => new HashEntry(x.Key, x.Value))
                    .ToArray();

                await db.HashSetAsync(key, entries);

                return Results.Ok(new
                {
                    success = true,
                    savedAs = "hash",
                    key,
                    fieldsSaved = entries.Length
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

        private static async Task<IResult> GetUserHashAsync(
            string id,
            IConnectionMultiplexer redis)
        {
            try
            {
                var db = redis.GetDatabase();
                var key = $"user:{id}:hash";

                var entries = await db.HashGetAllAsync(key);

                if (entries.Length == 0)
                {
                    return Results.NotFound(new
                    {
                        success = false,
                        message = $"User hash key '{key}' was not found in Redis."
                    });
                }

                var user = entries.ToDictionary(
                    e => e.Name.ToString(),
                    e => e.Value.ToString());

                return Results.Ok(new
                {
                    success = true,
                    key,
                    user
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

        private static async Task<IResult> GetUserHashFieldAsync(
            string id,
            string field,
            IConnectionMultiplexer redis)
        {
            try
            {
                var db = redis.GetDatabase();
                var key = $"user:{id}:hash";

                var value = await db.HashGetAsync(key, field);

                if (value.IsNullOrEmpty)
                {
                    return Results.NotFound(new
                    {
                        success = false,
                        message = $"Field '{field}' was not found in user hash '{key}'."
                    });
                }

                return Results.Ok(new
                {
                    success = true,
                    key,
                    field,
                    value = value.ToString()
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

        private static async Task<IResult> DeleteUserHashAsync(
            string id,
            IConnectionMultiplexer redis)
        {
            try
            {
                var db = redis.GetDatabase();
                var key = $"user:{id}:hash";

                var deleted = await db.KeyDeleteAsync(key);

                return Results.Ok(new
                {
                    success = true,
                    key,
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

        private static async Task<IResult> DeleteUserAsync(
            string id,
            IConnectionMultiplexer redis)
        {
            try
            {
                var db = redis.GetDatabase();
                var jsonKey = $"user:{id}:json";
                var hashKey = $"user:{id}:hash";

                var jsonDeleted = await db.KeyDeleteAsync(jsonKey);
                var hashDeleted = await db.KeyDeleteAsync(hashKey);

                return Results.Ok(new
                {
                    success = true,
                    jsonKeyDeleted = jsonDeleted,
                    hashKeyDeleted = hashDeleted
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
