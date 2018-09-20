using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BootstrappingMvcCore.Controllers
{
    [Route("go/to/[action]")]
    public class VipTourController : Controller
    {

        public IActionResult NewYork() //访问形式：/go/to/newyork
        {
            var action = RouteData.Values["action"].ToString();
            return Ok(action);
        }

        public IActionResult Chicogo() //访问形式：/go/to/chicogo
        {
            var action = RouteData.Values["action"].ToString();
            return Ok(action);

        }

        [Route("{days:int}/days")]
        public IActionResult SanFrancisco(int days) //访问形式：/go/to/sanfrancisco/2/days
        {
            var action = $"In{RouteData.Values["action"].ToString()} for {days} days";
            return Ok(action);
        } 


        public IActionResult Index()
        {
            return View();
        }
    }
}