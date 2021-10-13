using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthenticationPlugin;
using TestDemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TestDemo.Data;

namespace TestDemo.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private IConfiguration _configuration;
        private readonly AuthService _auth;
        private ApplicationDbContext _dbContext;
        public AccountsController(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            _configuration = configuration;
            _auth = new AuthService(_configuration);
            _dbContext = dbContext;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(ApplicationUser user)
        {
            var userWithSameEmail = _dbContext.Users.SingleOrDefault(u => u.Email == user.Email);
            if (userWithSameEmail != null) return BadRequest("User with this email already exists");
            var userObj = new ApplicationUser
            {
                Name = user.Name,
                Email = user.Email,
                Password = SecurePasswordHasherHelper.Hash(user.Password),
                
            };
            _dbContext.Users.Add(userObj);
            await _dbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login(ApplicationUser user)
        {
            var userEmail = _dbContext.Users.FirstOrDefault(u => u.Email == user.Email);
            if (userEmail == null) return StatusCode(StatusCodes.Status404NotFound);
            var hashedPassword = userEmail.Password;
            if (!SecurePasswordHasherHelper.Verify(user.Password, hashedPassword)) return Unauthorized();
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Name, user.Country),
                new Claim(ClaimTypes.Name, user.Bio)
            };

            var token = _auth.GenerateAccessToken(claims);
            return new ObjectResult(new
            {
                access_token = token.AccessToken,
                token_type = token.TokenType,
                user_Id = userEmail.Id,
                user_name = userEmail.Name,
                expires_in = token.ExpiresIn,
                creation_Time = token.ValidFrom,
                expiration_Time = token.ValidTo,
            });
        }
    }
}
