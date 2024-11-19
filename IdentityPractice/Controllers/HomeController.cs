using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityPractice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        public HomeController()
        {
            
        }

        [HttpPost("/register")]
        public IActionResult Register()
        {
            return Ok();
        }

        [HttpPost("/login")]
        public IActionResult Login()
        {
            return Ok();
        }

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
    }
}
