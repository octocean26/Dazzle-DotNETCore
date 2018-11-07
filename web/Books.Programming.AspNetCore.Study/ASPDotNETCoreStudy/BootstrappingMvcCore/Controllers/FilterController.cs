using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BootstrappingMvcCore.Controllers
{
    public class FilterController : Controller
    {
        protected DateTime testtime = DateTime.Now;
        protected DateTime StartTime = DateTime.Now;
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var action = filterContext.ActionDescriptor.RouteValues["action"];
            if (string.Equals(action, "index", StringComparison.CurrentCultureIgnoreCase))
            {
                StartTime = DateTime.Now;
            }
            base.OnActionExecuting(filterContext);
        }


        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var action = filterContext.ActionDescriptor.RouteValues["action"];
            if (string.Equals(action, "index", StringComparison.CurrentCultureIgnoreCase))
            {
                var timeSpan = DateTime.Now - StartTime;
                filterContext.HttpContext.Response.Headers.Add("duration", timeSpan.TotalMilliseconds.ToString());
            }
            base.OnActionExecuted(filterContext);
        }


        public IActionResult Index()
        {

            return Ok(testtime.ToFileTimeUtc()+ " Just processed Filter.Index");
        }

        public IActionResult Test()
        {
            return Ok(testtime.ToFileTimeUtc());
        }

        [Header(Name = "Action", Value = "About")]
        public ActionResult About()
        {
            return Ok("About");
        }
    }
}