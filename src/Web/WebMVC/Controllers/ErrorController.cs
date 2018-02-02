using Microsoft.AspNetCore.Mvc;

namespace HMS.WebMVC.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Error() => View();
    }
}
