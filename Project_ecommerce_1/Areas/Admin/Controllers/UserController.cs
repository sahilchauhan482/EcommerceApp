using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project_ecommerce_1.Data;
using Project_ecommerce_1.DataAccess.Repository.IRepository;
using Project_ecommerce_1.Model;
using Project_ecommerce_1.Utility;

namespace Project_ecommerce_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin+","+SD.Role_Employee)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        public UserController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            var userlist = _context.applicationUsers.ToList();
            var userroles = _context.UserRoles.ToList();
            var roles = _context.Roles.ToList();
            foreach (var user in userlist)
            {
                if (userroles.Any(x=>x.UserId == user.Id))
                {
                    var roleId = userroles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                    user.Role = roles.FirstOrDefault(r => r.Id == roleId).Name;
                }               

                if (user.CompanyId != null)
                {
                    user.Company = new Company()
                    {
                        Name = _unitOfWork.Company.Get(Convert.ToInt32(user.CompanyId)).Name
                    };

                }
                if (user.CompanyId == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };

                }
            }
            var adminuser = userlist.FirstOrDefault(u => u.Role == SD.Role_Admin);
            userlist.Remove(adminuser);

            return Json(new { data = userlist });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            bool isLocked = false;
            var UserInDb=_context.applicationUsers.FirstOrDefault(u => u.Id == id);
            if (UserInDb == null)
            {
                return Json(new { success = false, message = "Something went wrong while lock or unlock user" });

            }
            if(UserInDb !=null && UserInDb.LockoutEnd>DateTime.Now)
            {
                UserInDb.LockoutEnd=DateTime.Now;
                isLocked = false;
            }
            else
            {
                UserInDb.LockoutEnd=DateTime.Now.AddYears(100);
                isLocked = true;
            }
            _context.SaveChanges();
            return Json(new { success = true,message= isLocked==true? "User successfully locked":"User successfully unlocked" });
            }
        #endregion
    }
}
