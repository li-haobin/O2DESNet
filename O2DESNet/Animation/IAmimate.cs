using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Animation
{
    public interface IAnimate
    {
        IAnimator Animator { get; set; }
    }
}
