namespace O2DESNet.Standard
{
    public class Load : ILoad
    {
        private static int _count = 0;
        public int Index { get; private set; } = _count++;
        public virtual string Id { get { return string.Format("{0}#{1}", GetType().Name, Index); } }
        public override string ToString() { return Id; }
    }
}
