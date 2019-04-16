using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace O2DESNet.Animation
{
    internal class DummyAnimator : IAnimator
    {
        public void Add(Canvas canvas, string objectID, double x, double y, double degree, DateTime simlationTimeStamp)
        {
            
        }

        public void Move(string objectID, double x, double y, double degree, TimeSpan duration, DateTime simlationTimeStamp)
        {
            
        }

        public void Remove(string objectID, DateTime simlationTimeStamp)
        {
            
        }

        public void Update(Canvas canvas, string objectID, DateTime simlationTimeStamp)
        {

        }
    }
}
