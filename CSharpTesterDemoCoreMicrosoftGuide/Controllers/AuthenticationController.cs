using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpTesterDemoCoreMicrosoftGuide.Controllers
{
    public class AuthenticationController : Controller
    {
        public IActionResult ExternalLogin()
        {
            return View();
        }
    }
}
