using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project_ecommerce_1.DataAccess.Repository.IRepository;
using Project_ecommerce_1.Model;
using Project_ecommerce_1.Model.ViewModels;
using Project_ecommerce_1.Utility;

namespace Project_ecommerce_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class Product : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public Product(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;

            _webHostEnvironment = webHostEnvironment;

        }
        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            var productlist = _unitOfWork.Product.GetAll(IncludeProperties: "Category,CoverType");
            return Json(new { data = productlist });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var del = _unitOfWork.Product.Get(id);
            if (del == null)

                return Json(new { success = false, message = "Something went wrong while deleting " });
            var WebRootPath = _webHostEnvironment.WebRootPath;
            var ImagePath = Path.Combine(WebRootPath + del.ImageUrl);
            if (System.IO.File.Exists(ImagePath))

            { System.IO.File.Delete(ImagePath); }


            _unitOfWork.Product.Remove(del);
            _unitOfWork.save();
            return Json(new { success = true, message = "Data deleted successfully" });

        }
        #endregion

        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(cl => new SelectListItem()
                {
                    Text = cl.Name,
                    Value = cl.Id.ToString()

                }),
                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(cl => new SelectListItem()
                {
                    Text = cl.Name,
                    Value = cl.Id.ToString()
                }),

                Product = new Model.Product()


            };

            if (id == null) return View(productVM);
            productVM.Product = _unitOfWork.Product.Get(id.GetValueOrDefault());
            return View(productVM);
        }

        

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var webrootpath = _webHostEnvironment.WebRootPath;
                    var files = HttpContext.Request.Form.Files;
                    if (files.Count() > 0)
                    {
                        var filename = Guid.NewGuid().ToString();
                        var extension = Path.GetExtension(files[0].FileName);
                        var upload = Path.Combine(webrootpath, "Images\\Products\\");
                        if (productVM.Product.Id != 0)
                        {
                            var imageexist = _unitOfWork.Product.Get(productVM.Product.Id).ImageUrl;
                            productVM.Product.ImageUrl = imageexist;

                            var path = Path.Combine(webrootpath + productVM.Product.ImageUrl);
                            if (System.IO.File.Exists(path))

                            { System.IO.File.Delete(path); }




                        }

                        using (var filestream = new FileStream(Path.Combine(upload, filename + extension), FileMode.Create))
                        {
                            files[0].CopyTo(filestream);
                        }
                        productVM.Product.ImageUrl = @"\Images\Products\" + filename + extension;
                    }

                    else
                    {
                        var imageexist = _unitOfWork.Product.Get(productVM.Product.Id).ImageUrl;
                        productVM.Product.ImageUrl = imageexist;
                    }

                    if (productVM.Product.Id == 0)
                        _unitOfWork.Product.Add(productVM.Product);
                    else
                        _unitOfWork.Product.Update(productVM.Product);
                    _unitOfWork.save();
                    return RedirectToAction("Index");

                }
                else
                {
                    productVM = new ProductVM()
                    {
                        CategoryList = _unitOfWork.Category.GetAll().Select(cl => new SelectListItem()
                        {
                            Text = cl.Name,
                            Value = cl.Id.ToString()

                        }),
                        CoverTypeList = _unitOfWork.CoverType.GetAll().Select(cl => new SelectListItem()
                        {
                            Text = cl.Name,
                            Value = cl.Id.ToString()
                        }),

                        Product = new Model.Product()


                    };

                    if (productVM.Product.Id != 0)
                    {
                        productVM.Product = _unitOfWork.Product.Get(productVM.Product.Id);
                    }
                    return View(productVM);
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }
    }
}