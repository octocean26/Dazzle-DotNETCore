using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Extensions;

namespace MvcCoreViewSample
{
    public class MultiTenantViewLocationExpander : IViewLocationExpander
    {
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (!context.Values.ContainsKey("tenant") || 
                string.IsNullOrWhiteSpace(context.Values["tenant"]))
                return viewLocations;

            var tenant = context.Values["tenant"];
            var views = viewLocations.Select(f => f.Replace("/Views/", "/Views/" + tenant + "/"))
                .Concat(viewLocations)
                .ToList();

            return views;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            var tenant = context.ActionContext.HttpContext.Request.GetDisplayUrl();
            context.Values["tenant"] = "contoso";  //tenant;
        }

       
    }
}
