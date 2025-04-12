using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;


namespace COMP2139_Assignment1.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("/Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var request = HttpContext.Request;
            var user = HttpContext.User.Identity?.Name ?? "Anonymous";

            if (exceptionHandlerPathFeature != null)
            {
                _logger.LogError(
                    "Unhandled exception: Path={Path}, Method={Method}, QueryString={QueryString}, User={User}, Error={Error}",
                    request.Path,
                    request.Method,
                    request.QueryString.Value,
                    user,
                    exceptionHandlerPathFeature.Error
                );
                ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            }

            return View("UnknownError");
        }

        [Route("/Error/{statusCode:int}")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult StatusCodeHandler(int statusCode)
        {
            var request = HttpContext.Request;
            var user = HttpContext.User.Identity?.Name ?? "Anonymous";

            if (statusCode == 404)
            {
                _logger.LogWarning("HTTP {StatusCode} Not Found: Path={Path}, Method={Method}, QueryString={QueryString}, User={User}",
                                    statusCode, request.Path, request.Method, request.QueryString.Value, user);
                return View("Error404");
            }
            else if (statusCode == 500)
            {
                var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
                if (exceptionHandlerPathFeature != null)
                {
                    _logger.LogError(
                        "HTTP {StatusCode} Internal Server Error: Path={Path}, Method={Method}, QueryString={QueryString}, User={User}, Error={Error}",
                        statusCode,
                        request.Path,
                        request.Method,
                        request.QueryString.Value,
                        user,
                        exceptionHandlerPathFeature.Error
                    );
                    ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
                }
                else
                {
                    _logger.LogError("HTTP {StatusCode} Internal Server Error: Path={Path}, Method={Method}, QueryString={QueryString}, User={User}",
                                        statusCode, request.Path, request.Method, request.QueryString.Value, user);
                }
                return View("Error500");
            }
            else
            {
                _logger.LogWarning("HTTP {StatusCode} Error: Path={Path}, Method={Method}, QueryString={QueryString}, User={User}",
                                    statusCode, request.Path, request.Method, request.QueryString.Value, user);
                return View("UnknownError");
            }
        }
    }
}