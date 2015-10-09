namespace O2DESNet.Demos.Workshop
{
    public class Machine
    {
        public int Id { get; internal set; }
        public MachineType Type { get; internal set; }
        public Job Processing { get; set; }
        public bool IsIdle { get { return Processing == null; } }
    }
}
