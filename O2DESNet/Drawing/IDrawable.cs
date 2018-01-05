using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace O2DESNet
{
    public interface IDrawable
    {
        TransformGroup TransformGroup { get; }
        Canvas Drawing { get; }
        bool ShowTag { get; set; }
        void UpdDrawing(DateTime? clockTime = null);
    }
}
