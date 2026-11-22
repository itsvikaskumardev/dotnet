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
                              .WithTags("User JSON & Hash");

            // --- JSON Endpoints ---
            // Save User as JSON (Redis String)
            group.MapPost("/{id}/json", SaveUserJsonAsync)
                 .WithName("SaveUserJson")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Get User JSON
            group.MapGet("/{id}/json", GetUserJsonAsync)
                 .WithName("GetUserJson")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status404NotFound)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Delete User JSON
            group.MapDelete("/{id}/json", DeleteUserJsonAsync)
                 .WithName("DeleteUserJson")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // --- Hash Endpoints ---
            // Save User as Hash (Redis Hash)
            group.MapPost("/{id}/hash", SaveUserHashAsync)
                 .WithName("SaveUserHash")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Get Entire User Hash (HGETALL)
            group.MapGet("/{id}/hash", GetUserHashAsync)
                 .WithName("GetUserHash")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status404NotFound)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Get Specific Field from User Hash (HGET)
            group.MapGet("/{id}/hash/{field}", GetUserHashFieldAsync)
                 .WithName("GetUserHashField")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status404NotFound)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Delete User Hash
            group.MapDelete("/{id}/hash", DeleteUserHashAsync)
                 .WithName("DeleteUserHash")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Delete both JSON and Hash for a user
            group.MapDelete("/{id}", DeleteUserAsync)
                 .WithName("DeleteUserAll")
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
                await db.StringSetAsync(key, jsonString);

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

                // HashGetAll retrieves all field-value pairs in the Redis Hash (HGETALL)
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

                // HashGet retrieves a single field from the Redis Hash (HGET)
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
