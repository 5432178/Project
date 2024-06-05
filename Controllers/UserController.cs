using Microsoft.AspNetCore.Mvc;
using Project.Data;
using Project.Models;

namespace Project.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(UserLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.SingleOrDefault(u => u.Login == model.Login && u.Password == model.Password);
                if (user != null)
                {
                    HttpContext.Session.SetString("IsLoggedIn", "true");
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("UserName", user.Name);
                    return RedirectToAction("Index", "Home");
                }
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
             }
                    return  RedirectToAction("Login", "User"); 
        }
            public IActionResult Logout()
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "User");
            }
        }
    }

