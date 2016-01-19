using O2DESNet.Demos.Workshop.Dynamics;
using System;

namespace O2DESNet.Demos.Workshop.Events
{
    internal class Depart : Event<Scenario, Status>
    {
        internal Product Product { get; set; }
        protected override void Invoke()
        {
            // log departure
            Product.ExitTime = ClockTime;
            Status.ProductsDeparted.Add(Product);
            Status.ProductsInSystem.Remove(Product);
            Status.TimeSeries_ProductHoursInSystem.Add((Product.ExitTime - Product.EnterTime).TotalHours);
            Log("{0}: Product #{1} (Type {2}) departs.", ClockTime.ToString("yyyy/MM/dd HH:mm:ss"), Product.Id, Product.Type.Id);
        }
    }
}
