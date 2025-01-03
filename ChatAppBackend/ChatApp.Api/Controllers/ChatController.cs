using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
    [HttpGet]
    public IActionResult GetMessages()
    {
        return Ok(new { Message = "This is a protected endpoint" });
    }
}