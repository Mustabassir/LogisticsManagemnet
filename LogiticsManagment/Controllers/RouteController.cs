using LogiticsManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LogiticsManagment.Controllers
{
    public class RouteController : Controller
    {
       public LogisticManagmentEntities1 db = new LogisticManagmentEntities1();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ListRoutes()
        {
            var routes = db.Routes.ToList(); 
            return View(routes);
        }
        public ActionResult AddRoute()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRoute(Route route)
        {
            if (ModelState.IsValid)
            {
                db.Routes.Add(route);
                db.SaveChanges();
                return RedirectToAction(nameof(ListRoutes));
            }
            return View(route);
        }

        public ActionResult Delete(int id)
        {
            var route = db.Routes.Find(id);
            if (route == null)
            {
                return HttpNotFound();
            }
            return View(route);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var route = db.Routes.Find(id);
            if (route == null)
            {
                return HttpNotFound();
            }

            db.Routes.Remove(route);
            db.SaveChanges(); 

            return RedirectToAction("ListRoutes"); 
        }
        public ActionResult Edit(int id)
        {
            var route = db.Routes.Find(id);
            if (route == null)
            {
                return HttpNotFound();
            }
            return View(route);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Route route)
        {
            if (ModelState.IsValid)
            {
                var routes = db.Routes.Find(route.route_id);
                routes.route_link = route.route_link;
                db.SaveChanges(); 
                return RedirectToAction("ListRoutes"); 
            }
            return View(route);
        }
        
        public ActionResult AssignRoute(int id)
        {
            var route = db.Routes.Find(id);

            var vehicles = db.Vehicles.ToList();

            var viewModel = new VehicleRoute
            {
                route_id = route.route_id,
                assignment_date = DateTime.Now,
            };

            ViewBag.Vehicles = new SelectList(vehicles, "vehicle_id", "vehicle_id");

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignRoute(VehicleRoute viewModel)
        {
            if (ModelState.IsValid)
            {
                var vehicleRoute = new VehicleRoute
                {
                    vehicle_id = viewModel.vehicle_id,
                    route_id = viewModel.route_id,
                    assignment_date = DateTime.Now
                };

                db.VehicleRoutes.Add(vehicleRoute);
                db.SaveChanges();

                return RedirectToAction("ListRoutes");
            }


            var vehicles = db.Vehicles.ToList();
            ViewBag.Vehicles = new SelectList(vehicles, "vehicle_id", "vehicle_name");

            return View(viewModel);
        }
    }
}
