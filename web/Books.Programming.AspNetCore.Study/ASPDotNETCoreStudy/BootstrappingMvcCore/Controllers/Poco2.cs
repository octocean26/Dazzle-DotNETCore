using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BootstrappingMvcCore.Controllers
{
    [Controller]
    public class Poco2
    {
        public IActionResult Index([FromServices] IModelMetadataProvider provider)
        {
            var viewdata = new ViewDataDictionary<MyViewModel>(provider
                , new ModelStateDictionary());
            viewdata.Model = new MyViewModel() { Title = "Hi!" };
            return new ViewResult()
            {
                ViewData = viewdata,
                ViewName = "index"
            };
        }


        public IActionResult Simple()
        {
            return new ViewResult() { ViewName = "simple" };
        }


        public IActionResult Html()
        {
            return new ContentResult()
            {
                Content = "<h1>Hello</h1>",
                ContentType = "text/html",
                StatusCode = 200
            };
        }
    }
}
