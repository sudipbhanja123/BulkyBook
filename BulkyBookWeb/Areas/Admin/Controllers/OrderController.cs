using BulkyBook.Business.Services.IServices;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        [BindProperty]
        public OrderHeader OrderHeader { get; set; }

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;

        }
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {

            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int orderId)
        {
            OrderHeader = await _orderService.GetOrderByIdAsync(orderId, includeDetails: true, includeUser: true);
            return View(OrderHeader);
        }

        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        public async Task<IActionResult> UpdateOrderDetails()
        {

            var orderHeaderFromDb = await _orderService.GetOrderByIdAsync(OrderHeader.Id);
            orderHeaderFromDb.Name = OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderHeader.City;
            orderHeaderFromDb.State = OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderHeader.Carrier) && orderHeaderFromDb.OrderStatus == SD.StatusShipped)
            {
                orderHeaderFromDb.Carrier = OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderHeader.TrackingNumber) && orderHeaderFromDb.OrderStatus == SD.StatusShipped)
            {
                orderHeaderFromDb.TrackingNumber = OrderHeader.TrackingNumber;
            }

            await _orderService.UpdateOrderAsync(orderHeaderFromDb);
            TempData["Success"] = "Order Details Updated Successfully";

            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        public async Task<IActionResult> UpdateOrderStatus(string status)
        {

            var orderHeader = await _orderService.GetOrderByIdAsync(OrderHeader.Id);
            if (orderHeader == null)
            {
                TempData["Error"] = "Order not found";
                return RedirectToAction(nameof(Index));
            }
            string successMessage;
            switch (status)
            {
                case SD.StatusInProcess:
                    await _orderService.UpdateOrderStatusAsyncx(OrderHeader.Id, status);
                    successMessage = "Order is now in process";
                    break;
                case SD.StatusShipped:
                    if (string.IsNullOrEmpty(OrderHeader.Carrier) || string.IsNullOrEmpty(OrderHeader.TrackingNumber))
                    {
                        TempData["Error"] = "Please provide both carrier and tracking number.";
                        return RedirectToAction(nameof(Details), new { orderId = orderHeader.Id });
                    }
                    await _orderService.UpdateOrderStatusAsyncx(OrderHeader.Id, SD.StatusShipped, OrderHeader.Carrier, OrderHeader.TrackingNumber);
                    successMessage = "Order has been shipped";
                    break;

                case SD.StatusCancelled:
                    await _orderService.UpdateOrderStatusAsyncx(OrderHeader.Id, status);
                    successMessage = "Order has been cancelled";
                    break;
                default:
                    TempData["Error"] = "Invalid status";
                    return RedirectToAction(nameof(Details), new { orderId = OrderHeader.Id });
            }
            TempData["Success"] = successMessage;

            return RedirectToAction(nameof(Details), new { orderId = OrderHeader.Id });
        }









        #region API CALLS
        public async Task<IActionResult> GetAll(string status)
        {
            string? userId = null;
            if (!User.IsInRole(SD.RoleAdmin) && !User.IsInRole(SD.RoleEmployee))
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }
            }
            var orders = await _orderService.GetAllOrderAsync(userId, status);
            return Json(new { data = orders });
        }

        #endregion
    }
}
