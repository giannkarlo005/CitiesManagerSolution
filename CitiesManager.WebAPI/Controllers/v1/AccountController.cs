using CitiesManager.Core.DTO;
using CitiesManager.Core.Identity;
using CitiesManager.Core.ServiceContracts;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CitiesManager.WebAPI.Controllers.v1
{
    /// <summary>
    /// This controller is reponsible for User Accounts
    /// </summary>
    [AllowAnonymous] //Bypasses Authorization policy
    [ApiVersion("1.0")]
    public class AccountController : CustomControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IJwtService _jwtService;

        /// <summary>
        /// Initializes the UserManager, SignInManager, RoleManager
        /// </summary>
        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager,
                                 RoleManager<ApplicationRole> roleManager,
                                 IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Registers the User
        /// </summary>
        /// <param name="registerDTO"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<ActionResult<ApplicationUser>> RegisterUser(RegisterDTO registerDTO)
        {
            if(!ModelState.IsValid)
            {
                IEnumerable<string> modelStateErrors =  ModelState.Values.SelectMany(x => x.Errors)
                                                           .Select(y => y.ErrorMessage);

                string modelStateErrorMessage = string.Join(" | ", modelStateErrors);
                return Problem(modelStateErrorMessage);
            }

            ApplicationUser applicationUser = new ApplicationUser()
            {
                UserName = registerDTO.Email,
                PersonName = registerDTO.PersonName,
                Email = registerDTO.Email,
                PhoneNumber = registerDTO.PhoneNumber
            };

            IdentityResult result = await _userManager.CreateAsync(applicationUser, registerDTO.Password);

            if(result.Succeeded)
            {
                //isPersistent determines whether cookies will stay in browser when it is closed
                await _signInManager.SignInAsync(applicationUser, isPersistent: false);

                AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(applicationUser);

                applicationUser.RefreshToken = authenticationResponse.RefreshToken;
                applicationUser.RefreshTokenExpirationDateTime = authenticationResponse.RefreshTokenExpirationDateTime;

                await _userManager.UpdateAsync(applicationUser);

                return Ok(authenticationResponse);
            }

            IEnumerable<string> resultErrors = result.Errors.Select(y => y.Description);
            string resultErrorMessage = string.Join(" | ", resultErrors);

            return Problem(resultErrorMessage);
        }

        /// <summary>
        /// Check if email put by the user already exists in the database
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> IsEmailAlreadyRegistered(string email)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(email);

            if(user == null)
            {
                return Ok(true);
            }
            return Ok(false);
        }

        /// <summary>
        /// Logs in the user
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<ActionResult<ApplicationUser>> LoginUser(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                IEnumerable<string> modelStateErrors = ModelState.Values.SelectMany(x => x.Errors)
                                                           .Select(y => y.ErrorMessage);

                string modelStateErrorMessage = string.Join(" | ", modelStateErrors);
                return Problem(modelStateErrorMessage);
            }

            Microsoft.AspNetCore.Identity.SignInResult? signInResult = await _signInManager.PasswordSignInAsync(loginDTO.Email, loginDTO.Password, isPersistent: false, lockoutOnFailure: false);

            if(!signInResult.Succeeded)
            {
                return Problem("Invalid Email and Password");
            }

            ApplicationUser applicationUser = await _userManager.FindByEmailAsync(loginDTO.Email);

            if(applicationUser == null)
            {
                return NoContent();
            }

            AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(applicationUser);

            applicationUser.RefreshToken = authenticationResponse.RefreshToken;
            applicationUser.RefreshTokenExpirationDateTime = authenticationResponse.RefreshTokenExpirationDateTime;

            await _userManager.UpdateAsync(applicationUser);

            return Ok(authenticationResponse);
        }

        /// <summary>
        /// Logs out the user
        /// </summary>
        /// <returns></returns>
        [HttpGet("logout")]
        public async Task<ActionResult<ApplicationUser>> LogoutUser()
        {
            await _signInManager.SignOutAsync();
            return NoContent();
        }

        [HttpPost("generate-new-jwt-token")]
        public async Task<IActionResult> GenerateNewAccressToken(TokenDTO tokenDTO)
        {
            if(tokenDTO == null)
            {
                return BadRequest("Invalid client request");
            }

            string? jwtToken = tokenDTO.Token;
            string? refreshToken = tokenDTO.RefreshToken;

            ClaimsPrincipal? claimsPrincipal = _jwtService.GetPrincipalFromJwtToken(jwtToken);
            if (claimsPrincipal == null)
            {
                return BadRequest("Invalid jwt access token");
            }

            string? email = claimsPrincipal.FindFirstValue(ClaimTypes.Email);

            ApplicationUser? applicationUser = await _userManager.FindByEmailAsync(email);
            if(applicationUser == null ||
               applicationUser.RefreshToken != refreshToken ||
               applicationUser.RefreshTokenExpirationDateTime <= DateTime.UtcNow)
            {
                return BadRequest("Invalid refresh token");
            }

            AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(applicationUser);
            applicationUser.RefreshToken = authenticationResponse.RefreshToken;
            applicationUser.RefreshTokenExpirationDateTime = authenticationResponse.RefreshTokenExpirationDateTime;

            await _userManager.UpdateAsync(applicationUser);

            return Ok(authenticationResponse);
        }
    }
}
