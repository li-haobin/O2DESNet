﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using O2DESNet.Drawing;

namespace O2DESNet.Traffic
{
    public class Path : State<Path.Statics>, IDrawable
    {
        #region Statics
        public class Statics : Scenario, IDrawable
        {
            public int Index { get; internal set; }
            public string Tag { get; set; }
            public PathMover.Statics PathMover { get; internal set; }
            internal Statics() { }
            public Statics(ControlPoint start, ControlPoint end, List<Point> trajectory = null)
            {
                Trajectory = trajectory ?? new List <Point> { new Point(start.X, start.Y), new Point(end.X, end.Y) };
                Start = start;
                End = end;
                start.PathsOut.Add(this);
                end.PathsIn.Add(this);
                for (int i = 0; i < Trajectory.Count - 1; i++) Length += (Trajectory[i + 1] - Trajectory[i]).Length / 10;
            }

            public double Length { get; set; }
            public int Capacity { get; set; }
            /// <summary>
            /// Function map vehicle density in # vehicles per meter,  to the speed in meters per second
            /// </summary>
            public Func<double, double> SpeedByDensity { get; set; } = d => 4.5;
            public ControlPoint Start { get; internal set; }
            public ControlPoint End { get; internal set; }
            public bool CrossHatched { get; set; } = false;

            #region For Drawing
            public List<Point> Trajectory { get; private set; } = new List<Point>();
            public bool TurnWithRoad { get; set; } = true;
            internal TransformGroup SlipOnCurve(double ratio, bool turnWithRoad = false)
            {
                var distances = new List<double>();

                Func<Vector, double> l2Norm = v => Math.Sqrt(v.X * v.X + v.Y * v.Y);
                Func<Vector, double> degree = v => Math.Atan2(v.Y, v.X) / Math.PI * 180;
                for (int i = 0; i < Trajectory.Count - 1; i++)
                    distances.Add(l2Norm(Trajectory[i + 1] - Trajectory[i]));
                var total = distances.Sum();
                var cum = 0d;
                var dist = total * ratio;
                for (int i = 0; i < distances.Count; i++)
                {
                    cum += distances[i];
                    if (dist <= cum)
                    {
                        var p = Trajectory[i + 1] - (Trajectory[i + 1] - Trajectory[i]) / distances[i] * (cum - dist);
                        var tg = new TransformGroup();
                        if (TurnWithRoad || turnWithRoad) tg.Children.Add(new RotateTransform(degree(Trajectory[i + 1] - Trajectory[i])));
                        tg.Children.Add(new TranslateTransform(p.X, p.Y));
                        return tg;
                    }
                }
                throw new Exception("The path must contain more than one point.");
            }

