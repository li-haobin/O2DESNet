using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace O2DESNet.Animation
{
    public interface IAnimator
    {
        void Add(Canvas canvas, string objectID, double x, double y, double degree, DateTime simlationTimeStamp);

        void Move(string objectID, double x, double y, double degree, DateTime simlationTimeStamp);

        void Remove(string objectID, DateTime simlationTimeStamp);

        void Update(Canvas canvas, string objectID, DateTime simlationTimeStamp);
    }
}
