﻿using Braintree;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Linq;
using System;
using WebApplication1.Models;
using WebApplication1.Repository.IRepository;
using WebApplication1.Utility.BrainTree;
using WebApplication1.Models.ViewModels;

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

        public OrderController(
        IOrderHeaderRepository orderHRepo, IOrderDetailRepository orderDRepo, IBrainTreeGate brain)
        {
            _brain = brain;
            _orderDRepo = orderDRepo;
            _orderHRepo = orderHRepo;
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
        public IActionResult ShipOrder(OrderVM orderVM)
        {
            var orderHeaderFromDb = _orderHRepo.FirstOrDefault(u => u.Id == orderVM.OrderHeader.Id);
            if (orderHeaderFromDb != null)
            {
                orderHeaderFromDb.OrderStatus = WC.StatusShipped;

                if (DateTime.TryParse(orderVM.OrderHeader.ShippingDate.ToString(), out DateTime parsedDate))
                {
                    orderHeaderFromDb.ShippingDate = parsedDate;
                }

                _orderHRepo.Save();
            }

            return RedirectToAction("Details", new { id = orderHeaderFromDb.Id });
        }

        [HttpPost]
        public IActionResult CancelOrder()
        {
            OrderHeader orderHeader = _orderHRepo.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);

            var gateway = _brain.GetGateway();
            Transaction transaction = gateway.Transaction.Find(orderHeader.TransactionId);

            if (transaction.Status == TransactionStatus.AUTHORIZED || transaction.Status == TransactionStatus.SUBMITTED_FOR_SETTLEMENT)
            {
                //no refund
                Result<Transaction> resultvoid = gateway.Transaction.Void(orderHeader.TransactionId);
            }
            else
            {
                //refund
                Result<Transaction> resultRefund = gateway.Transaction.Refund(orderHeader.TransactionId);
            }
            orderHeader.OrderStatus = WC.StatusCancelled;
            _orderHRepo.Save();
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

