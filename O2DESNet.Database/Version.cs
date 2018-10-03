using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace O2DESNet.Database
{
    public class Version
    {
        public int Id { get; set; }
        public Project Project { get; set; }
        public DateTime CreateTime { get; set; }
        public ICollection<Scenario> Scenarios { get; set; } = new HashSet<Scenario>();
        public ICollection<InputPara> InputParas { get; set; } = new HashSet<InputPara>();
        public ICollection<OutputPara> OutputParas { get; set; } = new HashSet<OutputPara>();
        public string Number { get; set; }
        public string Comment { get; set; }
        public string URL { get; set; }
        public Scenario GetScenario(DbContext db, Dictionary<string, double> inputs)
        {
            var entry = db.Entry(this);
            if (entry.State != EntityState.Added && entry.State != EntityState.Detached)
            {
                entry.Collection(p => p.Scenarios).Query().Include(s => s.InputValues).Load();
                entry.Collection(p => p.InputParas).Query().Include(p => p.InputDesc).Load();
            }

            var scenario = Scenarios.Where(s => MapInputs(db, s, inputs)).FirstOrDefault();
            if (scenario == null)
            {
                scenario = new Scenario { Version = this, CreateTime = DateTime.Now };
                Scenarios.Add(scenario);
                foreach (var i in inputs)
                {
                    var para = InputParas.Where(p => p.InputDesc.Name == i.Key).FirstOrDefault();
                    if (para == null)
                    {
                        para = new InputPara { Version = this, InputDesc = Project.GetInputDesc(db, i.Key) };
                        InputParas.Add(para);
                    }                    
                    scenario.InputValues.Add(new InputValue { InputPara = para, Value = i.Value, Scenario = scenario });
                }
            }
            return scenario;
        }
        private bool MapInputs(DbContext db, Scenario scenario, Dictionary<string, double> inputs)
        {
            var entry = db.Entry(scenario);
            if (entry.State != EntityState.Added && entry.State != EntityState.Detached)
                entry.Collection(s => s.InputValues).Query().Include(i => i.InputPara.InputDesc).Load();

            if (scenario.InputValues.Count != inputs.Count) return false;
            foreach (var i in scenario.InputValues)
            {
                var key = i.InputPara.InputDesc.Name;
                if (!inputs.ContainsKey(key) || inputs[key] != i.Value) return false;
            }
            return true;
        }
    }
}