using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace O2DESNet
{
    public interface IDrawable
    {
        Canvas Drawing { get; }
        bool ShowTag { get; set; }
        void UpdDrawing(DateTime? clockTime = null);
    }
    
    public abstract class Drawer
    {
        private bool _showTag = false;
        protected DateTime? _timestamp = null;
        protected Canvas _drawing = null;
        public Canvas Drawing { get { return _drawing; } }
        public bool ShowTag { get { return _showTag; } set { _showTag = value; UpdDrawing(_timestamp); } }
        public abstract void UpdDrawing(DateTime? clockTime);
    }
}
