using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Project_ecommerce_1.DataAccess.Repository;
using Project_ecommerce_1.DataAccess.Repository.IRepository;
using Project_ecommerce_1.Model;
using Project_ecommerce_1.Model.ViewModels;
using Project_ecommerce_1.Utility;
using Stripe;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using Address = Project_ecommerce_1.Model.Address;

namespace Project_ecommerce_1.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private static bool isemailConfirm=false;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly TwilioService _twilioService;
        public CartController(IUnitOfWork unitOfWork,IEmailSender emailSender,UserManager<IdentityUser> userManager,TwilioService twilioService)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _userManager = userManager;
            _twilioService = twilioService;
        }
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public IActionResult Index()
        {
            var ClaimIdentity = (ClaimsIdentity)User.Identity;
            var Claim = ClaimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM = new ShoppingCartVM
            {
                Listcart = _unitOfWork.ShoppingCart.GetAll(SC => SC.ApplicationUserId == Claim.Value, IncludeProperties: "Product"),
                orderHeader = new OrderHeader()
            };
            ShoppingCartVM.orderHeader.OrderTotal = 0;
            ShoppingCartVM.orderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(AU => AU.Id == Claim.Value);
            foreach (var List in ShoppingCartVM.Listcart)
            {
                List.Price = SD.GetPriceBasedOnQuantity(List.Count, List.Product.Price, List.Product.Price50, List.Product.Price100);
                ShoppingCartVM.orderHeader.OrderTotal += (List.Price * List.Count);
                if (List.Product.Description.Length > 100)
                {
                    List.Product.Description = List.Product.Description.Substring(0, 100) + "....";
                }
            }
            return View(ShoppingCartVM);
        }

        public IActionResult Plus(int id)
        {
            var cart = _unitOfWork.ShoppingCart.FirstOrDefault(sc => sc.Id == id);
            cart.Count++;
            _unitOfWork.save();
            return RedirectToAction("Index");
        }

        public IActionResult Minus(int id)
        {
            var cart = _unitOfWork.ShoppingCart.FirstOrDefault(sc => sc.Id == id);
            if (cart.Count == 1)
                cart.Count = 1;
            else
                cart.Count--;
            _unitOfWork.save();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var cart = _unitOfWork.ShoppingCart.FirstOrDefault(c => c.Id == id);
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.save();
            var ClaimIdentidty = (ClaimsIdentity)User.Identity;
            var Claim = ClaimIdentidty.FindFirst(ClaimTypes.NameIdentifier);
            var Count = _unitOfWork.ShoppingCart.GetAll(SC => SC.ApplicationUserId == Claim.Value).ToList().Count;
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, Count);



            return RedirectToAction("Index");
        }

        public IActionResult Summary(List<int> SelectedItems)
        {
            var ClaimIdentity = (ClaimsIdentity)User.Identity;
            var Claim = ClaimIdentity.FindFirst(ClaimTypes.NameIdentifier);


            ShoppingCartVM = new ShoppingCartVM
            {
                Listcart = _unitOfWork.ShoppingCart.GetAll(SC => SC.ApplicationUserId == Claim.Value &&
                (SelectedItems == null || !SelectedItems.Any() || SelectedItems.Contains(SC.Id)), IncludeProperties: "Product"),
                orderHeader = new OrderHeader(),
                UserAddresses = new List<Address>()
            };

            var orderHeaders = _unitOfWork.OrderHeader.GetAll(SC => SC.ApplicationUserId == Claim.Value).ToList();

            ShoppingCartVM.UserAddresses = orderHeaders.Select(x => new Address
            {
                Name = x.Name,
                City = x.City,
                StreetAddress = x.StreetAddress,
                PhoneNumber = x.PhoneNumber,
                PostalCode = x.PostalCode,
                State = x.State,
                Id = x.Id,
            }).DistinctBy(x => x.FullAddress).ToList();
            

            ShoppingCartVM.orderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(AU => AU.Id == Claim.Value);
            foreach (var List in ShoppingCartVM.Listcart)
            {
                List.Price = SD.GetPriceBasedOnQuantity(List.Count, List.Product.Price, List.Product.Price50, List.Product.Price100);
                ShoppingCartVM.orderHeader.OrderTotal += (List.Price * List.Count);
                if (List.Product.Description.Length > 100)
                {
                    List.Product.Description = List.Product.Description.Substring(0, 100) + "....";
                }


            }

            ShoppingCartVM.orderHeader.Name = ShoppingCartVM.orderHeader.ApplicationUser.Name;
            ShoppingCartVM.orderHeader.PhoneNumber = ShoppingCartVM.orderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.orderHeader.StreetAddress = ShoppingCartVM.orderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.orderHeader.City = ShoppingCartVM.orderHeader.ApplicationUser.City;
            ShoppingCartVM.orderHeader.State = ShoppingCartVM.orderHeader.ApplicationUser.State;
            ShoppingCartVM.orderHeader.PostalCode = ShoppingCartVM.orderHeader.ApplicationUser.PostalCode;

            if (!isemailConfirm)
            {
                ViewBag.EmailMessage="Need to verify the email before placing the order";
                ViewBag.EmailCSS = "text-success";
                isemailConfirm = false;

            }
            else
            {
                ViewBag.EmailMessage = "Email must be confirm for authorize customer";
                ViewBag.EmailCSS = "text-danger";
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("VerifyEmailAjax")]
        public async Task<IActionResult> VerifyEmailAjax()
        {
            var ClaimIdentity = (ClaimsIdentity)User.Identity;
            var Claim = ClaimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == Claim.Value);
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);

            try
            {
                await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                isemailConfirm = true;

                return Json(new { success = true, message = "Email verification initiated successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as per your application's requirements
                return Json(new { success = false, message = "Error in email verification: " + ex.Message });
            }
        }
        [HttpPost]
        [ActionName("Summary")]
        public async Task<IActionResult> PlaceOrder(List<int> SelectedItems, string stripeToken)
        {
            var ClaimIdentity = (ClaimsIdentity)User.Identity;
            var Claim = ClaimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            
            ShoppingCartVM.orderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == Claim.Value);
            ShoppingCartVM.Listcart = _unitOfWork.ShoppingCart.GetAll(SC => SC.ApplicationUserId == Claim.Value && 
            (SelectedItems == null || !SelectedItems.Any() || SelectedItems.Contains(SC.Id)), IncludeProperties: "Product");
            //if (ShoppingCartVM.Listcart.Count() == 0) return NotFound();
            ShoppingCartVM.orderHeader.OrderStatus = SD.OrderStatusPending;
            ShoppingCartVM.orderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.orderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.orderHeader.ApplicationUserId = Claim.Value;
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.orderHeader);
            _unitOfWork.save();

            foreach (var list in ShoppingCartVM.Listcart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                OrderDetail orderDetail = new OrderDetail()
                {
                    OrderHeaderId = ShoppingCartVM.orderHeader.Id,
                    ProductId = list.Product.Id,
                    Price = list.Price,
                    Count = list.Count,
                };
                ShoppingCartVM.orderHeader.OrderTotal += (list.Price * list.Count);
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.save();
            }

            #region Stripe payment code
            if (stripeToken == null)
            {
                ShoppingCartVM.orderHeader.PaymentDueDate = DateTime.Today.AddDays(30);
                ShoppingCartVM.orderHeader.PaymentStatus = SD.PaymentStatusDelayPayment;
                ShoppingCartVM.orderHeader.OrderStatus = SD.OrderStatusApproved;
            }
            else
            {
                var options = new ChargeCreateOptions()
                {
                    Amount = Convert.ToInt32(ShoppingCartVM.orderHeader.OrderTotal),
                    Currency = "usd",
                    Description = "Order Id:" + ShoppingCartVM.orderHeader.Id,
                    Source = stripeToken
                };
                #region Payment
                var service = new ChargeService();
                Charge charge = service.Create(options);
                if (charge.BalanceTransactionId == null)
                {
                    ShoppingCartVM.orderHeader.PaymentStatus = SD.PaymentStatusRejected;
                    ShoppingCartVM.orderHeader.OrderStatus = SD.OrderStatusCancelled;
                }
                else
                    ShoppingCartVM.orderHeader.TransactionId = charge.BalanceTransactionId;
                if (charge.Status.ToLower() == "succeeded")
                {
                    ShoppingCartVM.orderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    ShoppingCartVM.orderHeader.OrderStatus = SD.OrderStatusApproved;
                    ShoppingCartVM.orderHeader.OrderDate = DateTime.Now;

                    // Send well-structured order confirmation email to the customer
                    string subject = "Order Confirmation - Shopping App";
                    StringBuilder messageBuilder = new StringBuilder();
                    messageBuilder.AppendLine($"Dear {ShoppingCartVM.orderHeader.ApplicationUser.Name},");
                    messageBuilder.AppendLine();
                    messageBuilder.AppendLine("Thank you for placing an order with Shopping App. Your order details are as follows:");

                    foreach (var orderDetail in ShoppingCartVM.Listcart)
                    {
                        messageBuilder.AppendLine($"Order Id: {ShoppingCartVM.orderHeader.Id}");
                        messageBuilder.AppendLine($"Product: {orderDetail.Product.Title}");
                        messageBuilder.AppendLine($"Quantity: {orderDetail.Count}");
                        messageBuilder.AppendLine($"Price: ${orderDetail.Price}");
                        messageBuilder.AppendLine();
                    }

                    messageBuilder.AppendLine($"Total Amount: ${ShoppingCartVM.orderHeader.OrderTotal}");
                    messageBuilder.AppendLine();
                    messageBuilder.AppendLine("Thank you for shopping with us!");
                    messageBuilder.AppendLine();
                    messageBuilder.AppendLine("Sincerely,");
                    messageBuilder.AppendLine("Rana Book store App Team");

                    // Use the EmailSender to send the email
                    await _emailSender.SendEmailAsync(ShoppingCartVM.orderHeader.ApplicationUser.Email, subject, messageBuilder.ToString());

                    string orderDetails = GenerateOrderDetailsForSMS(ShoppingCartVM); // Implement this method to generate order details
                    await _twilioService.SendOrderConfirmationSMS(ShoppingCartVM.orderHeader.PhoneNumber, orderDetails);
                    _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.Listcart);
                    _unitOfWork.save();

                    var ClaimIdentidty = (ClaimsIdentity)User.Identity;
                    var Claims = ClaimIdentidty.FindFirst(ClaimTypes.NameIdentifier);
                    var Count = _unitOfWork.ShoppingCart.GetAll(SC => SC.ApplicationUserId == Claims.Value).ToList().Count;
                    HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, Count);
                }
                _unitOfWork.save();
                #endregion
            }
            #endregion

            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.orderHeader.Id });
        }

        private string GenerateOrderDetailsForSMS(ShoppingCartVM shoppingCartVM)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Order Confirmation - Shopping App");
            sb.AppendLine($"Dear {shoppingCartVM.orderHeader.ApplicationUser.Name},");
            sb.AppendLine($"Thank you for placing an order with Shopping App. Your order details are as follows:");

            foreach (var orderDetail in shoppingCartVM.Listcart)
            {
                sb.AppendLine($"Order ID:{shoppingCartVM.orderHeader.Id},Product: {orderDetail.Product.Title}, Quantity: {orderDetail.Count}, Price: ${orderDetail.Price}");
            }

            sb.AppendLine($"Total Amount: ${shoppingCartVM.orderHeader.OrderTotal}");
            sb.AppendLine("Thank you for shopping with us!");

            return sb.ToString();
        }

        public async Task< IActionResult> OrderConfirmation(int id)
        {
           
            
            return View(id);
        }
    }
}