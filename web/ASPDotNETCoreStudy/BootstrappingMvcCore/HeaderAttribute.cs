using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BootstrappingMvcCore
{
    public class HeaderAttribute:ActionFilterAttribute
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if(!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Value))
            {
                context.HttpContext.Response.Headers.Add(Name, Value);
            }
            return;
        }
    }
}
