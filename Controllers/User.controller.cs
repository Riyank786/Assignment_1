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

namespace Assignment_1.Controllers {
    [ApiController]
    [Route("api/user")]
    public class UserController : Controller {
        private readonly Assignment_1Context _context;
        private readonly JwtService _jwtService;
        private readonly PasswordService _passwordService;
        private readonly IConfiguration _configuration;
        public UserController(Assignment_1Context context, JwtService jwtService, IConfiguration configuration, PasswordService passwordService) {
            _context = context;
            _jwtService = jwtService;
            _configuration = configuration;
            _passwordService = passwordService;
        }

        [Authorize(Roles = "Admin")]
        public void AddUser(string Username) {
            try {
                var user = new User
                {
                    Username = Username,
                    Password = _passwordService.HashPassword("123456"),
                    Role = "User"
                };
                _context.Users.Add(user);
                _context.SaveChanges();
                Console.WriteLine("USER ADDED");
            } catch(Exception e) {
                Console.WriteLine(e);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete]
        public void DeleteUser(string UserName) {
            try {
                var user = _context.Users.FirstOrDefault(u => u.Username == UserName);
                _context.Users.Remove(user);
                _context.SaveChanges();
                Console.WriteLine("USER DELETED");
            } catch(Exception e) {
                Console.WriteLine(e);
            }
        }

        [HttpPatch]
        [Route("changePassword/{password}")]
        public string ChangePassword(string password) {
            try {
                var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                var user = _context.Users.FirstOrDefault(u => u.Id == Int32.Parse(userId));
                if (user == null) {
                    return "User not found";
                }
                user.Password = _passwordService.HashPassword(password);
                _context.SaveChanges();
                return "Password changed";
            } catch(Exception e) {
                Console.WriteLine(e);
                return "Error changing password";
            }
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] User user) {
            if (user == null) {
                return BadRequest();
            }
            // var userInDb = _context.Users.FirstOrDefault(u => u.Username == user.Username && u.Password == user.Password);
            var userInDb = _context.Users.FirstOrDefault(u => u.Username == user.Username);
            if (userInDb == null) {
                return NotFound();
            }
            if (!_passwordService.VerifyPassword(user.Password, userInDb.Password)) {
                return Unauthorized();
            }
            var token = _jwtService.GenerateToken(userInDb.Username, userInDb.Role, userInDb.Id.ToString());
            var response = new {
                token = token,
                user = userInDb
            };
            return Ok(response);
        }
    }
}
