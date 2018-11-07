using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;

namespace BootstrappingMvcCore.Controllers
{
    


    public class PocoController
    {

        [ActionContext]
        public ActionContext Context { get; set; }


        public IActionResult Today()
        {
            var controller = Context.RouteData.Values["controller"];
            return new ContentResult() { Content = DateTime.Now.ToShortDateString() };
        }

        public IActionResult Http([FromQuery] int p1 = 0)
        {
            return new ContentResult() { Content = p1.ToString() };
        }




        private IActionContextAccessor _accessor;

        public PocoController(IActionContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public IActionResult Index()
        {
            object controller = _accessor.ActionContext.RouteData.Values["controller"];
            object action = _accessor.ActionContext.RouteData.Values["action"];
            string text = string.Format("{0}.{1}", controller, action);
            return new ContentResult { Content = text };

        }
    }
}
