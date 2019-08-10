using NUnit.Framework;
using O2DESNet.Distributions;
using O2DESNet.Standard;
using O2DESNet.Demos;
using System;
using System.Diagnostics;

namespace O2DESNet.UnitTests
{
    public class Version_3
    {
        [Test]
        public void WarmedUp()
        {
            var a = new A();
            a.WarmUp(TimeSpan.FromHours(1));
        }

        [Test]
        public void Generator()
        {
            var gen = new Generator.Statics { InterArrivalTime = 
                rs => Exponential.Sample(rs, TimeSpan.FromMinutes(3)) }.Sandbox();
            gen.Run(TimeSpan.FromHours(0.5));
            gen.Start();
            gen.Run(TimeSpan.FromHours(2));
            gen.End();
            gen.Run(TimeSpan.FromHours(0.5));
            gen.Start();
            gen.Run(TimeSpan.FromHours(1));
        }

        [Test]
        public void MMnQueue_Atomic()
        {
            for (int seed = 0; seed < 3; seed++)
            {
                var q = new MMnQueue_Atomic(4, 5, 1, seed);
                var sw = new Stopwatch();
                sw.Start();
                q.WarmUp(TimeSpan.FromHours(1000));
                q.Run(TimeSpan.FromHours(20000));
                sw.Stop();
                Debug.WriteLine("{0:F4}\t{1:F4}\t{2:F4}\t{3}ms", 
                    q.AvgNQueueing, q.AvgNServing, q.AvgHoursInSystem, sw.ElapsedMilliseconds);
            }
        }

        [Test]
        public void MMnQueue_Modular()
        {
            for (int seed = 0; seed < 3; seed++)
            {
                var q = new MMnQueue_Modular(4, 5, 1, seed);
                var sw = new Stopwatch();
                sw.Start();
                q.WarmUp(TimeSpan.FromHours(1000));
                q.Run(TimeSpan.FromHours(20000));
                sw.Stop();
                Debug.WriteLine("{0:F4}\t{1:F4}\t{2:F4}\t{3}ms", 
                    q.AvgNQueueing, q.AvgNServing, q.AvgHoursInSystem, sw.ElapsedMilliseconds);
            }
        }

        private class Assets : IAssets
        {
            public string Id { get { return GetType().Name; } }
        }
        private class A : Sandbox<Assets>
        {
            public A() : base(new Assets()) { AddChild(new B()); AddChild(new C()); }
            public override void Dispose() { }
            protected override void WarmedUpHandler()
            {
                Debug.WriteLine("A WarmedUp");
            }
        }
        private class B : Sandbox<Assets>
        {
            public B() : base(new Assets()) { AddChild(new D()); }
            public override void Dispose() { }
            protected override void WarmedUpHandler()
            {
                Debug.WriteLine("B WarmedUp");
            }
        }
        private class C : Sandbox<Assets>
        {
            public C() : base(new Assets()) { }
            public override void Dispose() { }
            protected override void WarmedUpHandler()
            {
                Debug.WriteLine("C WarmedUp");
            }
        }
        private class D : Sandbox<Assets>
        {
            public D() : base(new Assets()) { }
            public override void Dispose() { }
            protected override void WarmedUpHandler()
            {
                Debug.WriteLine("D WarmedUp");
            }
        }
    }

    
}
