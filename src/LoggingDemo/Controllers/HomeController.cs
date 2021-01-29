using System;
using System.Diagnostics;
using LoggingDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoggingDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult TriggerWarning()
        {
            _logger.LogWarning("{Username} visited privacy page at {Timestamp}", User.Identity?.Name, DateTime.UtcNow);
            return View();
        }

        public IActionResult ComplexLogging()
        {
            //Calls ToString
            _logger.LogInformation("User Information {User}", Request);

            //Attempts a dump
            _logger.LogInformation("Full User Identity Details {@User}", User.Identity);
            return View();
        }

        public IActionResult Privacy()
        {
            //Commented out to avoid performance impacts of executing this on each page request
            //_logger.LogInformation("User Request {@Request}", Request);
            return View();
        }

        public IActionResult EntityFramework()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            _logger.LogError("Bad error happened!");
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}