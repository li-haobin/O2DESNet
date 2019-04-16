using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace O2DESNet.Animation
{
    public class Animator : IAnimator
    {
        public double Scale { get; set; } = 1;
        public double MaxWidth { get; set; } = 600;
        public double MaxHeight { get; set; } = 400;
        public double MaxX { get; set; } = 600;
        public double MaxY { get; set; } = 400;

        public Canvas MyCanvas { get; private set; } = new Canvas();
        private DateTime _currentTime = DateTime.MinValue;
        private DispatcherTimer _clockTimer;

        public bool IsShowUpdate { get; set; } = true;

        private class ObjectData
        {
            public string ObjectName { get; set; } = string.Empty;
            public Canvas Canvas { get; set; }

            //For movement animation
            public Storyboard Storyboard { get; set; }
            public RotateTransform RotationTransform { get; set; }
            public TranslateTransform TranslateTransform { get; set; }
        };

        private Dictionary<String, ObjectData> _objectDataList = new Dictionary<String, ObjectData>();
        private HashSet<DispatcherTimer> _dispatchList = new HashSet<DispatcherTimer>();

        public Animator()
        {
            MyCanvas.Margin = new Thickness(10);
        }

        private void Start()
        {
            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1) };
            _clockTimer.Tick += (s, e) =>
            {
                _currentTime = _currentTime.AddMilliseconds(_clockTimer.Interval.TotalMilliseconds * Scale);
            };
            _clockTimer.Start();
        }

        public void Add(Canvas canvas, string id, double x, double y, double degree, DateTime simlationTimeStamp)
        {
            if (_clockTimer == null)
            {
                _currentTime = simlationTimeStamp;
                Start();
            }

            if (!_objectDataList.ContainsKey(id))
            {
                Canvas objectCanvas = new Canvas();
                objectCanvas.Name = canvas.Name;
                objectCanvas.Children.Add(canvas);
                objectCanvas.Visibility = Visibility.Collapsed;
                MyCanvas.Children.Add(objectCanvas);

                _objectDataList.Add(id, new ObjectData { ObjectName = id, Canvas = objectCanvas });

                Storyboard storyboard = new Storyboard { Name = "Storyboard_" + id };
                _objectDataList[id].Storyboard = storyboard;
                MyCanvas.RegisterName("Storyboard_" + id, storyboard);

                if (!Double.IsNaN(canvas.Width) && !Double.IsNaN(canvas.Height))
                {
                    TranslateTransform animatedTranslateTransform = new TranslateTransform
                    {
                        X = x - (canvas.Width / 2),
                        Y = y - (canvas.Height / 2)
                    };
                    _objectDataList[id].TranslateTransform = animatedTranslateTransform;

                    RotateTransform animatedRotateTransform = new RotateTransform { Angle = degree };
                    _objectDataList[id].RotationTransform = animatedRotateTransform;

                    MyCanvas.RegisterName("AnimatedTranslateTransform_" + id, animatedTranslateTransform);
                    MyCanvas.RegisterName("AnimatedRotateTransform_" + id, animatedRotateTransform);

                    Execute(new Action(delegate { AddObject(id, x, y, degree); }), simlationTimeStamp);
                }
                else
                {
                    if (simlationTimeStamp <= _currentTime)
                    {
                        AddObject(id, x, y, degree);
                    }
                    else
                    {
                        Execute(new Action(delegate { AddObject(id, x, y, degree); }), simlationTimeStamp);
                    }
                }
            }
        }

        public void Move(string id, double x, double y, double degree, TimeSpan duration, DateTime simlationTimeStamp)
        {
            Execute(new Action(delegate { MoveObject(id, x, y, degree, duration); }), simlationTimeStamp);
        }

        public void Remove(string id, DateTime simlationTimeStamp)
        {
            Execute(new Action(delegate { RemoveObject(id); }), simlationTimeStamp);
        }

        public void Update(Canvas canvas, string id, DateTime simlationTimeStamp)
        {
            if (IsShowUpdate)
            {
                Execute(new Action(delegate { UpdateObject(canvas, id); }), simlationTimeStamp);
            }
        }

        //-----Private-----
        private void AddObject(string id, double x, double y, double degree)
        {
            if (IsCanvasFound(id))
            {
                _objectDataList[id].Canvas.Visibility = Visibility.Visible;
                MoveObject(id, x, y, degree, TimeSpan.FromMilliseconds(0));
            }
        }

        private void MoveObject(string id, double x, double y, double degree, TimeSpan duration)
        {
            if (IsCanvasFound(id))
            {
                ObjectData od = _objectDataList[id];
                Canvas canvas = od.Canvas;
                Canvas myObject = null;
                try
                {
                    myObject = (Canvas)canvas.Children[0];
                }
                catch
                {
                    myObject = null;
                }

                if (myObject != null && !double.IsNaN(myObject.Height) && !double.IsNaN(myObject.Width))
                {
                    TranslateTransform animatedTranslateTransform = _objectDataList[id].TranslateTransform;
                    RotateTransform animatedRotateTransform = _objectDataList[id].RotationTransform;

                    TransformGroup myTransformGroup = new TransformGroup();
                    myTransformGroup.Children.Add(animatedRotateTransform);
                    myTransformGroup.Children.Add(animatedTranslateTransform);
                    myObject.RenderTransform = myTransformGroup;
                    myObject.RenderTransformOrigin = new Point(0.5, 0.5);

                    Storyboard storyboard = _objectDataList[id].Storyboard;
                    storyboard.Children.Clear();

                    double timeToTravel = duration.TotalMilliseconds / Scale;
                    if (timeToTravel < 0)
                    {
                        timeToTravel = 0;
                    }

                    //Rotation
                    degree = (degree + 180) % 360;

                    //Rotation Adjustment
                    if (degree > 270 && animatedRotateTransform.Angle < 90)
                    {
                        RotateObject(id, 359.9, 1);
                    }
                    else if (degree < 90 && animatedRotateTransform.Angle > 270)
                    {
                        RotateObject(id, 0, 1);
                    }

                    RotateObject(id, degree, timeToTravel);
                    TransformObject(id, x - (myObject.Width / 2), y - (myObject.Height / 2), timeToTravel);

                    storyboard.Begin(MyCanvas, true);
                }
            }
        }

        private void RemoveObject(string id)
        {
            if (IsCanvasFound(id))
            {
                Canvas myObject = _objectDataList[id].Canvas;
                myObject.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateObject(Canvas canvas, string id)
        {
            if (IsCanvasFound(id))
            {
                Canvas myObject = _objectDataList[id].Canvas;
                myObject.Children.Clear();
                myObject.Children.Add(canvas);
            }
        }

        private void RotateObject(string id, double deg, double timeInMilliseconds)
        {
            DoubleAnimation rotationAnimation = new DoubleAnimation
            {
                To = deg,
                Duration = TimeSpan.FromMilliseconds(timeInMilliseconds)
            };
            Storyboard.SetTargetName(rotationAnimation, "AnimatedRotateTransform_" + id);
            Storyboard.SetTargetProperty(rotationAnimation, new PropertyPath(RotateTransform.AngleProperty));
            _objectDataList[id].Storyboard.Children.Add(rotationAnimation);
        }

        private void TransformObject(string id, double x, double y, double timeInMilliseconds)
        {
            DoubleAnimation translationAnimationX = new DoubleAnimation
            {
                To = x,
                Duration = TimeSpan.FromMilliseconds(timeInMilliseconds)
            };
            Storyboard.SetTargetName(translationAnimationX, "AnimatedTranslateTransform_" + id);
            Storyboard.SetTargetProperty(translationAnimationX, new PropertyPath(TranslateTransform.XProperty));
            _objectDataList[id].Storyboard.Children.Add(translationAnimationX);

            DoubleAnimation translationAnimationY = new DoubleAnimation
            {
                To = y,
                Duration = TimeSpan.FromMilliseconds(timeInMilliseconds)
            };
            Storyboard.SetTargetName(translationAnimationY, "AnimatedTranslateTransform_" + id);
            Storyboard.SetTargetProperty(translationAnimationY, new PropertyPath(TranslateTransform.YProperty));
            _objectDataList[id].Storyboard.Children.Add(translationAnimationY);
        }

        private bool IsCanvasFound(string id)
        {
            return _objectDataList.ContainsKey(id) && _objectDataList[id].Canvas != null;
        }

        private void Execute(Action action, DateTime simlationTimeStamp)
        {
            double timeToDelay = (simlationTimeStamp - _currentTime).TotalMilliseconds / Scale;
            if (timeToDelay < 0) timeToDelay = 0;
            TimeSpan delay = TimeSpan.FromMilliseconds(timeToDelay);

            DispatcherTimer animateTimer = new DispatcherTimer { Interval = delay };
            animateTimer.Tick += (s, e) =>
            {
                action();
                animateTimer.Stop();
                _dispatchList.Remove(animateTimer);
                animateTimer = null;
            };
            _dispatchList.Add(animateTimer);
            animateTimer.Start();
        }
    }
}
