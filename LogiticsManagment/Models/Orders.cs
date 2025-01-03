using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LogiticsManagment.Interfaces;

namespace LogiticsManagment.Models
{
    public class Orders
    {
        private List<IOrderObserver> _observers = new List<IOrderObserver>();

        public int OrderId { get; set; }

        public void Attach(IOrderObserver observer)
        {
            _observers.Add(observer);
        }

        public void Detach(IOrderObserver observer)
        {
            _observers.Remove(observer);
        }

        public void NotifyObservers()
        {
            foreach (var observer in _observers)
            {
                observer.Update(this);
            }
        }
    }
}