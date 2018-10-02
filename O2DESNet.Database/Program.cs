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
            var db = new DbContext();

            var ver = db.GetVersion("TuasFinger3", "1.0.0.3");
            //db.SaveChanges();

            var inputs = new Dictionary<string, string> { { "c", "1" }, { "b", "3" } };

            var s = ver.GetScenario(db, inputs);

            //var prj = db.Projects.Where(p => p.Id == 2).First();

            //prj.InputDescs.Add(new InputDesc());
            //prj.OutputDescs.Add(new OutputDesc());

            //prj.InputDescs = new Add(new InputDesc());
            //prj.OutputDescs.Add(new OutputDesc());


            db.SaveChanges();
        }
    }
}
