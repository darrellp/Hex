using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HexLibrary;
using Shouldly;

namespace HexTest
{
    [TestClass]
    public class BoardTest
    {
        BoardDrawingTest bdt = new BoardDrawingTest();

        [TestMethod]
        public void TestConstructor()
        {
            var board = new Board(bdt);
            board.ShouldNotBeNull();
        }

        [TestMethod]
        public void TestAdjacent()
        {
            var board = new Board(bdt);
            // ReSharper disable InconsistentNaming
            var adj0_0 = board.Adjacent(new GridLocation(0, 0)).ToArray();
            adj0_0.Length.ShouldBe(2);
            adj0_0.Except(ToLocs(0, 1, 1, 0)).ShouldBeEmpty();
            var adj1_1 = board.Adjacent(new GridLocation(1, 1)).ToArray();
            adj1_1.Except(ToLocs(1, 0, 0, 1, 2, 0, 0, 2, 2, 1, 1, 2));
            var adj10_10 = board.Adjacent(new GridLocation(10, 10)).ToArray();
            adj10_10.Except(ToLocs(10, 9, 9, 10)).ShouldBeEmpty();
            // ReSharper restore InconsistentNaming
        }

        [TestMethod]
        public void TestBridgeTo()
        {
            var board = new Board(bdt);
            // ReSharper disable InconsistentNaming
            var bridge0_0 = board.BridgedTo(new GridLocation(0, 0)).ToArray();
            bridge0_0.Length.ShouldBe(0);
            //bridge0_0.Except(ToLocs(1, 1)).ShouldBeEmpty();
            board.PlaceStone(new GridLocation(0,0), PlayerColor.White);
            bridge0_0 = board.BridgedTo(new GridLocation(0, 0)).ToArray();
            bridge0_0.Length.ShouldBe(0);
            bridge0_0[0].ShouldBe(new GridLocation(1, 1));
            // ReSharper restore InconsistentNaming
        }

        private static GridLocation[] ToLocs(params int[] crds)
        {
            var size = crds.Length / 2;
            var ret = new GridLocation[size];

            for (var iloc = 0; iloc < size; iloc++)
            {
                ret[iloc] = new GridLocation(crds[2 * iloc], crds[2 * iloc + 1]);
            }

            return ret;
        }
    }
}
