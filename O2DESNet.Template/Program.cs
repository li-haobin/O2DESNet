namespace O2DESNet.Template
{
    class Program
    {
        static void Main(string[] args)
        {
            var scenario = new Scenario(); // preparing the scenario
            var sim = new Simulator(new Status(scenario, seed: 0)); // construct the simulator
            sim.Run(10000); // run simulator
            // sim.Status... // read analytics from status class
        }
    }
}
