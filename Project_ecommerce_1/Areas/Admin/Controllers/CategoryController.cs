using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project_ecommerce_1.DataAccess.Repository.IRepository;
using Project_ecommerce_1.Model;
using Project_ecommerce_1.Utility;

namespace Project_ecommerce_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize (Roles =SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
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
            var CategoryList = _unitOfWork.Category.GetAll();
            return Json(new {data=CategoryList});
        
        }

        [HttpDelete]
        public IActionResult delete (int id )
        {
            var del = _unitOfWork.Category.Get(id);
            if (del == null)
                return Json(new {success=false,message="Something went wrong while deleting"});
            _unitOfWork.Category.Remove(del);
            _unitOfWork.save();
            return Json(new { success = true, message = "Data deleted successfully" });
        }

        #endregion

        public IActionResult Upsert (int? id)
        {
            Category category1=new Category();
            if (id==null) return View(category1);
            category1 = _unitOfWork.Category.Get(id.GetValueOrDefault());
            if(category1 == null) return NotFound();
            return View(category1);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Category category)
        {
            if (category == null) return NotFound();
            if(!ModelState.IsValid) return View(category);
            if(category.Id==0)
                _unitOfWork.Category.Add(category);
            else
                _unitOfWork.Category.Update(category);
            _unitOfWork.save();
            return RedirectToAction("Index");

        }

    }
}
