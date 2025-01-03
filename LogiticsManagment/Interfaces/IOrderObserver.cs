using LogiticsManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogiticsManagment.Interfaces
{
    public interface IOrderObserver
    {
        void Update(Orders order);
    }
}
