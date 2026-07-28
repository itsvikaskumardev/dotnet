using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;
using WebMinimalExample.Data;
using WebMinimalExample.Models;
using WebMinimalExample.Models.DTOs;
using WebMinimalExample.Services;

namespace WebMinimalExample.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var authGroup = app.MapGroup("/api/auth").WithTags("Authentication");

            authGroup.MapPost("/login", Login)
                 .WithName("Login")
                 .Produces<ApiResponse>(StatusCodes.Status200OK)
                 .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                 .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                 .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);

            authGroup.MapPost("/register", Register)
                 .WithName("Register")
                 .Produces<ApiResponse>(StatusCodes.Status201Created)
                 .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                 .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        }

        private static async Task<IResult> Login(
            LoginRequestDTO loginRequestDTO,
            ApplicationDbContext db,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            if (string.IsNullOrWhiteSpace(loginRequestDTO.Email) ||
                string.IsNullOrWhiteSpace(loginRequestDTO.Password))
            {
                return Results.BadRequest(new ApiResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = ["UserName and Password are required"]
                });
            }

            var user = await db.LocalUsers
                .FirstOrDefaultAsync(u => u.Email.ToLower() == loginRequestDTO.Email.ToLower());

            if (user is null || !passwordHasher.VerifyPassword(loginRequestDTO.Password, user.Password))
            {
                return Results.Json(new ApiResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessages = ["Invalid username or password"]
                }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var token = jwtTokenGenerator.GenerateToken(user);
            return Results.Ok(new ApiResponse
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = new LoginResponseDTO
                {
                    Email = user.Email,
                    Token = token,
                    Role = user.Role
                }
            });
        }

        private static async Task<IResult> Register(
            RegisterationRequestDto registerationRequestDto,
            ApplicationDbContext db,
            IPasswordHasher passwordHasher)
        {
            if (string.IsNullOrWhiteSpace(registerationRequestDto.Email) ||
                string.IsNullOrWhiteSpace(registerationRequestDto.Password))
            {
                return Results.BadRequest(new ApiResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = ["Email and Password are required"]
                });
            }

            var existingUser = await db.LocalUsers
                .FirstOrDefaultAsync(u => u.Email.ToLower() == registerationRequestDto.Email.ToLower());

            if (existingUser is not null)
            {
                return Results.BadRequest(new ApiResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = ["User with this email already exists"]
                });
            }

            // Normalize role: only assign Admin if explicitly passed and valid, else default to User
            var assignedRole = string.Equals(
                registerationRequestDto.Role?.Trim(),
                StaticDetails.AdminRole,
                StringComparison.OrdinalIgnoreCase)
                ? StaticDetails.AdminRole
                : StaticDetails.UserRole;

            // Manual mapping: DTO -> Domain Entity
            var newUser = new LocalUser
            {
                Email = registerationRequestDto.Email,
                Password = passwordHasher.HashPassword(registerationRequestDto.Password),
                Name = registerationRequestDto.Name,
                Role = assignedRole
            };

            await db.LocalUsers.AddAsync(newUser);
            await db.SaveChangesAsync();

            return Results.Created($"/api/auth/{newUser.Id}", new ApiResponse
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.Created,
                Result = "A new User Registered Successfully"
            });
        }

    }
}