﻿using O2DESNet.Warehouse.Statics;
using System;
using System.Linq;

namespace O2DESNet.Warehouse
{
    class Program
    {
        static void Main(string[] args)
        {
            WarehouseSim whsim = new WarehouseSim();
            whsim.wh.ViewAll();

            Console.ReadKey();
        }      
    }
}
