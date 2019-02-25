using Microsoft.AspNetCore.Mvc;

namespace BootstrappingMvcCore.Controllers
{
    [Route("goto")]
    public class TourController : Controller
    {
        public IActionResult NewYork() //访问形式：/goto
        {
            string action = RouteData.Values["action"].ToString();
            return Ok(action);
        }

        [Route("nyc")]
        public IActionResult NewYorkCity() //访问形式：/goto/nyc
        {
            string action = RouteData.Values["action"].ToString();
            return Ok(action);
        }

        [Route("/ny")]
        public IActionResult BigApple() //访问形式：/ny
        {
            string action = RouteData.Values["action"].ToString();
            return Ok(action);
        }

        //public IActionResult Index() //访问形式：/goto
        //{
        //    return Ok("Index");
        //}
    }
}