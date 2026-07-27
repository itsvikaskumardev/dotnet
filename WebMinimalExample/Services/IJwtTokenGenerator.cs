using WebMinimalExample.Models;

namespace WebMinimalExample.Services
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(LocalUser user);
    }
}
