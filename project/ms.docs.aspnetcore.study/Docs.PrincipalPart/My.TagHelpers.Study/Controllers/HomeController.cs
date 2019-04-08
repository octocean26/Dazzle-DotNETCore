using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace My.TagHelpers.Study.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Tag");
        }

        public IActionResult Tag()
        {
            return View();
        }

        public IActionResult Test()
        {
            return View();
        }
    }
}