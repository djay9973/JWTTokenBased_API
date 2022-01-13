using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JWTTokenBased_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace JWTTokenBased_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        public IConfiguration _configuration;
        public readonly ApplicationDBContext _context;

        public TokenController(IConfiguration configuration, ApplicationDBContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Post(UserInfo userInfo)
        {
if(userInfo != null && userInfo.UserName != null && userInfo.Password != null)
            {
                var user = await GetUser(userInfo.UserName, userInfo.Password);
                if(user != null)
                {
                    var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                        new Claim("Id", user.UserId.ToString()),
                        new Claim("UserName", user.UserName),
                        new Claim("Password", user.Password)
                    };
                    var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));//here key
                    var signIn = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);//it's algorithm
                    var token = new JwtSecurityToken(
                    _configuration["Jwt: Issuer"],
                    _configuration["Jwt: Audience"],
                   claims,
                   expires: DateTime.Now.AddMinutes(20),//it's expiry date
                   signingCredentials:signIn);
                    return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                }
                else
                {
                    return BadRequest("Invalid credentials");
                }
               
            }
else
            {
                return BadRequest();
            }
        }
        [HttpGet]
        public async Task<UserInfo> GetUser(string userName, string pass)
        {
            return await _context.UserInfo.FirstOrDefaultAsync(u=>u.UserName == userName && u.Password == pass);
        }
    }
}
