using IdentityPractice.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityPractice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        private const string _secretKey = "KlSecretKey====00099230203230230 ::>> xsd023++..sd";
        private const string _issuer = "Issuer";
        private const string _audience = "Audience";

        public HomeController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("/register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var user = new IdentityUser()
            {
                UserName = model.Username
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("Register success");
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user is null)
            {
                return Unauthorized("Invalid username");
            }

            // Cookie
            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, false, false
                );

            if (!result.Succeeded)
            {
                return Unauthorized("Invalid username or password");
            }

            var token = this.CreateToken(user);

            var retVal = new
            {
                token
                ,msg = "Login success"
            };

            return Ok(retVal);
        }

        [HttpPost("/logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok("Signout success");
        }

        [Authorize]
        [HttpGet("/getTodos")]
        public IActionResult GetTodos()
        {
            var list = new List<string>()
            {
                "Todo item 1"
                ,"Todo item 2"
            };

            return Ok(list);
        }

        private string CreateToken(IdentityUser user)
        {
            string retVal = string.Empty;

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserName),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
                );

            retVal = new JwtSecurityTokenHandler().WriteToken(token);

            return retVal;
        }
    }
}
