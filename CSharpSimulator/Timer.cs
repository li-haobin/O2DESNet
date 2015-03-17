using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSimulator
{
    public class Timer
    {
        private DateTime _lastCheck;
        public Timer() { _lastCheck = DateTime.Now; }
        public TimeSpan Check()
        {
            var timeSpan = DateTime.Now - _lastCheck;
            _lastCheck = DateTime.Now;
            return timeSpan;
        }
    }
}
