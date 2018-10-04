using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;

namespace O2DESNet.Database
{
    public class Version
    {
        public int Id { get; set; }
        public Project Project { get; set; }
        public ICollection<Scenario> Scenarios { get; set; } = new HashSet<Scenario>();
        public ICollection<InputPara> InputParas { get; set; } = new HashSet<InputPara>();
        public ICollection<OutputPara> OutputParas { get; set; } = new HashSet<OutputPara>();
        public string Number { get; set; }
        public string Comment { get; set; }
        public string URL { get; set; }
        public DateTime Timestamp { get; set; }
        public string Operator { get; set; }
        #region Experiment Setting
        /// <summary>
        /// In unit of days
        /// </summary>
        public double RunInterval { get; set; }
        /// <summary>
        /// In unit of days
        /// </summary>
        public double WarmUpPeriod { get; set; }
        /// <summary>
        /// In unit of days
        /// </summary>
        public double RunLength { get; set; }
        #endregion

        internal Scenario GetScenario(DbContext db, Dictionary<string, double> inputValues, string by)
        {
            if (db.IsLoadable(this))
            {
                db.Entry(this).Collection(p => p.Scenarios).Query().Include(s => s.InputValues).Load();
                db.Entry(this).Collection(p => p.InputParas).Query().Include(p => p.InputDesc).Load();
            }

            var scenario = Scenarios.Where(s => MapInputs(db, s, inputValues)).FirstOrDefault();
            if (scenario == null)
            {
                scenario = new Scenario { Version = this, Timestamp = DateTime.Now, Operator = by };
                Scenarios.Add(scenario);
                foreach (var i in inputValues)
                {
                    var para = InputParas.Where(p => p.InputDesc.Name == i.Key).FirstOrDefault();
                    //if (para == null)
                    //{
                    //    para = new InputPara { Version = this, InputDesc = Project.GetInputDesc(db, i.Key) };
                    //    InputParas.Add(para);
                    //}                    
                    scenario.InputValues.Add(new InputValue { InputPara = para, Value = i.Value, Scenario = scenario });
                }
            }
            return scenario;
        }
        private bool MapInputs(DbContext db, Scenario scenario, Dictionary<string, double> inputs)
        {
            if (db.IsLoadable(scenario))
                db.Entry(scenario).Collection(s => s.InputValues).Query().Include(i => i.InputPara.InputDesc).Load();

            if (scenario.InputValues.Count != inputs.Count) return false;
            foreach (var i in scenario.InputValues)
            {
                var key = i.InputPara.InputDesc.Name;
                if (!inputs.ContainsKey(key) || inputs[key] != i.Value) return false;
            }
            return true;
        }
        internal OutputPara GetOutputPara(DbContext db, string name)
        {
            db.Entry(this).Collection(v => v.OutputParas).Query().Load();
            db.Entry(this).Reference(v => v.Project).Query().Load();
            var para = OutputParas.Where(p => p.OutputDesc.Name == name).FirstOrDefault();
            //if (para == null)
            //{
            //    //using (var transaction = db.Database.BeginTransaction())
            //    //{
            //    //    try
            //    //    {
            //    //        para = new OutputPara { Version = this, OutputDesc = Project.GetOutputDesc(db, name) };
            //    //        OutputParas.Add(para);
            //    //    }
            //    //    catch (Exception)
            //    //    {
            //    //        transaction.Rollback();
            //    //        Thread.Sleep(50);
            //    //        return GetOutputPara(db, name);
            //    //    }
            //    //}
            //    para = new OutputPara { Version = this, OutputDesc = Project.GetOutputDesc(db, name) };
            //    OutputParas.Add(para);
            //}            
            return para;
        }
    }
}