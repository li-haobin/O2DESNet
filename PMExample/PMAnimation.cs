using O2DESNet.PathMover;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PMExample
{
    public partial class PMAnimation : Form
    {
        private Simulator _sim;
        private DrawingParams _drawingParams;
        private DateTime _clockTime;

        public PMAnimation()
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); //双缓冲
        }

        private void DrawPMScenario(object sender, PaintEventArgs e)
        {
            _sim.Scenario.PM.Draw(e.Graphics, _drawingParams);
        }

        private void DrawPMStatus(object sender, PaintEventArgs e)
        {
            _sim.Status.GridStatus.Draw(e.Graphics, _drawingParams);
            _sim.Status.GridStatus.Changed = false;
        }

        private void PMAnimation_Load(object sender, EventArgs e)
        {
            var scenario = new Scenario(Enumerable.Repeat(200d, 4).ToArray(), new double[] { 90 }.Concat(Enumerable.Repeat(50d, 3)).ToArray(), 10, numVehicles: 10);
            _sim = new Simulator(new Status(scenario, 0));
            _clockTime = DateTime.MinValue;
            
            _drawingParams = new DrawingParams(this.Width, this.Height);
            this.label1.Text = _clockTime.ToLongTimeString();
        }
        
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.label1.Text = _clockTime.ToLongTimeString();
            double speed = 10;
            _clockTime += TimeSpan.FromSeconds(timer1.Interval * speed / 1000);
            this.SuspendLayout();
            _sim.Run(_clockTime);
            if (_sim.Status.GridStatus.Changed)
            {
                this.BackgroundImage = _sim.Status.GridStatus.DrawToImage(_drawingParams);
                this.Width = _drawingParams.Width + 20;
                this.Height = _drawingParams.Height + 55;
            }
            this.ResumeLayout();
        }
    }
}
