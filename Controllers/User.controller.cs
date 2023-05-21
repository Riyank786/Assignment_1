using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;



using Assignment_1.Models;
using Assignment_1.Data;
using Assignment_1.Services;

namespace Assignment_1.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : Controller
    {
        private readonly Assignment_1Context _context;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;
        public UserController(Assignment_1Context context, JwtService jwtService, IConfiguration configuration)
        {
            _context = context;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        [Authorize(Roles = "Admin")]
        public void AddUser(string Username)
        {
            try{
                var user = new User
                {
                    Username = Username,
                    Password = "123456",
                    Role = "User"
                };
                _context.Users.Add(user);
                _context.SaveChanges();
                Console.WriteLine("USER ADDED");
            } catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest();
            }
            var userInDb = _context.Users.FirstOrDefault(u => u.Username == user.Username && u.Password == user.Password);
            if (userInDb == null)
            {
                return NotFound();
            }   
            Console.WriteLine("USER IN DB : " + userInDb.Username + " " + userInDb.Password + " " + userInDb.Role);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userInDb.Username),
                new Claim(ClaimTypes.Role, userInDb.Role),
                new Claim("UserId", userInDb.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var signIn=new SigningCredentials(key,SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["JWT:Issuer"],
                _configuration["JWT:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: signIn);
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            Console.WriteLine("JWT TOKEN : " + jwtToken);
            var response = new
            {
                token = jwtToken,
                user = userInDb
            };
            return Ok(response);
        }
    }
}
