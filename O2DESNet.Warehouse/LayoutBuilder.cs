using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Statics;

namespace O2DESNet.Warehouse
{
    public static class LayoutBuilder
    {
        private static double interRowSpace = 1.7;
        private static double shelfWidth = 1.8;
        private static double aisleWidth = 1.5;
        private static double rackHeight = 0.35;
        private static int numRack = 9;

        private static Scenario wh;

        public static void ZABuilderEila(Scenario scenario)
        {
            wh = scenario;

            #region Aisles
            var aisleMain = wh.CreateAisle("aisleMain", interRowSpace * 99 + aisleWidth * 3);
            var aisleEF = wh.CreateAisle("aisleEF", aisleMain.Length);
            var aisleAB = wh.CreateAisle("aisleAB", interRowSpace * 72 + aisleWidth * 2);
            var aisleCD = wh.CreateAisle("aisleCD", aisleMain.Length + aisleAB.Length - aisleWidth);
            var aisleDSpecial = wh.CreateAisle("aisleDSpecial", interRowSpace * 20 + aisleWidth);
            var aisleDConnect = wh.CreateAisle("aisleDConnect", aisleAB.Length);
            var aisleY = wh.CreateAisle("aisleY", interRowSpace * 8 + aisleWidth);
            var aisleZ = wh.CreateAisle("aisleZ", interRowSpace * 11 + aisleWidth);
            var aisleV1 = wh.CreateAisle("aisleV1", shelfWidth * (6 + 4 + 6 + 7 + 12 + 3) + aisleWidth * 6);
            var aisleV2 = wh.CreateAisle("aisleV2", aisleV1.Length);
            var aisleV3 = wh.CreateAisle("aisleV3", aisleV1.Length + shelfWidth * 4 + aisleWidth);
            var aisleV4 = wh.CreateAisle("aisleV4", aisleV3.Length - (shelfWidth * 10 + aisleWidth * 2));

            double positionCD = shelfWidth * (7 + 12 + 3) + aisleWidth * 2;
            double positionV2 = interRowSpace * 42 + aisleWidth;

            wh.Connect(aisleV1, aisleMain, 0, 0);
            wh.Connect(aisleV1, aisleCD, positionCD, 0);
            wh.Connect(aisleV1, aisleEF, aisleV1.Length, 0);

            wh.Connect(aisleV2, aisleMain, 0, positionV2);
            wh.Connect(aisleV2, aisleCD, positionCD, positionV2);
            wh.Connect(aisleV2, aisleEF, aisleV2.Length, positionV2);
            wh.Connect(aisleV2, aisleY, shelfWidth * 8 + aisleWidth, 0);
            wh.StartCP = aisleV2.ControlPoints[0];

            wh.Connect(aisleV3, aisleAB, 0, 0);
            wh.Connect(aisleV3, aisleMain, shelfWidth * 4 + aisleWidth, aisleMain.Length);
            wh.Connect(aisleV3, aisleZ, shelfWidth * (7 + 12) + aisleWidth, 0);
            wh.Connect(aisleV3, aisleCD, shelfWidth * (7 + 12 + 7) + aisleWidth * 2, aisleMain.Length);
            wh.Connect(aisleV3, aisleDConnect, aisleV4.Length, 0);
            wh.Connect(aisleV3, aisleDSpecial, aisleV4.Length + shelfWidth * 4, 0);
            wh.Connect(aisleV3, aisleEF, aisleV3.Length, aisleEF.Length);


            wh.Connect(aisleV4, aisleAB, 0, aisleAB.Length);
            wh.Connect(aisleV4, aisleCD, shelfWidth * (7 + 12 + 7) + aisleWidth * 2, aisleCD.Length);
            wh.Connect(aisleV4, aisleDConnect, aisleV4.Length, aisleDConnect.Length);
            #endregion

            #region Rows
            CreateRowBlock(aisleCD, 0, "C", 1, 41, 11); // Given numshelf = 7
            CreateRowBlock(aisleCD, 0, "D", 1, 42, 10); // Given numshelf = 6
            CreateRowBlock(aisleCD, positionV2, "C", 42, 99, 11); // Given numshelf = 7
            CreateRowBlock(aisleCD, positionV2, "D", 43, 98, 10); // Given numshelf = 6
            CreateRowBlock(aisleCD, aisleMain.Length, "C", 100, 156, 11); // Given numshelf = 7
            CreateRowBlock(aisleCD, aisleMain.Length, "D", 101, 156, 10); // Given numshelf = 6

            CreateRowBlock(aisleEF, 0, "F", 1, 82, 8); // Given numshelf = 6
            CreateRowBlock(aisleEF, 0, "E", 1, 42, 6);
            CreateRowBlock(aisleEF, positionV2, "E", 43, 99, 6);

            //CreateRowBlock(aisleDSpecial, 0, "D", 79, 98, 4); // Given, duplicated!
            CreateRowBlock(aisleDSpecial, 0, "D", 99, 100, 10); // Given numshelf = 4
            CreateRowBlock(aisleY, 0, "Y", 1, 8, 9); // Given numshelf = 8
            CreateRowBlock(aisleZ, 0, "Z", 1, 11, 12);

            CreateRowBlock(aisleAB, 0, "B", 87, 158, 14); // Given numshelf = 7
            CreateRowBlock(aisleAB, 0, "A", 87, 158, 14); // Given numshelf = 6
            #endregion
        }

        private static void CreateRowBlock(PathAisle onAisle, double pathPosition, string zone, int startRowNum, int endRowNum, int numShelves)
        {
            // Row on Aisle
            for (int i = startRowNum; i <= endRowNum; i++)
            {
                double pos = interRowSpace * (i - startRowNum + 1);
                double rowLength = shelfWidth * numShelves;
                string rowID = zone + "-" + i.ToString();

                var row = wh.CreateRow(rowID, rowLength, onAisle, pathPosition + pos);

                // Shelves on Row
                for (int j = 1; j <= numShelves; j++)
                {
                    string shelfID = rowID + "-" + j.ToString();
                    double shelfHeight = rackHeight * numRack;

                    var shelf = wh.CreateShelf(shelfID, shelfHeight, row, j * shelfWidth);

                    // Racks on Shelf
                    for (int k = 1; k <= numRack; k++)
                    {
                        string rackID = shelfID + "-" + k.ToString();

                        wh.CreateRack(rackID, shelf, k * rackHeight);
                    }
                }
            }
        }
    }
}
