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
        public DbSet<Replication> Replications { get; set; }

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
            return GetProject(projectName).GetVersion(this, versionNumber);
        }
        public Scenario GetScenario(string projectName, string versionNumber, Dictionary<string, double> inputs)
        {
            return GetVersion(projectName, versionNumber).GetScenario(this, inputs);
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
            modelBuilder.Entity<OutputDesc>().HasRequired(d => d.Project).WithMany(p => p.OutputDescs).WillCascadeOnDelete(true);
            modelBuilder.Entity<OutputPara>().HasRequired(p => p.Version).WithMany(v => v.OutputParas).WillCascadeOnDelete(true);
            modelBuilder.Entity<OutputValue>().HasRequired(v => v.Snapshot).WithMany(s => s.OutputValues).WillCascadeOnDelete(true);
        }

        internal bool Loadable(object obj)
        {
            return Entry(obj).State != EntityState.Added && Entry(obj).State != EntityState.Detached;
        }
    }
}
