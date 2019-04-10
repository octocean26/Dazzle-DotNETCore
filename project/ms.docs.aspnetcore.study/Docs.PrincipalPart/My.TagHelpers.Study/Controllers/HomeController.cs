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
        [Route("Tag/{wy?}/{smallz?}")]
        public IActionResult Tag()
        {
            ViewBag.Wy = "wy";
            return View();
        }

        public IActionResult Test()
        {
            return View();
        }
        [Route("/Home/Test2",Name ="Custom")]
        public string Test2(){
            return "Test2";
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(string wy)
        {
            return View();
        }
    }
}