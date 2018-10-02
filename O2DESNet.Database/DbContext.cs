using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Database
{
    public class DbContext : System.Data.Entity.DbContext
    {
        public DbContext() : base() { }
        public DbSet<Project> Projects { get; set; }
    }
}
