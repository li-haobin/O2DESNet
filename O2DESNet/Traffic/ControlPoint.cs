using System;
using System.Collections.Generic;
using System.Linq;
using O2DESNet.SVGRenderer;
using System.Xml.Linq;

namespace O2DESNet.Traffic
{
    public class ControlPoint : Module<ControlPoint.Statics>
    {
        #region Statics
        public class Statics : Scenario
        {
            public int Index { get; internal set; }
            public string Tag { get; set; }
            public PathMover.Statics PathMover { get; internal set; }
            //internal Statics() { }

            public List<Path.Statics> PathsIn { get; private set; } = new List<Path.Statics>();
            public List<Path.Statics> PathsOut { get; private set; } = new List<Path.Statics>();
            public Dictionary<Statics, Statics> RoutingTable { get; internal set; }
            public Path.Statics PathTo(Statics target)
            {
                if (Equals(target)) return null;
                return PathsOut.Where(p => p.End.Equals(RoutingTable[target])).First();
            }

            #region SVG Output
            /// <summary>
            /// SVG - X coordinate for translation
            /// </summary>
            public double X { get; set; } = 0;
            /// <summary>
            /// SVG - Y coordinate for translation
            /// </summary>
            public double Y { get; set; } = 0;

            public Group SVG()
            {
                string cp_name = "cp#" + PathMover.Index + "_" + Index;
                var path = PathsOut.First();
                string path_name = "path#" + PathMover.Index + "_" + path.Index;
                var label = new Text(LabelStyle, string.Format("CP{0}", Index), new XAttribute("transform", "translate(3 6)"));
                if (path.X != 0 || path.Y != 0 || path.Rotate != 0)
                    return new PathMarker(cp_name, path.X, path.Y, path.Rotate, path_name + "_d", 0, new Use("cross"), label);
                else return new PathMarker(cp_name, path_name + "_d", 0, new Use("cross"), label);
            }

            public static CSS LabelStyle = new CSS("pm_cp_label", new XAttribute("text-anchor", "left"), new XAttribute("font-family", "Verdana"), new XAttribute("font-size", "4px"), new XAttribute("fill", "darkred"));

            /// <summary>
            /// Including arrows, styles
            /// </summary>
            public static Definition SVGDefs
            {
                get
                {
                    return new Definition(
                        new O2DESNet.SVGRenderer.Path("M -2 -2 L 2 2 M -2 2 L 2 -2", "darkred", new XAttribute("id", "cross"), new XAttribute("stroke-width", "0.5")),
                        new Style(LabelStyle)
                        );
                }
            }
            #endregion
        }
        #endregion

        //#region Sub-Components
        ////private Server<TLoad> Server { get; set; }
        //#endregion

        //#region Dynamics
        ////public int Occupancy { get { return Server.Occupancy; } }  
        //#endregion

        //#region Events
        //private abstract class EventOfControlPoint : Event { internal ControlPoint This { get; set; } } // event adapter 

        ////private class InternalEvent : EventOfControlPoint
        ////{
        ////    internal TLoad Load { get; set; }
        ////    public override void Invoke() {  }
        ////}
        //#endregion

        //#region Input Events - Getters
        ////public Event Input(TLoad load) { return new InternalEvent { This = this, Load = load }; }
        //#endregion

        //#region Output Events - Reference to Getters
        ////public List<Func<TLoad, Event>> OnOutput { get; private set; } = new List<Func<TLoad, Event>>();
        //#endregion

        public ControlPoint(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "ControlPoint";
        }

        public override void WarmedUp(DateTime clockTime) { }
    }
}
