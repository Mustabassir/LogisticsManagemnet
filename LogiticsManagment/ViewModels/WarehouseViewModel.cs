using LogiticsManagment.Models;
using System.Collections.Generic;

namespace LogiticsManagment.ViewModels
{
    public class WarehouseViewModel
    {
        public WarehouseViewModel()
        {
            // Initialize properties here
            Warehouse = new Warehouse();
            Inventories = new List<warehouse_inventory>();
            WarehouseInventory = new warehouse_inventory();
        }

        public Warehouse Warehouse { get; set; }
        public List<warehouse_inventory> Inventories { get; set; }
        public warehouse_inventory WarehouseInventory { get; set; }
    }
}
