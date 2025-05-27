using Braintree;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Linq;
using System;
using WebApplication1.Models;
using WebApplication1.Repository.IRepository;
using WebApplication1.Utility.BrainTree;
using WebApplication1.Models.ViewModels;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class OrderController : Controller
    {
        private readonly IOrderHeaderRepository _orderHRepo;
        private readonly IOrderDetailRepository _orderDRepo;
        private readonly IBrainTreeGate _brain;

        [BindProperty]
        public OrderVM OrderVM { get; set; }

        private readonly IEmailSender _emailSender;

        public OrderController(
            IOrderHeaderRepository orderHRepo,
            IOrderDetailRepository orderDRepo,
            IBrainTreeGate brain,
            IEmailSender emailSender)
        {
            _brain = brain;
            _orderDRepo = orderDRepo;
            _orderHRepo = orderHRepo;
            _emailSender = emailSender;
        }



        public IActionResult Index(string searchName = null, string searchEmail = null, string searchPhone = null, string Status = null)
        {
            OrderListVM orderListVM = new OrderListVM()
            {
                OrderHList = _orderHRepo.GetAll(),
                StatusList = WC.listStatus.ToList().Select(i => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = i,
                    Value = i
                })
            };

            if (!string.IsNullOrEmpty(searchName))
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.FullName.ToLower().Contains(searchName.ToLower()));
            }
            if (!string.IsNullOrEmpty(searchEmail))
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.Email.ToLower().Contains(searchEmail.ToLower()));
            }
            if (!string.IsNullOrEmpty(searchPhone))
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.PhoneNumber.ToLower().Contains(searchPhone.ToLower()));
            }
            if (!string.IsNullOrEmpty(Status) && Status != "--Order Status--")
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.OrderStatus.ToLower().Contains(Status.ToLower()));
            }

            return View(orderListVM);
        }


        public IActionResult Details(int id)
        {
            OrderVM = new OrderVM()
            {
                OrderHeader = _orderHRepo.FirstOrDefault(u => u.Id == id),
                OrderDetail = _orderDRepo.GetAll(o => o.OrderHeaderId == id, includeProperties: "Product")
            };

            return View(OrderVM);
        }

        [HttpPost]
        public IActionResult StartProcessing()
        {
            OrderHeader orderHeader = _orderHRepo.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.OrderStatus = WC.StatusInProcess;
            _orderHRepo.Save();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> ShipOrder()
        {
            OrderHeader orderHeader = _orderHRepo.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.OrderStatus = WC.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            _orderHRepo.Save();
            // Retrieve ordered items
            var orderDetails = _orderDRepo
                .GetAll(o => o.OrderHeaderId == orderHeader.Id, includeProperties: "Product")
                .ToList();

            // Generate book list with prices
            string bookListHtml = string.Join("<br/>", orderDetails.Select(d =>
                $"- <strong>{d.Product.Name}</strong> (x{d.Piece}) — {d.PricePerPiece:C} each"));

            // Calculate total
            decimal total = (decimal)orderDetails.Sum(d => d.Piece * d.PricePerPiece);

            // Create styled email
            string subject = "Your order has been shipped!";
            string body = $@"
               <div style='font-family:Arial, sans-serif; color:#333;'>
               <h2 style='color:#2c3e50;'>📚 BookShop Shipping Confirmation</h2>
               <p>Dear <strong>{orderHeader.FullName}</strong>,</p>
               <p>Your order has been <span style='color:green; font-weight:bold;'>shipped</span> on 
               <strong>{orderHeader.ShippingDate.ToString("MMMM dd, yyyy")}</strong>.</p>
               <p><strong>Ordered Book(s):</strong><br/>
                        {bookListHtml}
               </p>

                <p><strong>Total Amount:</strong> {total:C}</p>

                <p style='margin-top:20px;'>Thank you for shopping with us!<br/>
                     — The BookShop Team</p>
                </div>";

            await _emailSender.SendEmailAsync(orderHeader.Email, subject, body);

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> CancelOrder()
        {
            OrderHeader orderHeader = _orderHRepo.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);

            var gateway = _brain.GetGateway();
            Transaction transaction = gateway.Transaction.Find(orderHeader.TransactionId);

            if (transaction.Status == TransactionStatus.AUTHORIZED || transaction.Status == TransactionStatus.SUBMITTED_FOR_SETTLEMENT)
            {
                // no refund
                Result<Transaction> resultVoid = gateway.Transaction.Void(orderHeader.TransactionId);
            }
            else
            {
                // refund
                Result<Transaction> resultRefund = gateway.Transaction.Refund(orderHeader.TransactionId);
            }

            orderHeader.OrderStatus = WC.StatusCancelled;
            _orderHRepo.Save();

            // Send cancellation email
            string subject = $"Order Cancelled";
            string body = $"<p>Dear {orderHeader.FullName},</p>" +
                          $"<p>Your order placed on {orderHeader.OrderDate.ToString("MMMM dd, yyyy")} has been <strong>cancelled</strong>.</p>" +
                          $"<p>If you have any questions, feel free to contact us.</p>";

            await _emailSender.SendEmailAsync(orderHeader.Email, subject, body);

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetails()
        {
            
            
                if (!ModelState.IsValid)
                {
                    return View(OrderVM);  
                }

                var orderHeaderFromDb = _orderHRepo.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
                if (orderHeaderFromDb == null)
                {
                    return NotFound();
                }

                orderHeaderFromDb.FullName = OrderVM.OrderHeader.FullName;
                orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
                orderHeaderFromDb.Email = OrderVM.OrderHeader.Email;

                _orderHRepo.Save();

                return RedirectToAction("Details", "Order", new { id = orderHeaderFromDb.Id });
            }
        }
    }

