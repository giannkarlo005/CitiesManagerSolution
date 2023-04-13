using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

using CitiesManager.Core.DTO;
using CitiesManager.Core.Identity;
using CitiesManager.Core.ServiceContracts;

namespace CitiesManager.Core.ServiceImplementation
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration) 
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Generates the JWT Token
        /// </summary>
        /// <param name="applicationUser"></param>
        /// <returns></returns>
        public AuthenticationResponse CreateJwtToken(ApplicationUser applicationUser)
        {
            //JWT Token Generation Process
            //data = base64Encode(header) + "." + base64Encode(payload)
            //hashedData = hash(data, secret)
            //signature = base64Encode(hashedData)
            //jwtToken = data + "." + signature

            //Create DateTime object representing the token expiration time by adding the number of minutes specified in the
            //configuration to the current UTC time
            DateTime expirationTime = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:EXPIRATION_MINUTES"]));

            //payload creation
            //Creates an array of Claim objects representing the user's claims, such as their ID, name, email, etc.
            Claim[] claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, applicationUser.Id.ToString()), //Subject = (user ID)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), //JwtID = Unique ID for each token
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()), //IssuedAt (Date and Time token is generated)
                new Claim(ClaimTypes.NameIdentifier, applicationUser.Email), //Optional:Unique name identifier of the user (e.g email)
                new Claim(ClaimTypes.Name, applicationUser.PersonName) //Optional:Name of the user
            };

            //secret key
            //Creates a SymmetricSecurityKey objects using the key specified in the configuration
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            //Hashing Algorithm
            //Creates a SigningCredentials objects with the security key and the HMACSHA256 algorithm
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //jwtToken Creator
            //Creates a JwtSecurityToken object with the given issuer, audience, claims, expiration, and signing credentials
            JwtSecurityToken tokenGenerator = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expirationTime,
                signingCredentials: signingCredentials
            );

            //Create a JwtSecurityTokenHandler object and use it to write the token as a string
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            string token = jwtSecurityTokenHandler.WriteToken(tokenGenerator);

            //Create and return an AuthenticationResponse object containing the token, user email, user name, and token expiration time.
            return new AuthenticationResponse()
            {
                Token = token,
                Email = applicationUser.Email,
                PersonName = applicationUser.PersonName,
                Expiration = expirationTime,
            };
        }
    }
}
