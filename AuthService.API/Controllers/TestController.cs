using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class TestController : ControllerBase
{
    public TestController()
    {
        Console.WriteLine("🎯 TestController constructor called");
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        Console.WriteLine("🎯 Ping method called");
        return Ok(new { message = "pong", timestamp = DateTime.UtcNow });
    }
}