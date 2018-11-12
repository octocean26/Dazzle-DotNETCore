using Docs.DI.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Docs.DI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IMyDependency _myDependency;
        public IndexModel(IMyDependency myDependency)
        {
            _myDependency = myDependency;
        }
        public async Task OnGetAsync()
        {
            await _myDependency.WriteMessage("test");
        }
    }
}