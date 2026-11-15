using System.Security.Claims;
using EntityFrameworkCore.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EntityFrameworkCore.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId =>
            _httpContextAccessor.HttpContext?
                .User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? "system";
    }
}

/*
 
ASP.NET Core mein har HTTP request ka ek HttpContext hota hai.
Ismein request se related information hoti hai:
- Currently logged-in user
- User ke Claims
- Authentication information
- Request headers
- Request details
 */
