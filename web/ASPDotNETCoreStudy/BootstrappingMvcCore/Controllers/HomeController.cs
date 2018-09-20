using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BootstrappingMvcCore
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var catchall = RouteData.DataTokens["reason"] ?? "";
            if (string.IsNullOrEmpty( catchall.ToString()))
            {
                return View();
            }
            else
            {
                return Content("Error Error");
            }
        }

        [NonAction] //表示控制器方法不是操作方法。
        public ActionResult About()
        {
            return View();
        }
 
        [ActionName("About")]
        public ActionResult LoveGermanShepherds()
        {
            return View();

        }

        [AcceptVerbs("post","get")]
        public IActionResult CallMe()
        {
            return Content("CallMe");
        }


        //[HttpGet]
        //public ActionResult Edit()
        //{
        //    return Content("Edit Get");
        //}

        //[HttpPost]
        //public ActionResult Edit()
        //{
        //    return Content("Edit Post");
        //}

        [HttpGet]
        [ActionName("edit")]
        public ActionResult DisplayEditForm()
        {
            return Content("DisplayEditForm Get");
        }


        [HttpPost]
        [ActionName("edit")]
        public ActionResult SaveEditForm()
        {
            return Content("SaveEditForm Post");
        }


    }
}