//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LogiticsManagment.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class warehouse_inventory
    {
        public int package_id { get; set; }
        public string category { get; set; }
        public int shelf_no { get; set; }
        public int row_no { get; set; }
        public int column_no { get; set; }
        public Nullable<int> warehouse_id { get; set; }
    
        public virtual Package Package { get; set; }
        public virtual Warehouse Warehouse { get; set; }
    }
}