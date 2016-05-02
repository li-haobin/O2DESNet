using System;
using System.IO;

namespace O2DESNet
{
    public abstract class Status<TScenario> where TScenario : Scenario
    {
        internal protected TScenario Scenario { get; private set; }
        internal protected Random DefaultRS { get; private set; }
        private int _seed;
        public int Seed { get { return _seed; } set { _seed = value; DefaultRS = new Random(_seed); } }
        
        public Status(TScenario scenario, int seed = 0)
        {
            Scenario = scenario;
            Seed = seed;
            Display = false;
        }

        public virtual void WarmedUp(DateTime clockTime) { throw new NotImplementedException(); }

        #region For Logging
        private string _logFile;
        public bool Display { get; set; }
        public string LogFile
        {
            get { return _logFile; }
            set
            {
                _logFile = value; if (_logFile != null) using (var sw = new StreamWriter(_logFile)) { };
            }
        }
        public void Log(string format, params object[] args)
        {
            if (Display) Console.WriteLine(format, args);
            if (LogFile != null) using (var sw = new StreamWriter(LogFile, true)) sw.WriteLine(format, args);
        }
        #endregion
    }
}
