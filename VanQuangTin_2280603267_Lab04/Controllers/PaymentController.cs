using GearShop.Models;
using GearShop.Services.Momo;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VanQuangTin_2280603267_Lab04.Models;

namespace GearShop.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IMomoService _momoService;
        private readonly ApplicationDbContext _context;

        public PaymentController(IMomoService momoService, ApplicationDbContext context)
        {
            _momoService = momoService;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
        {
            var response = await _momoService.CreatePaymentAsync(model);

            if (response == null || string.IsNullOrEmpty(response.PayUrl))
            {
                TempData["error"] = "Không thể tạo URL thanh toán. Vui lòng thử lại.";
                return RedirectToAction("Checkout", "ShoppingCart"); // hoặc một trang phù hợp
            }

            return Redirect(response.PayUrl);
        }


        [HttpGet]
        public async Task<IActionResult> PaymentCallBack(MomoInfoModel model)
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            var requestQuery = HttpContext.Request.Query;
            //if (requestQuery["resultCode"] != "0")
            //{
            //    var newMomoInsert = new MomoInfoModel
            //    {
            //        OrderId = requestQuery["orderId"],
            //        FullName = User.FindFirstValue(ClaimTypes.Email),
            //        Amount = decimal.Parse(requestQuery["amount"]),
            //        OrderInfo = requestQuery["orderInfo"],
            //        DatePaid = DateTime.Now
            //    };

            //    _context.Add(newMomoInsert);
            //    await _context.SaveChangesAsync();
            //}
            //else
            //{
            //    TempData["success"] = "Giao dịch không thành công.";
            //    return RedirectToAction("OrderCompleted", "ShoppingCart");
            //}

            ////var checkoutResult = await Checkout(requestQuery["orderId"]);
            return View(response);
        }

    }
}