using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace O2DESNet.Database
{
    class Program
    {
        static void Main(string[] args)
        {
            //db.SaveChanges();

            while (true)
            {
                var db = new DbContext();

                var s = db.GetScenario("TuasFinger3", "1.0.0.3", new Dictionary<string, double> { { "b", 1 }, { "c", 4 } });

                //s.AddSnapshot(db, 2, new DateTime(2, 1, 1, 0, 1, 0), new Dictionary<string, double> { { "f", 0.01 }, { "g", 400 } }, Environment.MachineName);
                var res = s.RemoveReplication(db, 2);


                db.SaveChanges();         
            }
                       
        }
    }
}
