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
        public DbSet<Version> Versions { get; set; }
        public DbSet<Scenario> Scenarios { get; set; }
        //public DbSet<Replication> Replications { get; set; }

        public Project GetProject(string name)
        {
            var project = Projects.Where(i => i.Name == name).FirstOrDefault();
            if (project == null)
            {
                project = new Project { Name = name, CreateTime = DateTime.Now };
                Projects.Add(project);
            }
            return project;
        }
        public Version GetVersion(string projectName, string versionNumber)
        {
            var project = GetProject(projectName);
            var version = project.GetVersion(this, versionNumber);
            return version;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Version>().HasRequired(v => v.Project).WithMany(p => p.Versions).WillCascadeOnDelete(true);
            modelBuilder.Entity<Scenario>().HasRequired(s => s.Version).WithMany(v => v.Scenarios).WillCascadeOnDelete(true);
            modelBuilder.Entity<InputDesc>().HasRequired(d => d.Project).WithMany(p => p.InputDescs).WillCascadeOnDelete(true);
            modelBuilder.Entity<InputPara>().HasRequired(p => p.Version).WithMany(v => v.InputParas).WillCascadeOnDelete(true);
            modelBuilder.Entity<InputValue>().HasRequired(v => v.Scenario).WithMany(s => s.InputValues).WillCascadeOnDelete(true);
            modelBuilder.Entity<Replication>().HasRequired(r => r.Scenario).WithMany(s => s.Replications).WillCascadeOnDelete(true);
            modelBuilder.Entity<Snapshot>().HasRequired(s => s.Replication).WithMany(r => r.Snapshots).WillCascadeOnDelete(true);
        }

        internal bool Loadable(object obj)
        {
            return Entry(obj).State != EntityState.Added && Entry(obj).State != EntityState.Detached;
        }
    }
}