            private bool _showTag = true;
            private Canvas _drawing = null;
            public TransformGroup TransformGroup { get; } = new TransformGroup();
            public Canvas Drawing { get { if (_drawing == null) UpdDrawing(); return _drawing; } }
            public bool ShowTag { get { return _showTag; } set { if (_showTag != value) { _showTag = value; UpdDrawing(); } } }
            public void UpdDrawing(DateTime? clockTime = null)
            {
                _drawing = new Canvas();
                _drawing.Children.Add(new System.Windows.Shapes.Path // Trajectory
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection(new double[] { 3, 3 }),
                    Data = new PathGeometry
                    {
                        Figures = new PathFigureCollection(new PathFigure[] {
                            new PathFigure(Trajectory.First(),
                            Trajectory.GetRange(1, Trajectory.Count - 1).Select(pt => new LineSegment(pt, true)),
                            false)
                        })
                    }
                });
                _drawing.Children.Add(new System.Windows.Shapes.Path // Arrow
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Data = new PathGeometry
                    {
                        Figures = new PathFigureCollection(new PathFigure[] {
                            new PathFigure(new Point(-6, -3), new LineSegment[]{
                                new LineSegment(new Point(0, 0), true),
                                new LineSegment(new Point(-6, 3), true),
                            },
                            false)
                        })
                    },
                    RenderTransform = SlipOnCurve(0.4, true),
                });
                if (ShowTag) _drawing.Children.Add(new TextBlock
                {
                    Text = Tag,
                    FontSize = 4,
                    RenderTransform = SlipOnCurve(0.4),
                    
                });
                _drawing.RenderTransform = TransformGroup;
            }
            #endregion            
        }
        #endregion

        #region Dynamics
        public Dictionary<IVehicle, double> VehiclePositions { get; private set; } = new Dictionary<IVehicle, double>();
        public Dictionary<IVehicle, DateTime> VehicleCompletionTimes { get; private set; } = new Dictionary<IVehicle, DateTime>();
        public Queue<IVehicle> VehiclesCompleted { get; private set; } = new Queue<IVehicle>();
        public DateTime LastTimeStamp { get; private set; }
        /// <summary>
        /// Record the availabilities of the following paths for the vehicle to exit to,
        /// </summary>
        public Dictionary<Statics, bool> ToEnter { get; private set; } = new Dictionary<Statics, bool>();
        /// <summary>
        /// Record if the paths are exitable at when the vehicle completes its target.
        /// </summary>
        public Dictionary<Statics, bool> ToArrive { get; private set; } = new Dictionary<Statics, bool>();
        public double CurrentSpeed { get; private set; }
        public int Occupancy { get { return VehiclePositions.Count + VehiclesCompleted.Count; } }
        public int Vacancy { get { return Config.Capacity - Occupancy; } }
        public bool LockedByPaths { get; private set; } = false;
        public HourCounter HC_AllVehicles { get; private set; } = new HourCounter();
        public HourCounter HC_VehiclesTravelling { get; private set; } = new HourCounter();
        public HourCounter HC_VehiclesCompleted { get; private set; } = new HourCounter();
        #endregion

        #region Events
        private abstract class InternalEvent : Event<Path, Statics>
        {
            internal void UpdPositions()
            {
                var distance = This.CurrentSpeed * (ClockTime - This.LastTimeStamp).TotalSeconds;
                foreach (var veh in This.VehiclePositions.Keys.ToArray()) This.VehiclePositions[veh] += distance;
                This.LastTimeStamp = ClockTime;
            }
            protected void Count()
            {
                This.HC_AllVehicles.ObserveCount(This.Occupancy, ClockTime);
                This.HC_VehiclesTravelling.ObserveCount(This.VehiclePositions.Count, ClockTime);
                This.HC_VehiclesCompleted.ObserveCount(This.VehiclesCompleted.Count, ClockTime);
            }
        } // event adapter 

        // Alpha_1
        private class EnterEvent : InternalEvent
        {
            internal IVehicle Vehicle { get; set; }
            public override void Invoke()
            {
                if (This.Vacancy < 1) throw new Exception("There is no vacancy for incoming vehicle.");
                UpdPositions();
                This.VehiclePositions.Add(Vehicle, 0);
                This.VehicleCompletionTimes.Add(Vehicle, ClockTime);
                Execute(new UpdCompletionEvent());
                Execute(This.OnVacancyChg.Select(e => e()));
                //Log("Start,{0},{1}", Vehicle, This);
            }
        }

        // Alpha_2
        private class UpdToExitEvent : InternalEvent
        {
            internal Path Path { get; set; } // the path after which vehicles complete the last targets
            internal bool ToExit { get; set; }
            public override void Invoke()
            {
                if (This.ToArrive.ContainsKey(Path.Config)) This.ToArrive[Path.Config] = ToExit;
                else This.ToArrive.Add(Path.Config, ToExit);
                Execute(new ExitEvent());
            }
        }

        // Alpha_3
        private class UpdToEnterEvent : InternalEvent
        {
            internal Path Path { get; set; } // the path which vehicles exit to
            internal bool ToEnter { get; set; }
            public override void Invoke()
            {
                if (This.ToEnter.ContainsKey(Path.Config)) This.ToEnter[Path.Config] = ToEnter;
                else This.ToEnter.Add(Path.Config, ToEnter);
                Execute(new ExitEvent());
            }
        }

        // Alpha_4
        private class ResetEvent : InternalEvent
        {
            public override void Invoke()
            {
                This.VehiclePositions = new Dictionary<IVehicle, double>();
                This.VehicleCompletionTimes = new Dictionary<IVehicle, DateTime>();
                This.VehiclesCompleted = new Queue<IVehicle>();
                This.LastTimeStamp = ClockTime;
                foreach (var path in This.ToEnter.Keys.ToList()) This.ToEnter[path] = true;
                This.LockedByPaths = false;
                Count();
            }
        }

        // Beta_1
        private class UpdCompletionEvent : InternalEvent
        {
            public override void Invoke()
            {
                Count();
                This.CurrentSpeed = This.Config.SpeedByDensity(This.Occupancy / This.Config.Length);
                foreach (var veh in This.VehiclePositions.Keys)
                {
                    var completionTime = ClockTime + TimeSpan.FromSeconds(Math.Max(0, (Config.Length - This.VehiclePositions[veh]) / This.CurrentSpeed));
                    Schedule(new AttemptToCompleteEvent { Vehicle = veh }, completionTime);
                    This.VehicleCompletionTimes[veh] = completionTime;
                }
            }
        }

        // Beta_2
        private class AttemptToCompleteEvent : InternalEvent
        {
            internal IVehicle Vehicle { get; set; }
            public override void Invoke()
            {
                if (!This.VehicleCompletionTimes.ContainsKey(Vehicle) || !This.VehicleCompletionTimes[Vehicle].Equals(ClockTime)) return;
                This.VehiclePositions.Remove(Vehicle);
                This.VehicleCompletionTimes.Remove(Vehicle);
                This.VehiclesCompleted.Enqueue(Vehicle);
                UpdPositions();
                Execute(new ExitEvent());
            }
        }

        // Beta_3
        private class ExitEvent : InternalEvent
        {
            public override void Invoke()
            {
                if (This.VehiclesCompleted.Count == 0) return;

                var prevLockedByPaths = This.LockedByPaths;
                var vehicle = This.VehiclesCompleted.First();
                var target = vehicle.Targets.First();
                if (vehicle.Targets.Count > 1 && target.Equals(Config.End)) target = vehicle.Targets[1]; // if multiple targets exists, and the 1st target is to be reached, look at the 2nd target
                var exit = false;
                if (target == This.Config.End)  // target is reached at the End
                {
                    exit = This.ToArrive[Config];
                    This.LockedByPaths = false;
                }
                else
                {
                    var next = Config.End.PathTo(target);
                    exit = This.ToEnter[next];
                    This.LockedByPaths = !exit;

                    if (next.CrossHatched)
                    {
                        if (target == next.End)
                        {
                            exit &= This.ToArrive[next];
                        }
                        else
                        {
                            var next2 = next.End.PathTo(target);
                            exit &= This.ToEnter[next2];
                            This.LockedByPaths = !exit;
                            // both next and next^2 path should be available if the next is cross-hatched
                        }
                    }
                }
                if (exit)
                {
                    var veh = This.VehiclesCompleted.Dequeue();
                    //Log("Exit,{0},{1}", veh, This);
                    Execute(This.OnExit.Select(e => e(veh)));
                    Execute(new ExitEvent());
                    Execute(new UpdCompletionEvent());
                    Execute(This.OnVacancyChg.Select(e => e()));
                }
                if (!prevLockedByPaths && This.LockedByPaths || This.VehiclesCompleted.Count == 0)
                    Execute(This.OnLockedByPaths.Select(e => e()));
            }
        }
        #endregion

        #region Input Events - Getters
        public Event Enter(IVehicle vehicle) { return new EnterEvent { This = this, Vehicle = vehicle }; }
        /// <summary>
        /// Update the state of the following paths, 
        /// if they are available for the vehicle to exit to.
        /// </summary>
        public Event UpdToEnter(Path path, bool toEnter) { return new UpdToEnterEvent { This = this, Path = path, ToEnter = toEnter }; }
        /// <summary>
        /// Update the state of the current or following paths (if it is cross-hatched), 
        /// if they are exitable at the end when vehicles reached the last target.
        /// </summary>
        public Event UpdToExit(Path path, bool toExit)
        {
            return new UpdToExitEvent
            {
                This = this,
                Path = path,
                ToExit = toExit
            };
        }
        /// <summary>
        /// Reset the Path by emptying all the Vehicles, and releasing all locks cause by other congested paths (the locks for arriving which is not caused by congestion remain).
        /// </summary>
        public Event Reset() { return new ResetEvent { This = this }; }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<IVehicle, Event>> OnExit { get; private set; } = new List<Func<IVehicle, Event>>();
        public List<Func<Event>> OnVacancyChg { get; private set; } = new List<Func<Event>>();
        public List<Func<Event>> OnLockedByPaths { get; private set; } = new List<Func<Event>>();
        #endregion

        public Path(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "Path";
        }

        public override void WarmedUp(DateTime clockTime)
        {
            HC_AllVehicles.WarmedUp(clockTime);
            HC_VehiclesTravelling.WarmedUp(clockTime);
            HC_VehiclesCompleted.WarmedUp(clockTime);
        }

        public override void WriteToConsole(DateTime? clockTime = default(DateTime?))
        {
            Console.Write("Path{0} (CP{1} -> CP{2})", Config.Index, Config.Start.Index, Config.End.Index);
            if (VehiclePositions.Count > 0)
            {
                Console.Write("\tTravelling: ");
                foreach (var veh in VehiclePositions.Keys) Console.Write("{0} ", veh);
            }
            if (VehiclesCompleted.Count > 0)
            {
                Console.Write("\tCompleted: ");
                foreach (var veh in VehiclesCompleted) Console.Write("{0} ", veh);
            }
            Console.WriteLine();
        }

        #region For Drawing
        private DateTime? _timestamp = null;
        private Canvas _drawing = null;
        private System.Windows.Shapes.Path _overlay_red, _overlay_green;
        private Dictionary<IVehicle, Canvas> _drawing_vehicles = new Dictionary<IVehicle, Canvas>();

        public TransformGroup TransformGroup { get { return Config.TransformGroup; } } 
        public Canvas Drawing { get { if (_drawing == null) { InitDrawing(); UpdDrawing(); } return _drawing; } }
        public bool ShowTag {
            get { return Config.ShowTag; }
            set { if (Config.ShowTag != value) { Config.ShowTag = value; UpdDrawing(_timestamp); } }
        }
        private void InitDrawing()
        {
            _drawing = new Canvas();
            _overlay_red = new System.Windows.Shapes.Path
            {
                Stroke = Brushes.Red,
                StrokeThickness = 5,
                Data = new PathGeometry
                {
                    Figures = new PathFigureCollection(new PathFigure[] {
                            new PathFigure(Config.Trajectory.First(),
                            Config.Trajectory.GetRange(1, Config.Trajectory.Count - 1).Select(pt => new LineSegment(pt, true)),
                            false)
                        })
                },
                Opacity = 0,
                RenderTransform = TransformGroup,
            };
            _overlay_green = new System.Windows.Shapes.Path
            {
                Stroke = Brushes.Green,
                StrokeThickness = 5,
                Data = new PathGeometry
                {
                    Figures = new PathFigureCollection(new PathFigure[] {
                            new PathFigure(Config.Trajectory.First(),
                            Config.Trajectory.GetRange(1, Config.Trajectory.Count - 1).Select(pt => new LineSegment(pt, true)),
                            false)
                        })
                },
                Opacity = 0.4,
                RenderTransform = TransformGroup,
            };
            _drawing.Children.Add(Config.Drawing);
            _drawing.Children.Add(_overlay_red);
            _drawing.Children.Add(_overlay_green);
        }
        public void UpdDrawing(DateTime? clockTime = null)
        {
            clockTime = clockTime ?? LastTimeStamp;
            clockTime = clockTime >= LastTimeStamp ? clockTime : LastTimeStamp;
            _timestamp = clockTime;
            var toRemove = _drawing_vehicles.Keys.Where(veh => !VehiclePositions.ContainsKey(veh) && !VehiclesCompleted.Contains(veh)).ToList();
            foreach (var veh in toRemove)
            {
                _drawing.Children.Remove(_drawing_vehicles[veh]);
                _drawing_vehicles.Remove(veh);
            }
            var toAdd = VehiclePositions.Keys.Concat(VehiclesCompleted).Where(veh => !_drawing_vehicles.ContainsKey(veh)).ToList();
            foreach (var veh in toAdd)
            {
                var drw = veh.Drawing;
                _drawing_vehicles.Add(veh, drw);
                _drawing.Children.Add(drw);
            }
            foreach (var veh in _drawing_vehicles.Keys)
            {
                veh.ShowTag = ShowTag;
                var tg = Config.SlipOnCurve(
                    VehiclePositions.ContainsKey(veh) ?
                    Math.Min(1.0, (VehiclePositions[veh] + (clockTime.Value - LastTimeStamp).TotalSeconds * CurrentSpeed) / Config.Length) : 1.0
                    );
                foreach (var t in TransformGroup.Children) tg.Children.Add(t);
                _drawing_vehicles[veh].RenderTransform = tg;
            }
            if (Occupancy > 0)
            {
                _overlay_red.Opacity = 0.4;
                _overlay_green.Opacity = 0;
            }
            else
            {
                _overlay_red.Opacity = 0;
                _overlay_green.Opacity = 0.4;
            }
        }
        #endregion
    }
}