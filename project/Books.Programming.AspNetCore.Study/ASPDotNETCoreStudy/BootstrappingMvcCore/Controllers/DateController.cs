using System;
using Microsoft.AspNetCore.Mvc;

namespace BootstrappingMvcCore.Controllers
{
    [Route("Day")]
    public class DateController : Controller
    {
        public IActionResult Day(int offset,int id)
        {
            var controller= RouteData.Values["controller"];
            var action = RouteData.Values["action"];


            return Content(DateTime.Now.AddDays(offset).ToShortDateString());
        }


        [Route("{offset}")]
        public ActionResult Details(int offset)
        {
            return Content(DateTime.Now.AddDays(offset).ToShortDateString());
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}