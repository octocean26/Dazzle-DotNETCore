using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace My.ApplicationParts.Study
{
[GenericControllerNameConvention]
    public class GenericController<T>:Controller
    {
        public IActionResult Index(){
            return Content(typeof(T).Name);
        }
        
    }
}
