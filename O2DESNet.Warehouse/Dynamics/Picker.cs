using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Statics;

namespace O2DESNet.Warehouse.Dynamics
{
    [Serializable]
    public class Picker
    {
        public ControlPoint CurLocation { get; set; }
        public PickerType Type { get; private set; }
        public List<PickJob> Picklist { get; set; }
        public List<PickJob> PickListToComplete { get; set; }
        public List<PickJob> CompletedJobs { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsIdle { get; set; }

        public Picker(PickerType type)
        {
            CurLocation = null;
            Type = type;
            PickListToComplete = new List<PickJob>();
            CompletedJobs = new List<PickJob>();
        }

        // All time in seconds

        // HACK: Exploits basic wareouse structure
        /// <summary>
        /// Exploits Basic Warehouse structure.
        /// </summary>
        /// <param name="scenario"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public TimeSpan GetTravelTime_old(Scenario scenario, ControlPoint destination) // Called by Event
        {
            double moveSpeed = Type.AveMoveSpeed;
            double dist = 0;

            if (CurLocation == scenario.StartCP || destination == scenario.StartCP)
            {
                // Involving StartCP
                if (destination == scenario.StartCP)
                {
                    PathRow fromRow = (PathRow)CurLocation.Positions.Keys.ToList().Where(path => path is PathRow).ToList().First();
                    PathAisle fromAisle = fromRow.AisleIn;

                    PathAisle mainAisle = scenario.Aisles["MAIN"];

                    dist = CurLocation.Positions[fromRow] // shelf to aisle
                    + fromRow.ControlPoints.First().Positions[fromAisle] // aisle to main aisle
                    + fromAisle.ControlPoints.First().Positions[mainAisle]; // main aisle dist to StartCP
                }
                else
                {
                    PathAisle mainAisle = scenario.Aisles["MAIN"];

                    PathRow destRow = (PathRow)destination.Positions.Keys.ToList().Where(path => path is PathRow).ToList().First();
                    PathAisle destAisle = destRow.AisleIn;

                    dist = destAisle.ControlPoints.First().Positions[mainAisle] // main aisle dist to aisle
                    + destRow.ControlPoints.First().Positions[destAisle] // main aisle to aisle
                    + destination.Positions[destRow]; // aisle to shelf
                }
            }
            else
            {
                // CP are always BaseCP of Shelf (on Row)
                // Then Row CP[0] is the CP connecting Row to Aisle
                // Then Aisle CP[0] connects to MainAisle find the Dest.

                PathRow fromRow = (PathRow)CurLocation.Positions.Keys.ToList().Where(path => path is PathRow).ToList().First();
                PathAisle fromAisle = fromRow.AisleIn;

                PathAisle mainAisle = scenario.Aisles["MAIN"];

                PathRow destRow = (PathRow)destination.Positions.Keys.ToList().Where(path => path is PathRow).ToList().First();
                PathAisle destAisle = destRow.AisleIn;

                dist = CurLocation.Positions[fromRow] // shelf to aisle
                    + fromRow.ControlPoints.First().Positions[fromAisle] // aisle to main aisle
                    + mainAisle.GetDistanceOnPath(fromAisle.ControlPoints.First(), destAisle.ControlPoints.First())// main aisle distance
                    + destRow.ControlPoints.First().Positions[destAisle] // main aisle to aisle
                    + destination.Positions[destRow]; // aisle to shelf

                // In metres. From shelf baseCP to shelf baseCP.
            }
            return TimeSpan.FromSeconds(dist / moveSpeed);
        }

        /// <summary>
        /// Exploit Warehouse Structure. Row connects to one aisle only. Routing done within aisle.
        /// </summary>
        /// <param name="scenario"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public TimeSpan GetTravelTime(Scenario scenario, ControlPoint destination)
        {
            double moveSpeed = Type.AveMoveSpeed;
            double dist = 0;

            if (CurLocation == scenario.StartCP || destination == scenario.StartCP)
            {
                // StartCP to Shelf
                if (CurLocation == scenario.StartCP)
                {
                    PathRow destRow = (PathRow)destination.Positions.Keys.ToList().Where(path => path is PathRow).ToList().First();

                    dist = CurLocation.GetDistanceTo(destRow.BaseCP) // StartCP to Row
                        + destination.Positions[destRow]; // Row to Shelf
                }
                // Shelf to StartCP
                else
                {
                    PathRow fromRow = (PathRow)CurLocation.Positions.Keys.ToList().Where(path => path is PathRow).ToList().First();

                    dist = CurLocation.Positions[fromRow] // Shelf to Row
                        + fromRow.BaseCP.GetDistanceTo(destination); // Row to StartCP
                }
            }
            // Shelf to Shelf
            else
            {
                // If same Shelf
                if (CurLocation == destination)
                    dist = 0;
                else
                {
                    PathRow fromRow = (PathRow)CurLocation.Positions.Keys.ToList().Where(path => path is PathRow).ToList().First();

                    // If same Row
                    if (fromRow.ControlPoints.Contains(destination))
                    {
                        dist = Math.Abs(CurLocation.Positions[fromRow] - destination.Positions[fromRow]);
                    }

                    // To another row
                    else
                    {
                        PathRow destRow = (PathRow)destination.Positions.Keys.ToList().Where(path => path is PathRow).ToList().First();

                        dist = CurLocation.Positions[fromRow] // Shelf to Row
                         + fromRow.BaseCP.GetDistanceTo(destRow.BaseCP) // Row to Row
                         + destination.Positions[destRow]; // Row to Shelf
                    }
                }
            }

            return TimeSpan.FromSeconds(dist / moveSpeed);
        }

        /// <summary>
        /// Generic. Does not exploit any warehouse structure.
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        public TimeSpan GetTravelTime(ControlPoint destination)
        {
            return TimeSpan.FromSeconds(Type.GetNextTravelTime(CurLocation, destination));
        }

        public TimeSpan GetPickingTime()
        {
            return Type.GetNextPickingTime();
        }
        public void PickItem()
        {
            var pickJob = PickListToComplete.First();
            if (CurLocation != pickJob.rack.OnShelf.BaseCP) throw new Exception("ERROR! Wrong location, halt pick");

            pickJob.item.PickFromRack(pickJob.rack, pickJob.quantity);

            CompletedJobs.Add(pickJob);

            PickListToComplete.RemoveAt(0);
        }

        public int GetNumCompletedPickJobs()
        {
            return CompletedJobs.Count;
        }
        public TimeSpan GetTimeToCompletePickList()
        {
            if (EndTime < StartTime) throw new Exception("Error: EndTime < StartTime");

            return EndTime - StartTime;
        }
    }
}
