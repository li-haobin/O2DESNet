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

            var prj = new Project { Name = "TestProject" };
            //var prj = db.Projects.Where(p => p.Id == 2).First();

            prj.InputDescs.Add(new InputDesc());
            prj.OutputDescs.Add(new OutputDesc());

            //prj.InputDescs = new Add(new InputDesc());
            //prj.OutputDescs.Add(new OutputDesc());

            db.Projects.Add(prj);

            db.SaveChanges();
        }
    }
}
