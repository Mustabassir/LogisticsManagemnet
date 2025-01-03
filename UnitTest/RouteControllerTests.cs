using LogiticsManagment.Controllers;
using LogiticsManagment.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace UnitTest
{
    [TestClass]
    public class RouteControllerTests
    {
        private Mock<LogisticManagmentEntities1> _mockDbContext;
        private RouteController _controller;

        [TestInitialize]
        public void TestInitialize()
        {
            // Initialize mocks
            _mockDbContext = new Mock<LogisticManagmentEntities1>();
            _controller = new RouteController
            {
                db = _mockDbContext.Object // Injecting the mocked db context
            };
        }

        [TestMethod]
        public void Index_Returns_ViewResult()
        {
            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ListRoutes_Returns_ViewResult_With_ListOfRoutes()
        {
            // Arrange
            var routes = new List<Route>
    {
        new Route { route_id = 1, route_link = "Route1" },
        new Route { route_id = 2, route_link = "Route2" }
    };

            _mockDbContext.Setup(db => db.Routes.ToList()).Returns(routes);  // Mock routes list

            // Act
            var result = _controller.ListRoutes() as ViewResult;

            // Assert
            Assert.IsNotNull(result);  // Ensure the result is a ViewResult

            // Check the model of the ViewResult
            var model = result.Model as List<Route>;
            Assert.IsNotNull(model);  // Ensure the model is a list of Route
            Assert.AreEqual(2, model.Count);  // Ensure the list has 2 routes
            Assert.AreEqual("Route1", model[0].route_link);  // Check the first route's route_link
        }


        [TestMethod]
        public void AddRoute_Get_Returns_ViewResult()
        {
            // Act
            var result = _controller.AddRoute() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void AddRoute_Post_Redirects_To_ListRoutes_When_ModelIsValid()
        {
            // Arrange
            var route = new Route { route_id = 1, route_link = "NewRoute" };

            _mockDbContext.Setup(db => db.Routes.Add(It.IsAny<Route>()));
            _mockDbContext.Setup(db => db.SaveChanges());

            // Act
            var result = _controller.AddRoute(route) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ListRoutes", result.RouteValues["action"]);
        }

        [TestMethod]
        public void AddRoute_Post_Returns_ViewResult_When_ModelIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("route_link", "Required");
            var route = new Route { route_id = 1, route_link = "" };

            // Act
            var result = _controller.AddRoute(route) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(route, result.Model);
        }

        [TestMethod]
        public void Delete_Get_Returns_ViewResult_With_Route()
        {
            // Arrange
            var route = new Route { route_id = 1, route_link = "Route1" };
            _mockDbContext.Setup(db => db.Routes.Find(1)).Returns(route);

            // Act
            var result = _controller.Delete(1) as ViewResult;

            // Assert
            var model = result.Model as Route;
            Assert.IsNotNull(model);
            Assert.AreEqual(route.route_id, model.route_id);
        }

        [TestMethod]
        public void Delete_Post_Redirects_To_ListRoutes_When_RouteIsFound()
        {
            // Arrange
            var route = new Route { route_id = 1, route_link = "Route1" };
            _mockDbContext.Setup(db => db.Routes.Find(1)).Returns(route);
            _mockDbContext.Setup(db => db.Routes.Remove(route));
            _mockDbContext.Setup(db => db.SaveChanges());

            // Act
            var result = _controller.DeleteConfirmed(1) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ListRoutes", result.RouteValues["action"]);
        }

        [TestMethod]
        public void Edit_Get_Returns_ViewResult_With_Route()
        {
            // Arrange
            var route = new Route { route_id = 1, route_link = "Route1" };
            _mockDbContext.Setup(db => db.Routes.Find(1)).Returns(route);

            // Act
            var result = _controller.Edit(1) as ViewResult;

            // Assert
            var model = result.Model as Route;
            Assert.IsNotNull(model);
            Assert.AreEqual(route.route_id, model.route_id);
        }

        [TestMethod]
        public void Edit_Post_Redirects_To_ListRoutes_When_ModelIsValid()
        {
            // Arrange
            var route = new Route { route_id = 1, route_link = "UpdatedRoute" };
            _mockDbContext.Setup(db => db.Routes.Find(1)).Returns(new Route { route_id = 1, route_link = "Route1" });
            _mockDbContext.Setup(db => db.SaveChanges());

            // Act
            var result = _controller.Edit(route) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ListRoutes", result.RouteValues["action"]);
        }

        [TestMethod]
        public void AssignRoute_Get_Returns_ViewResult_With_Vehicles()
        {
            // Arrange
            var route = new Route { route_id = 1, route_link = "Route1" };
            var vehicles = new List<Vehicle> { new Vehicle { vehicle_id = 1 } };

            _mockDbContext.Setup(db => db.Routes.Find(1)).Returns(route);  // Mock route
            _mockDbContext.Setup(db => db.Vehicles.ToList()).Returns(vehicles);  // Mock vehicles list

            // Act
            var result = _controller.AssignRoute(1) as ViewResult;

            // Assert
            Assert.IsNotNull(result);

            // Check the model of the ViewResult
            var model = result.Model as VehicleRoute;
            Assert.IsNotNull(model);
            Assert.AreEqual(route.route_id, model.route_id);  // Ensure the route id is passed

            // Check the ViewBag.Vehicles for vehicles data
            var vehiclesList = result.ViewBag.Vehicles as SelectList;
            Assert.IsNotNull(vehiclesList);
            Assert.AreEqual(1, vehiclesList.Count());
            Assert.AreEqual("Vehicle1", vehiclesList.First().Text);  // Verify vehicle name in the list
        }


        [TestMethod]
        public void AssignRoute_Post_Redirects_To_ListRoutes_When_ModelIsValid()
        {
            // Arrange
            var vehicleRoute = new VehicleRoute { vehicle_id = 1, route_id = 1, assignment_date = DateTime.Now };
            _mockDbContext.Setup(db => db.VehicleRoutes.Add(It.IsAny<VehicleRoute>()));
            _mockDbContext.Setup(db => db.SaveChanges());

            // Act
            var result = _controller.AssignRoute(vehicleRoute) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ListRoutes", result.RouteValues["action"]);
        }
    }
}
