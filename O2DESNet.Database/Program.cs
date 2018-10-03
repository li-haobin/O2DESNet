using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                var ver = db.GetVersion("TuasFinger3", "1.0.0.3");
                var inputs = new Dictionary<string, double> { { "b", 1 }, { "c", 4 } };
                var s = ver.GetScenario(db, inputs);
                db.SaveChanges();         
            }
                       
        }
    }
}
