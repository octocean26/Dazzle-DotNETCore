using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DI_Sample.Controllers
{
    public class HomeController : Controller
    {
        private IFlagRepository _flagRepository;

        public HomeController(IFlagRepository flagRepository)
        {
            _flagRepository = flagRepository;
        }


       
        public IActionResult Index()
        {
           _flagRepository.GetFlag("nimenhao!");
            return View();
        }
    }
}