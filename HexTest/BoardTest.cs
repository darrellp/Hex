﻿using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HexLibrary;
using Shouldly;
using static HexTest.Utilities;

namespace HexTest
{
    [TestClass]
    public class BoardTest
    {
        [TestMethod]
        public void TestConstructor()
        {
            var board = new Board();
            board.ShouldNotBeNull();
        }

        [TestMethod]
        public void TestAdjacent()
        {
            var board = new Board();
            // ReSharper disable InconsistentNaming
            var adj0_0 = board.Adjacent(new GridLocation(0, 0)).ToArray();
            adj0_0.Length.ShouldBe(2);
            IsPermutation(adj0_0, ToLocs(0, 1, 1, 0)).ShouldBeTrue();
            var adj1_1 = board.Adjacent(new GridLocation(1, 1)).ToArray();
            IsPermutation(adj1_1, ToLocs(1, 0, 0, 1, 2, 0, 0, 2, 2, 1, 1, 2)).ShouldBeTrue();
            var adj10_10 = board.Adjacent(new GridLocation(10, 10)).ToArray();
            IsPermutation(adj10_10, ToLocs(10, 9, 9, 10)).ShouldBeTrue();
            // ReSharper restore InconsistentNaming
        }

        [TestMethod]
        public void TestBridgeTo()
        {
            var board = new Board();
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
            IsPermutation(bridge5_5, ToLocs(7, 4, 6, 6, 4, 7, 3, 6, 4, 4, 6, 3)).ShouldBeTrue();

			board.PlaceStone(new GridLocation(6, 5), PlayerColor.Black);
	        bridge5_5 = board.BridgedTo(loc).ToArray();
            IsPermutation(bridge5_5, ToLocs(4, 7, 3, 6, 4, 4, 6, 3)).ShouldBeTrue();

            // ReSharper restore InconsistentNaming
        }
    }
}
