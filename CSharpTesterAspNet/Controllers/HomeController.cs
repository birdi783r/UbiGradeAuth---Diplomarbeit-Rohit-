using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using Google.Apis.Services;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CSharpTesterAspNet.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        public async Task<ActionResult> About()
        {
            UserCredential credential = GoogleProviderHelper.CreateUserCredential((ClaimsIdentity)User.Identity);

            var service = new ClassroomService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "C# Tester"
            });

            CoursesResource.ListRequest request = service.Courses.List();
            // Restricts returned courses to those in one of the specified states 
            // The default value is ACTIVE, ARCHIVED, PROVISIONED, DECLINED.
            request.CourseStates = CoursesResource.ListRequest.CourseStatesEnum.ACTIVE;

            // Restricts returned courses to those having a teacher with the specified identifier. The identifier can be one of the following:
            // - the numeric identifier for the user
            // - the email address of the user
            // - the string literal "me", indicating the requesting user
            request.TeacherId = "me";

            // List courses.
            List<Course> courses = new List<Course>();
            ListCoursesResponse response = await request.ExecuteAsync();
            courses.AddRange(response.Courses);
            ViewBag.Message = courses.Count;

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}