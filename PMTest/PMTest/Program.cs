using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test;
using O2DESNet;

namespace PMTest
{
    class Program
    {
        static void Main()
        {
            Vehicle vehicle = new Vehicle(new Vehicle.Statics { Id = 1 }, seed: 0);
            Console.WriteLine("vehicle index: {0}", vehicle.Category.Id);
            Console.WriteLine();

            //Console.WriteLine("speed of current vehicle {0}", vehicle.Speed);
            //vehicle.SetSpeed(1);
            //Console.WriteLine("speed of current vehicle {0}", vehicle.Speed);
            //vehicle.SetSpeed(2);
            //Console.WriteLine("speed of current vehicle {0}", vehicle.Speed);
            //vehicle.SetSpeed(3);
            //Console.WriteLine("speed of current vehicle {0}", vehicle.Speed);
            //vehicle.SetSpeed(4);
            //Console.WriteLine("speed of current vehicle {0}", vehicle.Speed);

            //Console.WriteLine();
            //Console.WriteLine("acceleration is : {0}", vehicle.Acceleration);
            //vehicle.SetAcceleration(3);
            //Console.WriteLine("acceleration is : {0}", vehicle.Acceleration);
            //vehicle.SetAcceleration(4);
            //Console.WriteLine("acceleration is : {0}", vehicle.Acceleration);
            //vehicle.SetAcceleration(5);
            //Console.WriteLine("acceleration is : {0}", vehicle.Acceleration);
            //vehicle.SetAcceleration(6);
            //Console.WriteLine("acceleration is : {0}", vehicle.Acceleration);

            Console.WriteLine();
            Console.WriteLine("Mileage situation:");
            foreach (KeyValuePair<int, double> kvp in vehicle.Mileage)
            {
                Console.WriteLine(" No. {0} distance:{1}", kvp.Key, kvp.Value);
            }
            vehicle.UpdateMileage(1,10);
            Console.WriteLine();
            Console.WriteLine("Mileage situation:");
            foreach (KeyValuePair<int, double> kvp in vehicle.Mileage)
            {
                Console.WriteLine(" No. {0} distance:{1}", kvp.Key, kvp.Value);
            }
            vehicle.UpdateMileage(2,5);
            Console.WriteLine();
            Console.WriteLine("Mileage situation:");
            foreach (KeyValuePair<int, double> kvp in vehicle.Mileage)
            {
                Console.WriteLine(" No. {0} distance:{1}", kvp.Key, kvp.Value);
            }
            vehicle.UpdateMileage(3,18);
            Console.WriteLine();
            Console.WriteLine("Mileage situation:");
            foreach (KeyValuePair<int, double> kvp in vehicle.Mileage)
            {
                Console.WriteLine(" No. {0} distance:{1}", kvp.Key, kvp.Value);
            }
            vehicle.UpdateMileage(1,20);
            Console.WriteLine();
            Console.WriteLine("Mileage situation:");
            foreach (KeyValuePair<int, double> kvp in vehicle.Mileage)
            {
                Console.WriteLine(" No. {0} distance:{1}", kvp.Key, kvp.Value);
            }

            vehicle.SetMileage(1,0);
            Console.WriteLine();
            Console.WriteLine("after set the mileage [1] to 0, the mileage is:");
            foreach (KeyValuePair<int, double> kvp in vehicle.Mileage)
            {
                Console.WriteLine("Mileage situation: No. {0} distance:{1}", kvp.Key, kvp.Value);
            }

            //Console.WriteLine();
            //Console.WriteLine("Current timestamp is {0}", vehicle.TimeStamp);
            //vehicle.SetTimeStamp(new DateTime(2017, 2, 1, 0, 0, 0));
            //Console.WriteLine();
            //Console.WriteLine("Current timestamp is {0}", vehicle.TimeStamp);
            //vehicle.SetTimeStamp(new DateTime(2017, 3, 1, 0, 0, 0));
            //Console.WriteLine();
            //Console.WriteLine("Current timestamp is {0}", vehicle.TimeStamp);
            //vehicle.SetTimeStamp(new DateTime(2017, 4, 30, 13, 59, 30));
            //Console.WriteLine();
            //Console.WriteLine("Current timestamp is {0}", vehicle.TimeStamp);
            //vehicle.SetTimeStamp(vehicle.TimeStamp + new TimeSpan(12, 1, 0));
            //Console.WriteLine();
            //Console.WriteLine("After add a timespan, Current time stamp is {0}", vehicle.TimeStamp);

            //ControlPoint controlpoint1 = new ControlPoint(new ControlPoint.Statics { Id = 1 }, seed: 0);
            //ControlPoint controlpoint2 = new ControlPoint(new ControlPoint.Statics { Id = 2 }, seed: 0);
            //ControlPoint controlpoint3 = new ControlPoint(new ControlPoint.Statics { Id = 3 }, seed: 0);
            //ControlPoint controlpoint4 = new ControlPoint(new ControlPoint.Statics { Id = 4 }, seed: 0);
            //ControlPoint controlpoint5 = new ControlPoint(new ControlPoint.Statics { Id = 5 }, seed: 0);
            //Console.WriteLine("Current targets are:");
            //foreach (ControlPoint controlpoint in vehicle.Targets)
            //    Console.WriteLine(controlpoint.Config.Id);
            //vehicle.AddTargets(controlpoint1);
            //vehicle.AddTargets(controlpoint2);
            //Console.WriteLine();
            //Console.WriteLine("after add some target in the list, the targets become:");
            //foreach (ControlPoint controlpoint in vehicle.Targets)
            //    Console.WriteLine(controlpoint.Config.Id);
            //List<ControlPoint> NewTargets = new List<ControlPoint>();
            //NewTargets.Add(controlpoint3);
            //NewTargets.Add(controlpoint4);
            //NewTargets.Add(controlpoint5);
            //vehicle.SetTargets(NewTargets);
            //Console.WriteLine();
            //Console.WriteLine("after change another list of controlpoint, set it to be the new targets, the targets become:");
            //foreach (ControlPoint controlpoint in vehicle.Targets)
            //    Console.WriteLine(controlpoint.Config.Id);

            //vehicle.SetPosition(controlpoint3);
            //Console.WriteLine();
            //Console.WriteLine("set the controlpoint 3 to be the current position, the targets list become:");
            //foreach (ControlPoint controlpoint in vehicle.Targets)
            //    Console.WriteLine(controlpoint.Config.Id);
            //Console.WriteLine("current position become:");
            //Console.WriteLine(vehicle.Position.Config.Id);
            //vehicle.SetPosition(controlpoint2);
            //Console.WriteLine();
            //Console.WriteLine("when set a controlpoint that does not exsist in the targets list, nothing will be removed from the list:");
            //foreach (ControlPoint controlpoint in vehicle.Targets)
            //    Console.WriteLine(controlpoint.Config.Id);
            //Console.WriteLine("current position become:");
            //Console.WriteLine(vehicle.Position.Config.Id);

            //Path path1 = new Path(new Path.Statics { Id = 1 }, seed: 0);
            //Path path2 = new Path(new Path.Statics { Id = 2 }, seed: 0);
            //vehicle.SetTravellingPath(path1);
            //Console.WriteLine();
            //Console.WriteLine("current the vehicle is travelling on the path: {0}", vehicle.TravellingOn.Config.Id);
            //vehicle.SetTravellingPath(path2);
            //Console.WriteLine();
            //Console.WriteLine("current the vehicle is travelling on the path: {0}", vehicle.TravellingOn.Config.Id);
            //vehicle.SetTravellingPath(path1);
            //Console.WriteLine();
            //Console.WriteLine("current the vehicle is travelling on the path: {0}", vehicle.TravellingOn.Config.Id);


            Console.ReadKey();
            

        }
    }
}
