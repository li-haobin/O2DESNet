using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace O2DESNet.Database
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public DateTime CreateTime { get; set; }
        public ICollection<InputDesc> InputDescs { get; set; } = new HashSet<InputDesc>();
        public ICollection<OutputDesc> OutputDescs { get; set; } = new HashSet<OutputDesc>();
        public ICollection<Version> Versions { get; set; } = new HashSet<Version>();
        public InputDesc GetInputDesc(DbContext db, string name)
        {
            var entry = db.Entry(this);
            if (entry.State != EntityState.Added && entry.State != EntityState.Detached)
                entry.Collection(p => p.InputDescs).Query().Load();

            var desc = InputDescs.Where(i => i.Name == name).FirstOrDefault();
            if (desc == null)
            {
                desc = new InputDesc { Project = this, Name = name, CreateTime = DateTime.Now };
                InputDescs.Add(desc);
            }
            return desc;
        }
        public OutputDesc GetOutputDesc(DbContext db, string name)
        {
            var entry = db.Entry(this);
            if (entry.State != EntityState.Added && entry.State != EntityState.Detached)
                entry.Collection(p => p.OutputDescs).Query().Load();

            var desc = OutputDescs.Where(i => i.Name == name).FirstOrDefault();
            if (desc == null)
            {
                desc = new OutputDesc { Name = name, CreateTime = DateTime.Now };
                OutputDescs.Add(desc);
            }
            return desc;
        }
        public Version GetVersion(DbContext db, string number)
        {
            var entry = db.Entry(this);
            if (entry.State != EntityState.Added && entry.State != EntityState.Detached)
                entry.Collection(p => p.Versions).Load();

            var version = Versions.Where(i => i.Number == number).FirstOrDefault();
            if (version == null)
            {
                version = new Version { Project = this, Number = number, Timestamp = DateTime.Now };
                Versions.Add(version);
            }
            return version;

        }
    }
}