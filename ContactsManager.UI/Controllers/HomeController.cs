using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRUDExample.Controllers
{
  public class HomeController : Controller
  {
    [Route("/Error")]
    [AllowAnonymous]
    public IActionResult Error()
    {
      return View(); //Views/Shared/Error
    }
  }
}
