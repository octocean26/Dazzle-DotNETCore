using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using My.Razor.Study.Data;
using My.Razor.Study.Models;

namespace My.Razor.Study.Pages.Students
{
    public class IndexModel : PageModel
    {
        private readonly My.Razor.Study.Data.MyRazorContext _context;

        public IndexModel(My.Razor.Study.Data.MyRazorContext context)
        {
            _context = context;
        }

        public IList<StudentModel> StudentModel { get;set; }

        public async Task OnGetAsync()
        {
            StudentModel = await _context.Students.ToListAsync();
        }
    }
}
