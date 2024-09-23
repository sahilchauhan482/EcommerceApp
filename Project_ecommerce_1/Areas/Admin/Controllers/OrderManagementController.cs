using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Project_ecommerce_1.DataAccess.Repository.IRepository;
using Project_ecommerce_1.Model;
using Project_ecommerce_1.Model.ViewModels;
using Project_ecommerce_1.Utility;
using System.Security.Claims;
using System.Text;

namespace Project_ecommerce_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class OrderManagementController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly TwilioService _twilioService;
        public OrderManagementController(IUnitOfWork unitOfWork,IEmailSender emailSender, TwilioService twilioService)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _twilioService = twilioService;

        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAllOrders()
        {
            var allOrders = _unitOfWork.OrderHeader.GetAll(IncludeProperties: "ApplicationUser");

            var orderHeaderVM = allOrders.Select(order => new OrderHeaderVM
            {
                OrderId = order.Id,
                Status = order.OrderStatus,
                OrderDate = order.OrderDate,
                CustomerName = order.Name,
                PhoneNumber = order.PhoneNumber,
                CustomerEmail = order.ApplicationUser.Email,

            }).ToList();

            return Json(new { Data = orderHeaderVM });
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            // Get the order details by OrderId
            var orderDetails = _unitOfWork.OrderDetail
                .GetAll(filter: od => od.OrderHeaderId == id, IncludeProperties: "Product");

            var orderDetailsVM = orderDetails.Select(od => new OrderDetailsVM
            {
                OrderID = od.OrderHeaderId,
                ProductName = od.Product.Title,
                Price = (decimal)od.Price,
                Quantity = od.Count
            }).ToList();

            return View(orderDetailsVM);
        }

        public IActionResult CancelOrder(int id)
        {
            
            
            var delete = _unitOfWork.OrderHeader.FirstOrDefault(x => x.Id == id,IncludeProperties: "ApplicationUser");
            if (delete == null) return NotFound();
            var orderDetails = _unitOfWork.OrderDetail.GetAll(filter: o => o.OrderHeaderId == id, IncludeProperties: "OrderHeader,Product");
            // Send SMS notification
            string cancellationCause = "Due to unavailability of stock";

            // Customize the SMS body in the action
            string smsBody = $"Order Cancellation - Rana Shopping App\n\n{GetOrderDetailsText(orderDetails)}\n\nCancellation Cause: " +
                $"{cancellationCause}\n\nWe regret to inform you that your order has been canceled due to unavailability of stock. We apologize for any inconvenience caused by the cancellation.";

            _twilioService.SendOrderConfirmationSMS(delete.ApplicationUser.PhoneNumber, smsBody);
            _unitOfWork.OrderHeader.Remove(delete);
            SendCancellationEmail(delete.ApplicationUser.Email, delete.Id, orderDetails.ToList());
            _unitOfWork.save();
            return RedirectToAction("Index");
        }

        private string GetOrderDetailsText(IEnumerable<OrderDetail> orderDetails)
        {
            StringBuilder orderDetailsText = new StringBuilder();

            // Customize the order details text based on your requirements
            foreach (var orderDetail in orderDetails)
            {
                orderDetailsText.AppendLine($"Product: {orderDetail.Product.Title}, Quantity: {orderDetail.Count}, Price: ${orderDetail.Price}");
            }

            orderDetailsText.AppendLine($"Total Amount: ${orderDetails.First().OrderHeader.OrderTotal}");

            return orderDetailsText.ToString();
        }





        private void SendCancellationEmail(string customerEmail, int orderId, List<OrderDetail> orderDetails)
        {
            try
            {
                string subject = "Order Cancellation Notification";
                StringBuilder messageBuilder = new StringBuilder();

                // Begin HTML content
                messageBuilder.AppendLine("<!DOCTYPE html>");
                messageBuilder.AppendLine("<html lang=\"en\">");
                messageBuilder.AppendLine("<head>");
                messageBuilder.AppendLine("<meta charset=\"UTF-8\">");
                messageBuilder.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
                messageBuilder.AppendLine("<title>Order Cancellation</title>");
                messageBuilder.AppendLine("</head>");
                messageBuilder.AppendLine("<body>");

                // Email body
                messageBuilder.AppendLine($"<p>Dear {orderDetails.First().OrderHeader.ApplicationUser.Name},</p>");
                messageBuilder.AppendLine("<p>We regret to inform you that your order with ID " + $"{orderId} has been canceled due to unavailability of stock. The details are as follows:</p>");

                messageBuilder.AppendLine("<table border=\"1\">");
                foreach (var orderDetail in orderDetails)
                {
                    messageBuilder.AppendLine("<tr>");
                    messageBuilder.AppendLine($"<td>Product: {orderDetail.Product.Title}</td>");
                    messageBuilder.AppendLine($"<td>Quantity: {orderDetail.Count}</td>");
                    messageBuilder.AppendLine($"<td>Price: ${orderDetail.Price}</td>");
                    messageBuilder.AppendLine("</tr>");
                }
                messageBuilder.AppendLine("</table>");

                messageBuilder.AppendLine($"<p>Total Amount: ${orderDetails.First().OrderHeader.OrderTotal}</p>");
                messageBuilder.AppendLine("<p>We apologize for any inconvenience caused by the cancellation.</p>");
                messageBuilder.AppendLine("<p>Thank you for considering Shopping App.</p>");

                // Additional information or instructions can be added as needed

                messageBuilder.AppendLine("<p>Sincerely,</p>");
                messageBuilder.AppendLine("<p>Rana Bookstore App Team</p>");

                // End HTML content
                messageBuilder.AppendLine("</body>");
                messageBuilder.AppendLine("</html>");

                // Use the EmailSender to send the HTML-formatted email
                _emailSender.SendEmailAsync(customerEmail, subject, messageBuilder.ToString());
            }
            catch (Exception ex)
            {
                // Handle email sending failure (log, return error message, etc.)
            }
        }

        [HttpPost]

        public IActionResult DispatchOrder([FromBody] OrderHeader model)
        {
            var fordispatch = _unitOfWork.OrderHeader.FirstOrDefault(x => x.Id == model.Id);

            if (fordispatch == null) return BadRequest("Error dispatching order. Please try again.");
            {
                fordispatch.OrderStatus = SD.OrderStatusShipped;
                fordispatch.TrackingNumber = model.TrackingNumber;
                fordispatch.Carrier = model.Carrier;

                _unitOfWork.save();

                return Ok(new { message = "Order dispatched successfully." });
                
            }

            
        }
    

     

        
    }
}

