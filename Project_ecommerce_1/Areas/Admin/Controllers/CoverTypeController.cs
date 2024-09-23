using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project_ecommerce_1.DataAccess.Repository.IRepository;
using Project_ecommerce_1.Model;
using Project_ecommerce_1.Utility;

namespace Project_ecommerce_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CoverTypeController(IUnitOfWork unitOfWork)
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
            return Json(new {data=_unitOfWork.CoverType.GetAll()});
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var del=_unitOfWork.CoverType.Get(id);
            if(del == null)
                return Json(new { success = false, message = "Something went wrong while deleting" });
            else
                _unitOfWork.CoverType.Remove(del);
            _unitOfWork.save();
            return Json(new { success = true, message = "Data deleted successfully" });
        }
        #endregion
        public IActionResult Upsert( int? id)
        {
            CoverType coverType=new CoverType();
            if(coverType.Id == 0) return View(coverType);
            else
                return View(coverType);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(CoverType coverType)
        {
            if (coverType == null) return BadRequest();
            if(!ModelState.IsValid) return View(coverType);
            if (coverType.Id == 0)
                _unitOfWork.CoverType.Add(coverType);
            else
                _unitOfWork.CoverType.Update(coverType);
            _unitOfWork.save();
            return RedirectToAction("Index");
        }
    }
}
