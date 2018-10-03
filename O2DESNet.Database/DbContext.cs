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
            modelBuilder.Entity<Version>().HasRequired(v => v.Project).WithMany()
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<Scenario>().HasRequired(s => s.Version).WithMany()
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<InputDesc>().HasRequired(d => d.Project).WithMany()
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<InputPara>().HasRequired(p => p.Version).WithMany()
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<InputValue>().HasRequired(v => v.Scenario).WithMany()
                .WillCascadeOnDelete(true);
        }
    }
}
