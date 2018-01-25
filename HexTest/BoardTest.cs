using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HexLibrary;
using Shouldly;

namespace HexTest
{
    [TestClass]
    public class BoardTest
    {
	    readonly BoardDrawingTest _bdt = new BoardDrawingTest();

        [TestMethod]
        public void TestConstructor()
        {
            var board = new Board(_bdt);
            board.ShouldNotBeNull();
        }

        [TestMethod]
        public void TestAdjacent()
        {
            var board = new Board(_bdt);
            // ReSharper disable InconsistentNaming
            var adj0_0 = board.Adjacent(new GridLocation(0, 0)).ToArray();
            adj0_0.Length.ShouldBe(2);
            adj0_0.Except(ToLocs(0, 1, 1, 0)).ShouldBeEmpty();
            var adj1_1 = board.Adjacent(new GridLocation(1, 1)).ToArray();
            adj1_1.Except(ToLocs(1, 0, 0, 1, 2, 0, 0, 2, 2, 1, 1, 2)).ShouldBeEmpty();
            var adj10_10 = board.Adjacent(new GridLocation(10, 10)).ToArray();
            adj10_10.Except(ToLocs(10, 9, 9, 10)).ShouldBeEmpty();
            // ReSharper restore InconsistentNaming
        }

        [TestMethod]
        public void TestBridgeTo()
        {
            var board = new Board(_bdt);
            var loc = new GridLocation(0, 0);
            // ReSharper disable InconsistentNaming
            var bridge0_0 = board.BridgedTo(loc).ToArray();
            bridge0_0.Length.ShouldBe(0);
            board.PlaceStone(loc, PlayerColor.White);
            bridge0_0 = board.BridgedTo(loc).ToArray();
            bridge0_0.Length.ShouldBe(1);
            bridge0_0[0].ShouldBe(new GridLocation(1, 1));

			loc = new GridLocation(5, 5);
	        board.PlaceStone(loc, PlayerColor.White);
	        var bridge5_5 = board.BridgedTo(loc).ToArray();
	        bridge5_5.Length.ShouldBe(6);
			bridge5_5.Except(ToLocs(7,4,6,6,4,7,3,6,4,4,6,3)).ShouldBeEmpty();

			board.PlaceStone(new GridLocation(6, 5), PlayerColor.Black);
	        bridge5_5 = board.BridgedTo(loc).ToArray();
	        bridge5_5.Length.ShouldBe(4);
	        bridge5_5.Except(ToLocs(4, 7, 3, 6, 4, 4, 6, 3)).ShouldBeEmpty();
	        // ReSharper restore InconsistentNaming
		}

		private static IEnumerable<GridLocation> ToLocs(params int[] crds)
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
