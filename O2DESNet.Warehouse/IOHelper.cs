using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using O2DESNet.Warehouse.Statics;

namespace O2DESNet.Warehouse
{
    public static class IOHelper
    {
        private static string inputFolder = @"Inputs\";
        private static string inputFile = "_InputParams";
        private static string outputFolder = @"Outputs\";
        private static string outputFile = "_Output_";
        private static string orderCountFile = "_OrderCount_";
        private static string csv = ".csv";
        /// <summary>
        /// Flag to determine if input has been read
        /// </summary>
        public static bool isInputRead = false;

        public static int ItemTotesCapacity; // item-based
        public static int OrderTotesCapacity; // order-based

        public static int MasterBatchSize; // Release Batch every X orders

        public static int MaxOrderBatchSize;
        public static int SortingRate; // seconds per item
        public static int NumSorters; // number of simultaneous sorters

        private static List<string> outputCSV;

        // Debug: count the number of orders:
        public static Dictionary<PicklistGenerator.Strategy, List<int>> OrderCount;

        /// <summary>
        /// Converts csv file with header into list (row) of string array (column)
        /// </summary>
        /// <param name="csvfile"></param>
        /// <returns></returns>
        public static List<string[]> CSVToList(string csvfile)
        {
            List<string[]> output = new List<string[]>();
            string line;

            using (StreamReader sr = new StreamReader(csvfile))
            {
                sr.ReadLine(); // Skip header
                while ((line = sr.ReadLine()) != null)
                {
                    output.Add(line.Split(','));
                }
            }

            return output;
        }

        public static int GetNumRuns(string scenarioName)
        {
            var inFilename = inputFolder + scenarioName + inputFile + csv;
            int count;
            using (StreamReader sr = new StreamReader(inFilename))
            {
                count = sr.ReadLine().Split(',').Count(); // Read header and count number of columns
            }
            return count - 2; // 2 header columns
        }

        /// <summary>
        /// Read input parameters. Called by WarehouseSim constructor.
        /// </summary>
        /// <param name="scenarioName"></param>
        /// <param name="runID"></param>
        public static void ReadInputParams(string scenarioName, int runID)
        {
            int col = runID + 1; // Start from [2]
            var inFilename = inputFolder + scenarioName + inputFile + csv;

            var data = CSVToList(inFilename);

            ItemTotesCapacity = int.Parse(data[0][col]);
            OrderTotesCapacity = int.Parse(data[1][col]);
            MasterBatchSize = int.Parse(data[2][col]);
            MaxOrderBatchSize = int.Parse(data[3][col]);
            SortingRate = int.Parse(data[4][col]);
            NumSorters = int.Parse(data[5][col]);

            isInputRead = true;
        }

        /// <summary>
        /// One file per RunID, 4 strategies per RunID.
        /// </summary>
        /// <param name="whsim"></param>
        public static void AddOutputFile(WarehouseSim whsim)
        {
            if (outputCSV == null) outputCSV = new List<string>(whsim.GetOutputHeaders());

            var data = whsim.GetOutputStatistics();
            for (int i = 0; i < data.Count; i++) // for each line
            {
                outputCSV[i] = outputCSV[i] + "," + data[i];
            }

        }

        public static void WriteOutputFile(WarehouseSim whsim)
        {
            ResolveNumOrderError(); // HACK: because of the counting error...

            string scenarioName = whsim.sim.Scenario.Name;
            int runID = whsim.RunID;
            string filename = OutputFileName(scenarioName, runID);

            File.WriteAllLines(filename, outputCSV.ToArray());
            outputCSV = null; // clear
        }

        public static void WriteNumOrderFile(WarehouseSim whsim)
        {
            string scenarioName = whsim.sim.Scenario.Name;
            int runID = whsim.RunID;
            string filename = OrderCountFilename(scenarioName, runID);

            List<string> output = new List<string>();

            foreach (var strategy in OrderCount.Keys)
            {
                output.Add(strategy.ToString() + "," + string.Join(",", OrderCount[strategy].ToArray()));
            }

            File.WriteAllLines(filename, output.ToArray());
            OrderCount = null; // clear
        }

        /// <summary>
        /// Deletes all output fies
        /// </summary>
        /// <param name="whsim"></param>
        public static void ClearOutputFiles(string scenarioName)
        {
            int idx = 1;


            while (File.Exists(OutputFileName(scenarioName, idx)))
            {
                File.Delete(OutputFileName(scenarioName, idx++));
            }
        }

        private static string OutputFileName(string scenarioName, int id)
        {
            return outputFolder + scenarioName + outputFile + id.ToString() + csv;
        }

        private static string OrderCountFilename(string scenarioName, int id)
        {
            return outputFolder + scenarioName + orderCountFile + id.ToString() + csv;
        }

        // HACK: Resolve counting error here...
        private static void ResolveNumOrderError()
        {
            var numLines = outputCSV.Count;
            var withSorting = outputCSV[numLines - 2].Split(',');
            var noSorting = outputCSV[numLines-1].Split(',');

            var totalOrders = int.Parse(noSorting[2]); // Correct
            var wrongTotal = int.Parse(noSorting[3]) + int.Parse(withSorting[3]); // Wrong, over-count
            var correction = wrongTotal - totalOrders;

            // Correct with sorting
            List<string> lineWithSorting = new List<string>();
            lineWithSorting.Add(withSorting[0]); // header
            lineWithSorting.Add(withSorting[1]); // sorting A = 0
            lineWithSorting.Add(withSorting[2]); // sorting B = 0
            lineWithSorting.Add((int.Parse(withSorting[3]) - correction).ToString()); // Sorting C, -correction
            lineWithSorting.Add((int.Parse(withSorting[4]) - correction).ToString()); // Sorting D, -correction

            outputCSV[numLines - 2] = string.Join(",", lineWithSorting.ToArray());

            // Correct no sorting
            List<string> lineNoSorting = new List<string>();
            lineNoSorting.Add(noSorting[0]); // header
            lineNoSorting.Add(noSorting[2]); // no sorting A = no sorting B
            lineNoSorting.Add(noSorting[2]); // no sorting B
            lineNoSorting.Add(noSorting[3]); // no sorting C
            lineNoSorting.Add(noSorting[4]); // no sorting D

            outputCSV[numLines - 1] = string.Join(",", lineNoSorting.ToArray());
        }
    }
}
