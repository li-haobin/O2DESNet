using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public abstract class Component
    {
        protected static int _count = 0;
        public int Id { get; protected set; }
        public string Name { get; protected set; }
        public string Tag { get; set; }
        public Random DefaultRS { get; private set; }
        protected Component(int seed = -1, string tag = null) { Id = ++_count; Tag = tag; if (seed > -1) DefaultRS = new Random(seed); }
        public override string ToString()
        {
            if (Tag != null && Tag.Length > 0) return Tag;
            return string.Format("{0}#{1}", Name, Id);
        }

        public abstract void WarmedUp(DateTime clockTime);
        public virtual void WriteToConsole() { }
    }
}
