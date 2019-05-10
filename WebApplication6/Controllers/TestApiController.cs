using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication6.Models;

namespace WebApplication6.Controllers
{
    [Route("api/[controller]")]
    //[ApiController]
    public class TestApiController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<TestData>> GetTodoItems()
        {
            return new TestData { Name = "Michael", Department = "IT", Saldo = 20.23 };
        }
    }
}