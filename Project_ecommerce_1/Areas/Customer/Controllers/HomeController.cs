using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_ecommerce_1.Areas.Admin.Controllers;
using Project_ecommerce_1.DataAccess.Repository.IRepository;
using Project_ecommerce_1.Model;
using Project_ecommerce_1.Model.ViewModels;
using Project_ecommerce_1.Utility;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Security.Claims;
using Product = Project_ecommerce_1.Model.Product;

namespace Project_ecommerce_1.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        public IActionResult MostPurchasedProduct()
        {
            // Get the current user's Id
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                // Handle the case when the user is not authenticated
                return RedirectToAction("Index", "Home"); // Redirect to a suitable page
            }

            // Query to get the most purchased product for the current user
            var mostPurchasedProductId = _unitOfWork.OrderDetail
                .GetAll(od => od.OrderHeader.ApplicationUserId == userId)
                .GroupBy(od => od.ProductId)
                .OrderByDescending(group => group.Count())
                .Select(group => group.Key)
                .FirstOrDefault();

            if (mostPurchasedProductId != 0) // Assuming 0 is not a valid ProductId
            {
                var mostPurchasedProduct = _unitOfWork.Product
                    .FirstOrDefault(p => p.Id == mostPurchasedProductId);

                // Fetch all order details for the current user
                var orderDetails = _unitOfWork.OrderDetail
                    .GetAll(od => od.OrderHeader.ApplicationUserId == userId && od.ProductId == mostPurchasedProductId, IncludeProperties: "Product")
                    .ToList();

                // Calculate the count of purchases for the most purchased product
                var purchaseCount = orderDetails.Count;

                ViewBag.MostPurchasedProduct = mostPurchasedProduct;
                ViewBag.PurchaseCount = purchaseCount;

                return View();
            }
            else
            {
                // Handle the case when the user hasn't purchased any products
                return RedirectToAction("Index", "Home"); // Redirect to a suitable page
            }
        }
        [HttpGet]
        [Route("Home/Search")]
        public IActionResult Search(string searchField, string searchString)
        {
            
            if (string.IsNullOrEmpty(searchString))
            {
                // If search string is empty, show all products
                var allProducts = _unitOfWork.Product.GetAll(IncludeProperties: "Category,CoverType");
                return View("Index", allProducts);
            }

            // Case-insensitive search predicate based on the selected field
            Expression<Func<Project_ecommerce_1.Model.Product, bool>> searchPredicate = null;

            switch (searchField)
            {
                case "Title":
                    searchPredicate = p => p.Title.ToLower().Contains(searchString.ToLower());
                    break;
                case "Author":
                    searchPredicate = p => p.Author.ToLower().Contains(searchString.ToLower());
                    break;
                // Add more cases if needed

                default:
                    // If an invalid field is selected, show all products
                    var allProducts = _unitOfWork.Product.GetAll(IncludeProperties: "Category,CoverType");
                    return View("Index", allProducts);
            }

            // Search results retrieval and sending to the view
            var searchResults = _unitOfWork.Product.GetAll(searchPredicate, IncludeProperties: "Category,CoverType");

            // Get the most frequently bought together product
            if (searchResults.Any())
            {
                var searchedProductId = searchResults.First().Id; // Assuming the first result is used for simplicity
                var frequentlyBoughtTogetherProduct = GetFrequentlyBoughtTogetherProduct(searchedProductId);
                ViewBag.FrequentlyBoughtTogetherProduct = frequentlyBoughtTogetherProduct as Product;
            }

            // Sending search term, selected field, and suggestions to the view
            ViewBag.SearchTerm = searchString;
            ViewBag.SearchField = searchField;

            return View("Index", searchResults);
        }

        private Product GetFrequentlyBoughtTogetherProduct(int searchedProductId)
        {
            // Assuming you have a table named OrderDetail with OrderId and ProductId columns
            var frequentlyBoughtProductId = _unitOfWork.OrderDetail
                .GetAll(od => od.ProductId == searchedProductId)
                .GroupBy(od => od.OrderHeaderId)
                .OrderByDescending(group => group.Count())
                .Select(group => group.Key)
                .LastOrDefault();

            // Retrieve the product details for the most frequently bought together product
            var frequentlyBoughtTogetherProduct = _unitOfWork.OrderDetail
                .GetAll(od => od.OrderHeaderId == frequentlyBoughtProductId && od.ProductId != searchedProductId, IncludeProperties: "Product")
                .FirstOrDefault()?.Product;

            return frequentlyBoughtTogetherProduct;
        }

        public IActionResult Index()
        {
            var ClaimIdentidty = (ClaimsIdentity)User.Identity;
            var Claim = ClaimIdentidty.FindFirst(ClaimTypes.NameIdentifier);
            if (Claim != null)
            {
                var Count = _unitOfWork.ShoppingCart.GetAll(SC => SC.ApplicationUserId == Claim.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, Count);
            }

            var ProductList = _unitOfWork.Product.GetAll(IncludeProperties: "Category,CoverType");
            return View(ProductList);

        }

        public IActionResult Details(int id)
        {
            var ClaimIdentidty = (ClaimsIdentity)User.Identity;
            var Claim = ClaimIdentidty.FindFirst(ClaimTypes.NameIdentifier);
            if (Claim != null)
            {
                var Count = _unitOfWork.ShoppingCart.GetAll(SC => SC.ApplicationUserId == Claim.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, Count);
            }


            var ProductInDb = _unitOfWork.Product.FirstOrDefault(x => x.Id == id, IncludeProperties: "Category,CoverType");
            if (ProductInDb == null) return NotFound();
            var ShoppingCart = new ShoppingCart()
            {
                ProductId = id,
                Product = ProductInDb

            };
            return View(ShoppingCart);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            shoppingCart.Id = 0;
            if (ModelState.IsValid)
            {
                var ClaimsIdentity = (ClaimsIdentity)User.Identity;
                var Claims = ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                shoppingCart.ApplicationUserId = Claims.Value;
                var ShoppingCartFromDb = _unitOfWork.ShoppingCart.FirstOrDefault(SC => SC.ApplicationUserId == Claims.Value && SC.ProductId == shoppingCart.ProductId);
                if (ShoppingCartFromDb == null)
                    _unitOfWork.ShoppingCart.Add(shoppingCart);
                else
                    ShoppingCartFromDb.Count += shoppingCart.Count;
                _unitOfWork.save();
                return RedirectToAction("Index");
            }
            else
            {
                var ProductInDb = _unitOfWork.Product.FirstOrDefault(x => x.Id == shoppingCart.Id, IncludeProperties: "Category,CoverType");
                if (ProductInDb == null) return NotFound();
                var ShoppingCart = new ShoppingCart()
                {
                    ProductId = shoppingCart.Id,
                    Product = ProductInDb

                };
                return View(ShoppingCart);

            }
        }


        





        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

