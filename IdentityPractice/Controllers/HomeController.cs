﻿using IdentityPractice.Model;
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
        private readonly IConfiguration _configuration;

        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public HomeController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = config;

            _secretKey = config["Jwt:Key"];
            _issuer = config["Jwt:Issuer"];
            _audience = config["Jwt:Audience"];
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
            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized("Invalid password");
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
                new Claim(ClaimTypes.NameIdentifier, user.Id),
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
