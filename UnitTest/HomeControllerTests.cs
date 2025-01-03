using LogiticsManagment.Controllers;
using LogiticsManagment.Models;
using LogiticsManagment.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq.Expressions;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Data.Entity;

namespace UnitTest
{
    [TestClass]
    public class HomeControllerTests
    {
        private Mock<LogisticManagmentEntities1> _mockContext;
        private Mock<DbSet<Order>> _mockOrdersSet;
        private Mock<DbSet<Payment>> _mockPaymentsSet;
        private Mock<DbSet<Customer>> _mockCustomersSet;
        private Mock<DbSet<Shipment>> _mockShipmentsSet;
        private Mock<DbSet<Package>> _mockPackagesSet;
        private Mock<DbSet<admin>> _mockAdminSet;
        private HomeController _controller;

        [TestInitialize]
        public void Initialize()
        {
            // Set up mock DbSets
            _mockContext = new Mock<LogisticManagmentEntities1>();
            _mockOrdersSet = new Mock<DbSet<Order>>();
            _mockPaymentsSet = new Mock<DbSet<Payment>>();
            _mockCustomersSet = new Mock<DbSet<Customer>>();
            _mockShipmentsSet = new Mock<DbSet<Shipment>>();
            _mockPackagesSet = new Mock<DbSet<Package>>();
            _mockAdminSet = new Mock<DbSet<admin>>();

            // Set up DbContext to return mocked DbSets
            _mockContext.Setup(c => c.Orders).Returns(_mockOrdersSet.Object);
            _mockContext.Setup(c => c.Payments).Returns(_mockPaymentsSet.Object);
            _mockContext.Setup(c => c.Customers).Returns(_mockCustomersSet.Object);
            _mockContext.Setup(c => c.Shipments).Returns(_mockShipmentsSet.Object);
            _mockContext.Setup(c => c.Packages).Returns(_mockPackagesSet.Object);
            _mockContext.Setup(c => c.admins).Returns(_mockAdminSet.Object);

            // Initialize controller with mocked context
            _controller = new HomeController(_mockContext.Object);
        }

        [TestMethod]
        public void Index_ReturnsView()
        {
            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ListOrders_ReturnsView()
        {
            // Act
            var result = _controller.ListOrders() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void OrderSuccess_ReturnsView()
        {
            // Act
            var result = _controller.OrderSuccess() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void EditOrder_ValidModel_ReturnsRedirectToAction()
        {
            // Arrange
            var order = new Order { order_id = 1 };
            var payment = new Payment { order_id = 1 };
            _mockOrdersSet.Setup(m => m.FirstOrDefault(It.IsAny<Expression<Func<Order, bool>>>())).Returns(order);
            _mockPaymentsSet.Setup(m => m.FirstOrDefault(It.IsAny<Expression<Func<Payment, bool>>>())).Returns(payment);

            // Act
            var result = _controller.EditOrder(order) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ListOrders", result.RouteValues["action"]);
        }

        [TestMethod]
        public void EditOrder_InvalidModel_ReturnsView()
        {
            // Arrange
            var order = new Order { order_id = 1 };
            _mockOrdersSet.Setup(m => m.FirstOrDefault(It.IsAny<Expression<Func<Order, bool>>>())).Returns(order);

            // Act
            var result = _controller.EditOrder(order) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CompleteOrder_ValidOrderId_ReturnsRedirectToAction()
        {
            // Arrange
            var order = new Order { order_id = 1 };
            _mockOrdersSet.Setup(m => m.SingleOrDefault(It.IsAny<Expression<Func<Order, bool>>>())).Returns(order);

            // Act
            var result = _controller.CompleteOrder(1) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ListOrders", result.RouteValues["action"]);
        }

        [TestMethod]
        public void CompleteOrder_InvalidOrderId_ReturnsHttpNotFound()
        {
            // Act
            var result = _controller.CompleteOrder(999) as HttpNotFoundResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Detail_ValidOrderId_ReturnsView()
        {
            // Arrange
            var order = new Order { order_id = 1 };
            var customer = new Customer { customer_id = 1 };
            var shipment = new Shipment { customer_id = 1 };
            var package = new Package { order_id = 1 };
            var payment = new Payment { order_id = 1 };

            _mockOrdersSet.Setup(m => m.SingleOrDefault(It.IsAny<Expression<Func<Order, bool>>>())).Returns(order);
            _mockCustomersSet.Setup(m => m.SingleOrDefault(It.IsAny<Expression<Func<Customer, bool>>>())).Returns(customer);
            _mockShipmentsSet.Setup(m => m.FirstOrDefault(It.IsAny<Expression<Func<Shipment, bool>>>())).Returns(shipment);
            _mockPackagesSet.Setup(m => m.FirstOrDefault(It.IsAny<Expression<Func<Package, bool>>>())).Returns(package);
            _mockPaymentsSet.Setup(m => m.FirstOrDefault(It.IsAny<Expression<Func<Payment, bool>>>())).Returns(payment);

            // Act
            var result = _controller.Detail(1) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Detail_InvalidOrderId_ReturnsHttpNotFound()
        {
            // Act
            var result = _controller.Detail(999) as HttpNotFoundResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Login_ValidModel_ReturnsRedirectToAction()
        {
            // Arrange
            var admin = new admin { username = "admin", password = "password" };
            _mockAdminSet.Setup(m => m.FirstOrDefault(It.IsAny<Expression<Func<admin, bool>>>())).Returns(admin);

            // Act
            var result = _controller.Login(new LoginViewModel { Username = "admin", Password = "password" }) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [TestMethod]
        public void Login_InvalidModel_ReturnsViewWithErrorMessage()
        {
            // Arrange
            _mockAdminSet.Setup(m => m.FirstOrDefault(It.IsAny<Expression<Func<admin, bool>>>())).Returns((admin)null);

            // Act
            var result = _controller.Login(new LoginViewModel { Username = "admin", Password = "wrongpassword" }) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ViewData.ModelState.ContainsKey("InvalidLogin"));
        }
    }
}
