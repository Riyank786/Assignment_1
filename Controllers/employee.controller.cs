using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

using Assignment_1.Data;
using Assignment_1.Models;

namespace Assignment_1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/employee")]
    public class EmployeeController : Controller
    {
        private readonly Assignment_1Context _context;
        private readonly UserController _userController;
        public EmployeeController(Assignment_1Context context, UserController userController)
        {
            _context = context;
            _userController = userController;
        }
        
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("getall")]
        public IActionResult GetAll()
        {
            var employees = _context.Employees.ToList();
            return Ok(employees);
        }

        [HttpGet]
        [Route("getUser")]
        public IActionResult GetUser()
        {
            Console.WriteLine("GET USER");
            // get user id from token which is stored ad userId
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            Console.WriteLine("USER ID : " +userId);
            // get user from db
            var user = _context.Users.FirstOrDefault(u => u.Id == Int32.Parse(userId));
            // return user
            return Ok(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("get/{id}")]
        public IActionResult Get(int id)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.Id == id);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("add")]
        public IActionResult Add([FromBody] Employee employee)
        {
            if (employee == null)
            {
                return BadRequest();
            }
            _context.Employees.Add(employee);
            _context.SaveChanges();
            _userController.AddUser(employee.Name);
            return Ok(employee);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        [Route("update")]
        public IActionResult Update([FromBody] Employee employee)
        {
            if (employee == null)
            {
                return BadRequest();
            }
            var employeeInDb = _context.Employees.FirstOrDefault(e => e.Id == employee.Id);
            if (employeeInDb == null)
            {
                return NotFound();
            }
            employeeInDb.Name = employee.Name;
            employeeInDb.Position = employee.Position;
            _context.SaveChanges();
            return Ok(employeeInDb);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete]
        [Route("delete/{id}")]
        public IActionResult Delete(int id)
        {
            var employeeInDb = _context.Employees.FirstOrDefault(e => e.Id == id);
            if (employeeInDb == null)
            {
                return NotFound();
            }
            _context.Employees.Remove(employeeInDb);
            _context.SaveChanges();
            return Ok(employeeInDb);
        }
    }
}