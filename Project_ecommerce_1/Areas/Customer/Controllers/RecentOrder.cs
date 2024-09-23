using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_ecommerce_1.DataAccess.Repository.IRepository;
using Project_ecommerce_1.Model;
using Project_ecommerce_1.Model.ViewModels;
using Project_ecommerce_1.Utility;
using System.Security.Claims;

namespace Project_ecommerce_1.Areas.Customer.Controllers
{

    [Area("Customer")]
    public class RecentOrder : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public RecentOrder(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != null)
            {
                var recentOrders = _unitOfWork.OrderDetail
                    .GetAll(
                        order => order.OrderHeader.ApplicationUserId == userId,
                        query => query.OrderByDescending(o => o.OrderHeaderId),
                        IncludeProperties: "Product,OrderHeader"
                    )
                    .GroupBy(o => o.OrderHeaderId).FirstOrDefault(); 


                return View(recentOrders);
            }

            return View("NoOrder");
        }


        [HttpPost]
        [ActionName("BuyAgain")]
        [ValidateAntiForgeryToken]
        public IActionResult BuyAgain(List<int> selectedItems)
        {
            if (selectedItems != null && selectedItems.Any())
            {
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                foreach (var productId in selectedItems)
                {
                    var productInDb = _unitOfWork.Product.FirstOrDefault(p => p.Id == productId);

                    if (productInDb != null)
                    {
                        var shoppingCartFromDb = _unitOfWork.ShoppingCart.FirstOrDefault(
                            sc => sc.ApplicationUserId == userId && sc.ProductId == productId
                        );

                        if (shoppingCartFromDb == null)
                        {
                            var shoppingCart = new ShoppingCart
                            {
                                ProductId = productId,
                                Count = 1, // You may set the count as needed
                                ApplicationUserId = userId
                            };

                            _unitOfWork.ShoppingCart.Add(shoppingCart);
                        }
                        else
                        {
                            shoppingCartFromDb.Count++; // Increase the count if the item is already in the cart
                        }
                    }
                }
                var ClaimIdentidty = (ClaimsIdentity)User.Identity;
                var Claims = ClaimIdentidty.FindFirst(ClaimTypes.NameIdentifier);
                var Count = _unitOfWork.ShoppingCart.GetAll(SC => SC.ApplicationUserId == Claims.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, Count);

                _unitOfWork.save();

                // Redirect to the cart page
                return RedirectToAction("Summary", "Cart");
            }

            // Redirect to the home page if no items are selected
            return RedirectToAction("Index", "Home");
        }

    }
}

