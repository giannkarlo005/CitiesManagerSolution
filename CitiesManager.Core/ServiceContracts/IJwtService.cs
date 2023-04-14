using CitiesManager.Core.DTO;
using CitiesManager.Core.Identity;
using System.Security.Claims;

namespace CitiesManager.Core.ServiceContracts
{
    public interface IJwtService
    {
        AuthenticationResponse CreateJwtToken(ApplicationUser applicationUser);
        ClaimsPrincipal? GetPrincipalFromJwtToken(string? token);
    }
}
