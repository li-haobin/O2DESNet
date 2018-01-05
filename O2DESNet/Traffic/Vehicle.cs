using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace O2DESNet.Traffic
{
    public interface IVehicle : IModule, IDrawable
    {
        List<ControlPoint> Targets { get; }

        Event RemoveTarget();
        Event CalcMilage(double meters);
    }
    public class Vehicle : State<Vehicle.Statics>, IVehicle
    {
        #region Statics
        public class Statics : Scenario
        {
            public double Speed { get; set; }
            public string Tag { get; set; }

            #region For Drawing
            private bool _showTag = true;
            private Canvas _drawing = null;
            public TransformGroup TransformGroup { get; } = new TransformGroup();
            public Canvas Drawing { get { if (_drawing == null) UpdDrawing(); return _drawing; } }
            public bool ShowTag { get { return _showTag; } set { if (_showTag != value) { _showTag = value; UpdDrawing(); } } }
            public void UpdDrawing(DateTime? clockTime = null)
            {
                _drawing = new Canvas();
                _drawing.Children.Add(new System.Windows.Shapes.Path
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Data = new PathGeometry
                    {
                        Figures = new PathFigureCollection(new PathFigure[] {
                            new PathFigure(
                                new Point(-70, -15),
                                new LineSegment[]{
                                    new LineSegment(new Point(-70, 15), true),
                                    new LineSegment(new Point(70, 15), true),
                                    new LineSegment(new Point(70, -15), true),
                                }, true)
                        })
                    },
                    Fill = Brushes.Orange,
                });
                _drawing.Children.Add(new System.Windows.Shapes.Path
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 3,
                    Data = new PathGeometry
                    {
                        Figures = new PathFigureCollection(new PathFigure[] {
                            new PathFigure(new Point(-10, -10), new LineSegment[]{
                                new LineSegment(new Point(10, 0), true),
                                new LineSegment(new Point(-10, 10), true),
                            },
                            false)
                        })
                    },
                });
                if (ShowTag) _drawing.Children.Add(new TextBlock
                {
                    Text = Tag,
                    FontSize = 10,
                    Margin = new Thickness(30, 0, 0, 0),
                });
                _drawing.Opacity = 0.5;
                _drawing.RenderTransform = TransformGroup;
            }
            #endregion          
        }
        #endregion

        #region Dynamics
        public List<ControlPoint> Targets { get; private set; } = new List<ControlPoint>();
        public Dictionary<int, double> Milage { get; private set; } = new Dictionary<int, double> { { 0, 0 } };
        public Dictionary<int, List<TimeSpan>> Duration { get; private set; } = new Dictionary<int, List<TimeSpan>>();
        public int CurrentPhase { get; private set; } = 0;
        /// <summary>
        /// The time when the current phase started
        /// </summary>
        public DateTime TimeStamp { get; private set; } = DateTime.MinValue;
        #endregion

        #region Events
        private abstract class InternalEvent : Event { internal Vehicle This { get; set; } } // event adapter       

        private class AddTargetEvent : InternalEvent
        {
            internal ControlPoint Target { get; set; }
            public override void Invoke()
            {
                This.Targets.Add(Target);
            }
        }
        private class SetTargetsEvent : InternalEvent
        {
            internal List<ControlPoint> Targets { get; set; }
            public override void Invoke()
            {
                This.Targets = new List<ControlPoint>(Targets);
            }
        }

        private class RemoveTargetEvent : InternalEvent { public override void Invoke() { This.Targets.RemoveAt(0); } }

        private class IncreaseMilageEvent : InternalEvent
        {
            internal double Meters { get; set; }
            public override void Invoke()
            {
                This.Milage[0] += Meters;
                if (!This.Milage.ContainsKey(This.CurrentPhase)) This.Milage.Add(This.CurrentPhase, 0);
                else This.Milage[This.CurrentPhase] += Meters;
            }
        }
        private class UpdPhaseEvent : InternalEvent
        {
            internal int Phase { get; set; }
            public override void Invoke()
            {
                if (Phase == This.CurrentPhase) return;
                if (This.CurrentPhase > 0) This.Duration[This.CurrentPhase].Add(ClockTime - This.TimeStamp); // do not count if current state is 0

                if (!This.Milage.ContainsKey(Phase)) This.Milage.Add(Phase, 0);
                if (!This.Duration.ContainsKey(Phase) && Phase > 0) This.Duration.Add(Phase, new List<TimeSpan>());
                This.CurrentPhase = Phase;
                This.TimeStamp = ClockTime;
            }
        }
        #endregion

        #region Input Events - Getters
        public Event AddTarget(ControlPoint target) { return new AddTargetEvent { This = this, Target = target }; }
        public Event SetTargets(List<ControlPoint> targets) { return new SetTargetsEvent { This = this, Targets = targets }; }
        public Event RemoveTarget() { return new RemoveTargetEvent { This = this }; }
        public Event CalcMilage(double meters)
        {
            return new IncreaseMilageEvent { This = this, Meters = meters };
        }
        public Event UpdPhase(int phase) { return new UpdPhaseEvent { This = this, Phase = phase }; }
        #endregion

        public Vehicle() : base(new Statics()) { Name = "Veh"; }
        public Vehicle(Statics category, int seed, string tag = null) : base(category, seed, tag) { Name = "Veh"; }

        public override void WarmedUp(DateTime clockTime)
        {
            foreach (var phase in Milage.Keys.ToList()) Milage[phase] = 0;
            foreach (var phase in Duration.Keys.ToList()) Duration[phase] = new List<TimeSpan>();
            TimeStamp = clockTime;
        }

        #region For Drawing
        private Canvas _drawing = null;
        public TransformGroup TransformGroup { get { return Config.TransformGroup; } }
        public Canvas Drawing { get { if (_drawing == null) { UpdDrawing(); } return _drawing; } }
        public bool ShowTag { get { return Config.ShowTag; } set { Config.ShowTag = value; UpdDrawing(); } }        
        public void UpdDrawing(DateTime? clockTime = null)
        {
            Config.UpdDrawing();
            _drawing = Config.Drawing;
        }
        #endregion
    }
}
