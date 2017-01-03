using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public abstract class Component : State<Scenario>        
    {
        protected static int _count = 0;
        public int Id { get; protected set; }
        public string Name { get; protected set; }
        public string Tag { get; set; }
        protected Component(int seed = 0, string tag = null) : base(new Scenario(), seed)
        {
            Id = ++_count; Tag = tag;            
            InitEvents = new List<Event>();
        }        
        public override string ToString()
        {
            if (Tag != null && Tag.Length > 0) return Tag;
            return string.Format("{0}#{1}", Name, Id);
        }
        public List<Event> InitEvents { get; private set; }
    }

    public abstract class Component<TStatics> : Component
        where TStatics : Scenario
    {
        public TStatics Config { get; private set; }
        public Component(TStatics config, int seed = 0, string tag = null) : base(seed, tag)
        {
            Config = config;
        }
    }
}
