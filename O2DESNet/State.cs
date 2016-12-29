using System;
using System.IO;

namespace O2DESNet
{
    public abstract class State<TScenario> where TScenario : Scenario
    {
        internal protected TScenario Scenario { get; private set; }
        internal protected Random DefaultRS { get; private set; }
        private int _seed;
        public int Seed { get { return _seed; } set { _seed = value; DefaultRS = new Random(_seed); } }
        
        public State(TScenario scenario, int seed = 0)
        {
            Scenario = scenario;
            Seed = seed;
            Display = false;
        }

        public virtual void WarmedUp(DateTime clockTime) { throw new NotImplementedException(); }
        public virtual void WriteToConsole(DateTime? clockTime = null) { throw new NotImplementedException(); }

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
        public void Log(params object[] args)
        {
            if (Display)
            {
                foreach (var arg in args) Console.Write("{0}\t", arg);
                Console.WriteLine();
            }
                
            if (LogFile != null)
                using (var sw = new StreamWriter(LogFile, true))
                {
                    foreach (var arg in args) sw.Write("{0},", arg);
                    sw.WriteLine();
                }
        }
        #endregion
    }    
}
