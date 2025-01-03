using LogiticsManagment.Models;
using LogiticsManagment.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LogisticsManagement.Controllers
{
    public class WarehouseController : Controller
    {
        private LogisticManagmentEntities1 db = new LogisticManagmentEntities1();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult WarehouseManagement()
        {
            return View();
        }

        public ActionResult ListWarehouses()
        {
            var warehouses = db.Warehouses.ToList();
            var warehouseViewModels = warehouses.Select(w => new WarehouseViewModel
            {
                Warehouse = w,
                Inventories = db.warehouse_inventory
                               .Where(i => i.warehouse_id == w.warehouse_id)
                               .ToList()
            }).ToList();

            return View(warehouseViewModels);
        }

        public ActionResult Details(int id)
        {
            var warehouse = db.Warehouses.Find(id);
            if (warehouse == null)
            {
                return HttpNotFound();
            }

            var inventories = db.warehouse_inventory
                        .Where(i => i.warehouse_id == id)
                        .Include(i => i.Package) 
                        .Include(i => i.Package.Order) 
                        .Where(i => i.Package.Order.Status != "Completed")
                        .ToList();

            var warehouseViewModel = new WarehouseViewModel
            {
                Warehouse = warehouse,
                Inventories = inventories
            };



            return View(warehouseViewModel);
        }


        public ActionResult EditInventory(int id)
        {
            var inventory = db.warehouse_inventory.Find(id);
            if (inventory == null)
            {
                return HttpNotFound();
            }

            var warehouseViewModel = new WarehouseViewModel
            {
                Warehouse = db.Warehouses.Find(inventory.warehouse_id),
                Inventories = new List<warehouse_inventory> { inventory }
            };

            return View(warehouseViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditInventory(WarehouseViewModel warehouseViewModel)
        {
            if (ModelState.IsValid)
            {
                var inventory = warehouseViewModel.Inventories.FirstOrDefault();
                if (inventory == null)
                {
                    return HttpNotFound();
                }

                db.Entry(inventory).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Details", new { id = inventory.warehouse_id });
            }
            return View(warehouseViewModel);
        }

        public ActionResult AddWarehouse()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddWarehouse(Warehouse warehouse)
        {
            if (ModelState.IsValid)
            {
                db.Warehouses.Add(warehouse);
                db.SaveChanges();
                return RedirectToAction("ListWarehouses");
            }
            return View(warehouse);
        }

        public ActionResult EditWarehouse(int id)
        {
            var warehouse = db.Warehouses.Find(id);
            WarehouseViewModel vm = new WarehouseViewModel();
            vm.Warehouse = warehouse;
            if (warehouse == null)
            {
                return HttpNotFound();
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditWarehouse(Warehouse warehouse)
        {
            if (ModelState.IsValid)
            {
                db.Entry(warehouse).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ListWarehouses");
            }
            return View(warehouse);
        }

        public ActionResult DeleteWarehouse(int id)
        {
            var warehouse = db.Warehouses.Find(id);
            if (warehouse == null)
            {
                return HttpNotFound();
            }
            return View(warehouse);
        }

        [HttpPost, ActionName("DeleteWarehouse")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            db.DeleteWarehouse(id);
            db.SaveChanges();
            return RedirectToAction("ListWarehouses");
        }

        public ActionResult DeleteInventory(int id)
        {
            var warehouse = db.warehouse_inventory.Find(id);
            if (warehouse == null)
            {
                return HttpNotFound();
            }
            return View(warehouse);
        }

        [HttpPost, ActionName("DeleteInventory")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteInventoryConfirmed(int id)
        {
            var inventory = db.warehouse_inventory.Find(id);
            if (inventory == null)
            {
                return HttpNotFound();
            }

            db.warehouse_inventory.Remove(inventory);
            db.SaveChanges();

            return RedirectToAction("ListWarehouses");
        }


        public ActionResult AddPackage(int warehouseId)
        {
            WarehouseViewModel vm = new WarehouseViewModel();
            vm.WarehouseInventory.warehouse_id = warehouseId;
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPackage(WarehouseViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var inventory = new warehouse_inventory
                {
                    package_id = viewModel.WarehouseInventory.package_id,
                    warehouse_id = viewModel.WarehouseInventory.warehouse_id,
                    category = viewModel.WarehouseInventory.category,
                    shelf_no = viewModel.WarehouseInventory.shelf_no,
                    row_no = viewModel.WarehouseInventory.row_no,
                    column_no = viewModel.WarehouseInventory.column_no
                };

                db.warehouse_inventory.Add(inventory);
                db.SaveChanges();

                return RedirectToAction("Details", new { id = viewModel.WarehouseInventory.warehouse_id });
            }

            return View(viewModel);
        }


        public ActionResult InventoryEdit(int id)
        {
            WarehouseViewModel viewModel;
            using (var db = new LogisticManagmentEntities1())
            {
                var inventoryItem = db.warehouse_inventory.FirstOrDefault(i => i.package_id == id);

                 viewModel = new WarehouseViewModel
                {
                    WarehouseInventory = inventoryItem
                };
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InventoryEdit(WarehouseViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                warehouse_inventory inventoryItem;
                using (var db = new LogisticManagmentEntities1())
                {
                     inventoryItem = db.warehouse_inventory.FirstOrDefault(i => i.package_id == viewModel.WarehouseInventory.package_id);
                }
                if (inventoryItem == null)
                {
                    return HttpNotFound();
                }
                inventoryItem.category = viewModel.WarehouseInventory.category;
                inventoryItem.shelf_no = viewModel.WarehouseInventory.shelf_no;
                inventoryItem.row_no = viewModel.WarehouseInventory.row_no;
                inventoryItem.column_no = viewModel.WarehouseInventory.column_no;


                db.SaveChanges();

                return RedirectToAction("Details", new { id = inventoryItem.warehouse_id });
            }

            return View(viewModel);
        }

    }
}