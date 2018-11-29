using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
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

        //private Window mainWindow;
        public Canvas MyCanvas { get; private set; } = new Canvas();
        private DateTime SimStartTime { get; set; }
        private DateTime CompStartTime { get; set; }

        private struct ObjectData
        {
            public string ObjectName { get; set; }
            public List<EventData> EventDataList { get; set; }
            public bool StoryboardIsPlaying { get; set; }
            public Canvas Canvas { get; set; }
            public bool AddedToScene { get; set; }

            public ObjectData(String name, Canvas cvs)
            {
                ObjectName = name;
                EventDataList = new List<EventData>();
                StoryboardIsPlaying = false;
                Canvas = cvs;
                AddedToScene = false;
            }
        };

        private struct EventData
        {
            public Vector Position { get; set; }
            public double Rotation { get; set; }
            public DateTime TimeToArrive { get; set; }
            //public TimeSpan travelDuration { get; set; }

            public EventData(Vector pos, double rotate, DateTime time, TimeSpan duration)
            {
                Position = pos;
                Rotation = rotate;
                TimeToArrive = time;
                //travelDuration = new TimeSpan();
            }
        };

        private Dictionary<String, ObjectData> objectDataList = new Dictionary<String, ObjectData>();
        private Vector MapScale { get; set; }

        public Animator()
        {
            MyCanvas.Margin = new Thickness(10);

            //mainWindow = Application.Current.MainWindow;
            //mainWindow.Content = MyCanvas;
            //NameScope.SetNameScope(mainWindow, new NameScope());
        }

        public void Start()
        {
            CompStartTime = DateTime.Now;
            SimStartTime = new DateTime(1, 1, 1, 0, 0, 0, 0);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += new EventHandler(Updater);
            timer.Start();
        }

        public void Add(Canvas canvas, string id, double x, double y, double degree, DateTime simlationTimeStamp)
        {
            if (CheckIfObjectInCanvas(id))
            {
                return;
            }

            double timeToDelay = (ConvertToCompTime(simlationTimeStamp) - DateTime.Now).TotalMilliseconds;
            if (timeToDelay < 0)
            {
                timeToDelay = 0;
            }

            objectDataList.Add(id, new ObjectData(id, canvas));

            Storyboard storyboard = new Storyboard
            {
                Name = "StoryboardName" + id
            };
            MyCanvas.RegisterName("myStoryboard" + id, storyboard);
            MyCanvas.RegisterName("AnimatedTransform" + id, canvas);
            TranslateTransform animatedTranslateTransform = new TranslateTransform();
            MyCanvas.RegisterName("AnimatedTranslateTransform" + id, animatedTranslateTransform);
            RotateTransform animatedRotateTransform = new RotateTransform(degree);
            MyCanvas.RegisterName("AnimatedRotateTransform" + id, animatedRotateTransform);

            EventData ed = new EventData();
            ed.Position = new Vector(x, y);
            ed.Rotation = degree;
            ed.TimeToArrive = simlationTimeStamp;
            objectDataList[id].EventDataList.Add(ed);

            if (timeToDelay <= 0)
            {
                AddObject(canvas, id, x, y, degree, simlationTimeStamp);
            }
            else
            {
                ExecuteWithDelay(new Action(delegate { AddObject(canvas, id, x, y, degree, simlationTimeStamp); }), TimeSpan.FromMilliseconds(timeToDelay / Scale));
            }
        }

        public void Move(string id, double x, double y, double degree, DateTime simlationTimeStamp)
        {
            DateTime timeStamp = ConvertToCompTime(simlationTimeStamp);
            if (!CheckIfObjectInCanvas(id))
            {
                return;
            }

            Storyboard storyboard = (Storyboard)MyCanvas.FindName("myStoryboard" + id);
            if (storyboard != null)
            {
                double newDuration = (timeStamp - DateTime.Now).TotalMilliseconds;
                if (newDuration < 0)
                {
                    newDuration = 0;
                }

                //if (storyboard.GetCurrentTime(MyCanvas) != null)
                //{
                //    double currentDurationRemaining = (storyboard.Duration.TimeSpan - (TimeSpan)storyboard.GetCurrentTime(MyCanvas)).TotalMilliseconds;
                //    if (newDuration < currentDurationRemaining)
                //    {
                //        //throw new Exception("Invalid time given - Cannot be earlier than previous timeStamp");
                //    }
                //}

                EventData ed = new EventData();
                ed.Position = new Vector(x, y);
                ed.Rotation = degree;
                ed.TimeToArrive = simlationTimeStamp;
                objectDataList[id].EventDataList.Add(ed);
            }
        }

        public void Remove(string id, DateTime timeStamp)
        {
            double timeToDelay = (ConvertToCompTime(timeStamp) - DateTime.Now).TotalMilliseconds;
            if (timeToDelay < 0)
            {
                timeToDelay = 0;
            }
            ExecuteWithDelay(new Action(delegate { RemoveObject(id); }), TimeSpan.FromMilliseconds(timeToDelay / Scale));
        }

        public void Update(Canvas canvas, string id, DateTime simlationTimeStamp)
        {
            DateTime timeStamp = ConvertToCompTime(simlationTimeStamp);
            if (!CheckIfObjectInCanvas(id))
            {
                return;
            }

            Storyboard storyboard = (Storyboard)MyCanvas.FindName("myStoryboard" + id);
            if (storyboard != null)
            {
                double timeToDelay = (timeStamp - DateTime.Now).TotalMilliseconds;
                if (timeToDelay < 0)
                {
                    timeToDelay = 0;
                }
                ExecuteWithDelay(new Action(delegate { UpdateObject(canvas, id); }), TimeSpan.FromMilliseconds(timeToDelay / Scale));
            }
        }

        //-----Private-----

        private void AddObject(Canvas canvas, string id, double x, double y, double degree, DateTime timeStamp)
        {
            if (!CheckIfObjectInCanvas(id))
            {
                return;
            }

            Storyboard storyboard = (Storyboard)MyCanvas.FindName("myStoryboard" + id);

            MyCanvas.Children.Add(canvas);

            EventHandler handler = (s, e) => {
                Storyboard_MoveCompleted(s, e, id);
            };
            storyboard.CurrentStateInvalidated += handler;

            ObjectData od = objectDataList[id];
            od.AddedToScene = true;
            objectDataList[id] = od;

            MoveObject(id, x, y, degree, timeStamp);
        }

        private void MoveObject(string id, double x, double y, double degree, DateTime timeStamp_sim)
        {
            ObjectData od = objectDataList[id];
            od.StoryboardIsPlaying = true;
            objectDataList[id] = od;

            if (!Double.IsNaN(od.Canvas.Height) && !Double.IsNaN(od.Canvas.Width))
            {
                TranslateTransform animatedTranslateTransform = (TranslateTransform)MyCanvas.FindName("AnimatedTranslateTransform" + id);
                RotateTransform animatedRotateTransform = (RotateTransform)MyCanvas.FindName("AnimatedRotateTransform" + id);
                Canvas myObject = (Canvas)MyCanvas.FindName("AnimatedTransform" + id);

                TransformGroup myTransformGroup = new TransformGroup();
                myTransformGroup.Children.Add(animatedRotateTransform);
                myTransformGroup.Children.Add(animatedTranslateTransform);
                myObject.RenderTransform = myTransformGroup;
                myObject.RenderTransformOrigin = new Point(0.5, 0.5);

                Storyboard storyboard = (Storyboard)MyCanvas.FindName("myStoryboard" + id);
                storyboard.Children.Clear();
                SetMapScale();

                //Translation
                DoubleAnimation translationAnimationX = new DoubleAnimation();
                translationAnimationX.To = (x - (od.Canvas.Width / 2)) * MapScale.X;
                Storyboard.SetTargetName(translationAnimationX, "AnimatedTranslateTransform" + id);
                Storyboard.SetTargetProperty(translationAnimationX, new PropertyPath(TranslateTransform.XProperty));
                DoubleAnimation translationAnimationY = new DoubleAnimation();
                translationAnimationY.To = (y - (od.Canvas.Height / 2)) * MapScale.Y;
                Storyboard.SetTargetName(translationAnimationY, "AnimatedTranslateTransform" + id);
                Storyboard.SetTargetProperty(translationAnimationY, new PropertyPath(TranslateTransform.YProperty));
                //Trace.WriteLine("MoveX: " + x + ", " + mapScale.X + ", " + translationAnimationX.To);

                DateTime timeStamp_comp = ConvertToCompTime(timeStamp_sim);
                TimeSpan timeElasped_comp = DateTime.Now - CompStartTime;
                TimeSpan timeFromStart_sim = timeStamp_sim - SimStartTime;
                double timeFromStart_comp = (timeFromStart_sim.TotalMilliseconds / Scale);
                double timeToTravel = (timeFromStart_comp - timeElasped_comp.TotalMilliseconds);// * Scale;
                if (timeToTravel < 0)
                {
                    timeToTravel = 0;
                }
                translationAnimationX.Duration = (TimeSpan.FromMilliseconds(timeToTravel));// / Scale)); / Scale));
                translationAnimationY.Duration = (TimeSpan.FromMilliseconds(timeToTravel));// / Scale)); / Scale));
                storyboard.Children.Add(translationAnimationX);
                storyboard.Children.Add(translationAnimationY);

                //Rotation
                double deg = degree + 180;
                //double oneeighty = 180;
                //deg += oneeighty;

                if (deg < 360.1 && deg > 359.9)
                {
                    deg = 0;
                }
                DoubleAnimation rotationAnimation = new DoubleAnimation();
                rotationAnimation.To = deg;
                Storyboard.SetTargetName(rotationAnimation, "AnimatedRotateTransform" + id);
                Storyboard.SetTargetProperty(rotationAnimation, new PropertyPath(RotateTransform.AngleProperty));
                rotationAnimation.Duration = (TimeSpan.FromMilliseconds(timeToTravel));// / Scale)); / Scale));
                storyboard.Children.Add(rotationAnimation);

                storyboard.Duration = (TimeSpan.FromMilliseconds(timeToTravel));// / Scale)); / Scale));
                storyboard.Begin(MyCanvas, true);
            }
        }

        private void RemoveObject(string id)
        {
            Storyboard storyboard = (Storyboard)MyCanvas.FindName("myStoryboard" + id);
            if (storyboard != null)
            {
                EventHandler handler;
                handler = (s, e) =>
                {
                    Storyboard_MoveCompleted(s, e, id);
                };
                storyboard.CurrentStateInvalidated -= handler;

                string nameRemoveAnimatedTransform = "AnimatedTransform" + id;
                string nameRemoveAnimatedTranslateTransform = "AnimatedTranslateTransform" + id;
                string nameRemoveAnimatedRotateTransform = "AnimatedRotateTransform" + id;
                string nameRemoveStoryboard = "myStoryboard" + id;

                Canvas transform = (Canvas)MyCanvas.FindName(nameRemoveAnimatedTransform);
                if (transform != null)
                {
                    MyCanvas.UnregisterName(nameRemoveAnimatedTransform);
                    MyCanvas.UnregisterName(nameRemoveAnimatedTranslateTransform);
                    MyCanvas.UnregisterName(nameRemoveAnimatedRotateTransform);
                    MyCanvas.UnregisterName(nameRemoveStoryboard);
                }
                MyCanvas.Children.Remove(transform);
            }

            //for (int i = 0; i < MyCanvas.Children.Count; i++)
            //{
            //    String shapeName = ((Visual)(MyCanvas.Children[i])).ToString();
            //    if (shapeName.Contains("Rectangle"))
            //    {
            //        Rectangle pointRectElem = (Rectangle)MyCanvas.Children[i];
            //        if (pointRectElem.Name.Contains(id))
            //        {
            //            MyCanvas.Children.Remove(MyCanvas.Children[i]);
            //            i--;
            //            continue;
            //        }
            //    }
            //}
        }

        private void UpdateObject(Canvas canvas, string id)
        {
            Canvas myObject = (Canvas)MyCanvas.FindName("AnimatedTransform" + id);
            myObject.Children.Clear();
            UIElement[] elements = new UIElement[canvas.Children.Count];
            canvas.Children.CopyTo(elements, 0);

            for (int i = 0; i < elements.Length; i++)
            {
                canvas.Children.Remove(elements[i]);
                myObject.Children.Add(elements[i]);
            }

            ObjectData od = objectDataList[id];
            od.Canvas = myObject;
            objectDataList[id] = od;
        }

        private DateTime ConvertToCompTime(DateTime simTime)
        {
            TimeSpan timeDifference = simTime - SimStartTime;
            DateTime convertedTime = CompStartTime + timeDifference;

            return convertedTime;
        }

        //private Rectangle DrawRectangleBasicReturn(Canvas canvas, double width, double height, string name, SolidColorBrush colour, Vector position)
        //{
        //    Rectangle myRect = new Rectangle
        //    {
        //        Name = name
        //    };
        //    myRect.Height = height;
        //    myRect.Width = width;
        //    myRect.Fill = colour;
        //    canvas.Children.Add(myRect);

        //    return myRect;
        //}

        private bool CheckIfObjectInCanvas(string id)
        {
            //bool objectInCanvas = false;
            //for (int i = 0; i < MyCanvas.Children.Count; i++)
            //{
            //    String shapeName = ((Visual)(MyCanvas.Children[i])).ToString();
            //    if (shapeName.Contains("Canvas"))
            //    {
            //        Canvas canvasElem = (Canvas)MyCanvas.Children[i];
            //        if (canvasElem.Name.Contains(id))
            //        {
            //            objectInCanvas = true;
            //            break;
            //        }
            //    }
            //}
            //return objectInCanvas;

            string nameRemoveAnimatedTransform = "AnimatedTransform" + id;
            Canvas transform = (Canvas)MyCanvas.FindName(nameRemoveAnimatedTransform);

            if (transform == null)
            {
                return false;
            }

            return true;
        }

        //-----Static-----
        private static void ExecuteWithDelay(Action action, TimeSpan delay)
        {
            double timeToDelay = delay.TotalMilliseconds;
            if (timeToDelay < 0)
            {
                timeToDelay = 0;
            }
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = delay;
            timer.Tag = action;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        private static void timer_Tick(object sender, EventArgs e)
        {
            DispatcherTimer timer = (DispatcherTimer)sender;
            Action action = (Action)timer.Tag;

            action.Invoke();
            timer.Stop();
            timer = null;
        }

        //-----Events-----
        private void Storyboard_MoveCompleted(object sender, EventArgs e, string id)
        {
            ObjectData od = objectDataList[id];

            Clock myStoryboardClock = (Clock)sender;
            if (myStoryboardClock.CurrentState == ClockState.Filling)
            {
                od.StoryboardIsPlaying = false;
                objectDataList[id] = od;

                if (objectDataList[id].EventDataList.Count > 0)
                {
                    objectDataList[id].EventDataList.RemoveAt(0);
                }
            }
        }

        //-----Misc-----
        //private TextBlock _clockTime_textblock = new TextBlock();
        private void Updater(object sender, EventArgs e) //done
        {
            //DateTime clockTime = DateTime.Now;
            //_clockTime_textblock.Text = clockTime.ToString();
            //_clockTime_textblock.Margin = new Thickness(320, 0, 0, 0);

            CheckForNewMove();
        }

        private void CheckForNewMove()
        {
            List<ObjectData> objList = objectDataList.Values.ToList();

            foreach (ObjectData obj in objList)
            {
                if (obj.AddedToScene && !obj.StoryboardIsPlaying && obj.EventDataList.Count > 0)
                {
                    MoveObject(obj.ObjectName, obj.EventDataList[0].Position.X, obj.EventDataList[0].Position.Y, obj.EventDataList[0].Rotation, obj.EventDataList[0].TimeToArrive);
                }
            }
        }

        private void SetMapScale()
        {
            MapScale = new Vector(MaxWidth / MaxX, MaxHeight / MaxY);
        }
    }
}
