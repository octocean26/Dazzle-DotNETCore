#define smallz

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Docs.Logging.Pages
{
    public class IndexModel : PageModel
    {
        public string Name { get; set; }
        public void OnGet()
        {
#if wy
            this.Name = "wy";
#elif smallz
            this.Name="smallz";
#endif

        }
    }
}