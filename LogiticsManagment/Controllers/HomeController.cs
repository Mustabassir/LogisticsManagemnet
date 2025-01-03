using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web.Mvc;
using LogiticsManagment.Interfaces;
using LogiticsManagment.Models;
using LogiticsManagment.Observers;
using LogiticsManagment.ViewModels;
using Microsoft.Ajax.Utilities;

namespace LogiticsManagment.Controllers
{
    public class HomeController : Controller
    {
        private readonly LogisticManagmentEntities1 _context;

        public HomeController(LogisticManagmentEntities1 context)
        {
            _context = context;
        }
        public LogisticManagmentEntities1 db = new LogisticManagmentEntities1();

        [HttpGet]
        public ActionResult ListOrders()
        {
            var orders = db.Orders
        .Select(o => new OrdersViewModel
        {
            OrderID = o.order_id,
            OrderDate = o.order_date,
            CustomerID = o.customer_id, 
            Status = o.Status,
            DeliveryCharges = o.delivery_charges 
        })
        .ToList();

            return View(orders);
        }
        public ActionResult Index()
        {
            return RedirectToAction("ListOrders");
        }

        private Orders order = new Orders();
        private List<IOrderObserver> _observers = new List<IOrderObserver>
        {
            new OrderCreatedObserver(),
            new PackageDetailCapturedObserver(),
            new ShipmentCreatedObserver(),
            new PaymentProcessedObserver()
        };

        public HomeController()
        {
            foreach (var observer in _observers)
            {
                order.Attach(observer);
            }
        }

        public ActionResult CustomerDetail()
        {
            return View("Index", new OrderViewModel());
        }

        [HttpPost]
        public ActionResult CustomerDetail(OrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new LogisticManagmentEntities1())
                {
                    db.Customers.Add(model.Customer);
                    db.Orders.Add(model.Order);
                    db.SaveChanges();

                    order.OrderId = model.Order.order_id;
                    order.NotifyObservers();

                    TempData["Notification"] = "Order Created for: "+order.OrderId;
                }
                return RedirectToAction("PackageDetail");
            }
            return View("Index", model);
        }

        [HttpGet]
        public ActionResult PackageDetail()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PackageDetail(OrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new LogisticManagmentEntities1())
                {
                    var lastOrder = db.Orders.OrderByDescending(p => p.order_id).FirstOrDefault();
                    model.Package.order_id = lastOrder != null ? lastOrder.order_id : 100;
                    db.Packages.Add(model.Package);
                    db.SaveChanges();

                    order.OrderId = model.Package.order_id??0;
                    order.NotifyObservers();

                    TempData["Notification"] = "Package details captured"; 
                }
                return RedirectToAction("ShipmentDetail");
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult ShipmentDetail()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ShipmentDetail(OrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new LogisticManagmentEntities1())
                {
                    var lastPackage = db.Packages.OrderByDescending(p => p.package_id).FirstOrDefault();
                    var lastCustomer = db.Customers.OrderByDescending(c => c.customer_id).FirstOrDefault();

                    model.Shipment.package_id = lastPackage != null ? lastPackage.package_id : 10;
                    model.Shipment.customer_id = lastCustomer != null ? lastCustomer.customer_id : 1000;
                    model.Shipment.shipment_date = DateTime.Now;

                    DeliveryDateCalculator deliveryDateCalculator = new DeliveryDateCalculator();
                    model.Shipment.estimated_delivery_date = deliveryDateCalculator.CalculateEstimatedDeliveryDate(model.Shipment.source_city_name, model.Shipment.destination_city_name, model.Shipment.delivery_method);

                    db.Shipments.Add(model.Shipment);
                    db.SaveChanges();

                    order.OrderId = model.Shipment.package_id??0;
                    order.NotifyObservers();

                    TempData["Notification"] = "Shipment details captured"; 
                }

                return RedirectToAction("PaymentDetail");
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult PaymentDetail()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PaymentDetail(OrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new LogisticManagmentEntities1())
                {
                    var lastCustomer = db.Customers.OrderByDescending(p => p.customer_id).FirstOrDefault();
                    var maxOrderId = db.Orders.Max(p => p.order_id);
                    var lastOrder = db.Orders.FirstOrDefault(p => p.order_id == maxOrderId);

                    model.Payment.order_id = lastOrder != null ? lastOrder.order_id : 100;

                    lastOrder.delivery_charges = model.Payment.amount;
                    lastOrder.customer_id = lastCustomer != null ? lastCustomer.customer_id : 1000;
                    lastOrder.Status = "Processing";
                    lastOrder.order_date = DateTime.Now;

                    db.Payments.Add(model.Payment);
                    db.SaveChanges();

                    order.OrderId = model.Payment.order_id ?? 0;
                    order.NotifyObservers();

                    TempData["Notification"] = HttpContext.Session["Notification"];
                }
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult OrderSuccess()
        {
            return View();
        }

        public ActionResult EditOrder(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditOrder([Bind(Include = "order_id,customer_id,order_date,delivery_charges,Status")] Order order)
        {
            if (ModelState.IsValid)
            {
                // Update Order table
                db.Entry(order).State = EntityState.Modified;

                // Update related Payment table
                Payment payment = db.Payments.FirstOrDefault(p => p.order_id == order.order_id);
                if (payment != null)
                {
                    if (order.delivery_charges.HasValue)
                    {
                        payment.amount = order.delivery_charges.Value; // Explicitly access the value
                    }
                    else
                    {
                        payment.amount = 0;
                    }
                    db.Entry(payment).State = EntityState.Modified;
                }

                db.SaveChanges();
                return RedirectToAction("ListOrders");
            }

            return View(order);
        }

        public ActionResult CompleteOrder(int id)
        {
            var order = db.Orders.SingleOrDefault(o => o.order_id == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            order.Status = "Completed";
            db.SaveChanges();

            return RedirectToAction("ListOrders");
        }
        public ActionResult Detail(int id)
        {
            
            var order = db.Orders.SingleOrDefault(o => o.order_id == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            var customer = db.Customers.SingleOrDefault(c => c.customer_id == order.customer_id);

            var shipment = db.Shipments.FirstOrDefault(s => s.customer_id == customer.customer_id);

            var package = db.Packages.FirstOrDefault(p => p.order_id == order.order_id);

            var payment = db.Payments.FirstOrDefault(p => p.order_id == order.order_id);

            var viewModel = new OrderViewModel
            {
                Customer = customer,
                Shipment = shipment,
                Package = package,
                Payment = payment
            };

            return View(viewModel);
        }

        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new LogisticManagmentEntities1())
                {
                    var admin = context.admins.FirstOrDefault(a => a.username == model.Username);

                    if (admin != null)
                    {
                        if (admin.password == model.Password)
                        {
                            Session["AdminId"] = admin.username; 
                            return RedirectToAction("Index"); 
                        }
                    }
                    ModelState.AddModelError("", "Invalid username or password");
                }
            }
            return View(model);
        }

    }
}