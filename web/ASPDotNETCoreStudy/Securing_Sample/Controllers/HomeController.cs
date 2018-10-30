using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Securing_Sample.Controllers
{
    [Authorize(ActiveAuthenticationSchemes="wy")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}