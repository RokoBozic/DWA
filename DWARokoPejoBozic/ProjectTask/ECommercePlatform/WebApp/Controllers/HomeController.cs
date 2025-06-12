using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    // HomeController: Handles the landing page of the application.
    public class HomeController : Controller
    {
        // Landing: Displays the landing page of the application.
        public IActionResult Landing()
        {
            return View();
        }
    }
}
