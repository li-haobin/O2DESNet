namespace O2DESNet.Standard
{
    public class Load : ILoad
    {
        private static int _count = 0;
        public int Index { get; } = _count++;
        public virtual string Id => $"{GetType().Name}#{Index}";
        public override string ToString() { return Id; }
    }
}
