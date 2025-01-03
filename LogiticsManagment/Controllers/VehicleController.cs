using LogiticsManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace LogiticsManagment.Controllers
{
    public class VehicleController : Controller
    {
        private LogisticManagmentEntities1 db = new LogisticManagmentEntities1();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ListVehicles()
        {
            var vehicles = db.Vehicles.ToList();
            return View(vehicles);
        }
        public ActionResult EditVehicle(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            Vehicle vehicle = db.Vehicles.Find(id);
            if (vehicle == null)
            {
                return HttpNotFound();
            }

            ViewBag.StatusList = new SelectList(new[] { "Available", "In Transit", "Under Maintenance" }, vehicle.status);

            ViewBag.CityList = new SelectList(db.Cities.Select(c => c.city_name), vehicle.vehicle_city_name);

            return View(vehicle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditVehicle([Bind(Include = "vehicle_id,status,vehicle_city_name")] Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                var update = db.Vehicles.Find(vehicle.vehicle_id);
                update.vehicle_city_name = vehicle.vehicle_city_name;
                update.status = vehicle.status;
                db.SaveChanges();
                return RedirectToAction("ListVehicles");
            }

            ViewBag.StatusList = new SelectList(new[] { "Available", "In Transit", "Under Maintenance" }, vehicle.status);
            ViewBag.CityList = new SelectList(db.Cities.Select(c => c.city_name), vehicle.vehicle_city_name);

            return View(vehicle);
        }


        public ActionResult AddVehicle()
        {
            // Prepare a list of city names for the dropdown
            ViewBag.vehicle_city_name = new SelectList(db.Cities, "city_name", "city_name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddVehicle([Bind(Include = "vehicle_number,status,vehicle_city_name")] Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                db.Vehicles.Add(vehicle);
                db.SaveChanges();
                return RedirectToAction("ListVehicles");
            }

            ViewBag.vehicle_city_name = new SelectList(db.Cities, "city_name", "city_name", vehicle.vehicle_city_name);
            return View(vehicle);
        }

        public ActionResult DeleteVehicle(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Vehicle vehicle = db.Vehicles.FirstOrDefault(v => v.vehicle_id == id);

            if (vehicle == null)
            {
                return HttpNotFound();
            }

            return View(vehicle);
        }

        [HttpPost, ActionName("DeleteVehicle")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Vehicle vehicle = db.Vehicles.Find(id);

            if (vehicle == null)
            {
                return HttpNotFound();
            }

            db.Vehicles.Remove(vehicle);
            db.SaveChanges();

            return RedirectToAction("ListVehicles");
        }

        public ActionResult DetailsVehicle(int id)
        {

            var shipmentVehicles = db.ShipmentVehicles
                                     .Where(sv => sv.vehicle_id == id)
                                     .ToList();
            ViewBag.VehicleId = id; 
            return View(shipmentVehicles);

        }

        public ActionResult LoadShipment(int id)
        {
            var shipmentList = db.Shipments
             .Where(s => s.Status == "Local Warehouse" || s.Status == "In Destination City Warehouse" || s.Status == "Pending" || s.Status == "To Airport")
             .Select(s => new SelectListItem
             {
                 Value = s.shipment_id.ToString(),
                 Text = s.shipment_id.ToString()
             }).ToList();

            ViewBag.ShipmentList = new SelectList(shipmentList, "Value", "Text");
            ViewBag.VehicleId = id;

            var model = new ShipmentVehicle { vehicle_id = id };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LoadShipment(ShipmentVehicle model)
        {
            if (ModelState.IsValid)
            {
                var shipment = db.Shipments.Find(model.shipment_id);
                var package = db.Packages.Find(shipment.package_id);
                var order = db.Orders.Find(package.order_id);


                switch (shipment.Status)
                {
                    case "Pending":
                        shipment.Status = "Local Warehouse";
                        order.Status = "Warehouse";
                        break;
                    case "Local Warehouse":
                        if (shipment.source_city_name == shipment.destination_city_name)
                        {
                            shipment.Status = "Out for Delivery";
                            order.Status = "Enroute";
                        }
                        else
                        {
                            shipment.Status = "To Airport";
                        }
                        break;
                    case "To Airport":
                        shipment.Status = "In Destination City Warehouse";
                        break;
                    case "In Destination City Warehouse":
                        shipment.Status = "Out for Delivery";
                        order.Status = "Enroute";
                        break;
                    default:
                        throw new InvalidOperationException("Unexpected shipment status.");
                }

                model.load_date = DateTime.Now;
                db.ShipmentVehicles.Add(model);
                db.SaveChanges();

                return RedirectToAction("DetailsVehicle", new { id = model.vehicle_id });
            }

            var shipmentList = db.Shipments
             .Where(s => s.Status == "Local Warehouse" || s.Status == "In Destination City Warehouse" || s.Status == "Pending" || s.Status == "To Airport")
             .Select(s => new SelectListItem
             {
                 Value = s.shipment_id.ToString(),
                 Text = s.shipment_id.ToString()
             }).ToList();

            ViewBag.ShipmentList = new SelectList(shipmentList, "Value", "Text");

            var modell = new ShipmentVehicle { vehicle_id = model.vehicle_id };
            return View(modell);
        }


        public ActionResult UnloadShipment(int id, int shipmentId)
        {
            var shipment = db.Shipments.Find(shipmentId);
            var package = db.Packages.Find(shipment.package_id);
            var order = db.Orders.Find(package.order_id);

            if (shipment.Status == "Out for Delivery")
            {
                shipment.Status = "Delivered";
                shipment.actual_delivery_date = DateTime.Now;
                order.Status = "Completed";

            }
            var shipmentVehicle = db.ShipmentVehicles.Find(shipmentId, id);
            db.ShipmentVehicles.Remove(shipmentVehicle);
            db.SaveChanges();

            return RedirectToAction("DetailsVehicle", new { id = id });
        }

        public ActionResult VehicleRoute(int id)
        {
            var vehicleRoutes = db.VehicleRoutes
                                 .Include(vr => vr.Vehicle)
                                 .Include(vr => vr.Route)
                                 .Where(vr => vr.vehicle_id == id)
                                 .ToList();

            return View(vehicleRoutes);
        }
        [HttpPost]
        public ActionResult RemoveRoute(int id)
        {
            var route = db.VehicleRoutes.Find(id);

            if (route != null )
            { 
                db.VehicleRoutes.Remove(route);
                db.SaveChanges();
                return RedirectToAction("ListVehicles");
            }
            return RedirectToAction("ListVehicles"); 
        }

    }
}