using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Drawing
{
    public class Background
    {
        public string ImageFile { get; set; }
        /// <summary>
        /// The scale of background image against the foreground
        /// </summary>
        public double Scale { get; set; }
        /// <summary>
        /// Translate X of background image against the foreground
        /// </summary>
        public double Translate_X { get; set; }
        /// <summary>
        /// Translate Y of background image against the foreground
        /// </summary>
        public double Translate_Y { get; set; }
    }
}
