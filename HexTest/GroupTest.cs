﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using HexLibrary;
using Shouldly;
using static HexTest.Utilities;

namespace HexTest
{
    [TestClass]
    public class GroupTest
    {
        [TestMethod]
        public void TestOneInsert()
        {
            var board = new Board();
            var loc = new GridLocation(6, 6);
            board.PlaceStone(loc, PlayerColor.White);
            var id = board.Analysis.IdAt(loc);
            id.ShouldNotBe(0);
            var group = board.Analysis.GroupLocations(id);
            group.Count.ShouldBe(1);
            group[0].ShouldBe(loc);
        }

        [TestMethod]
        public void TestEdgeAmalgamation()
        {
            var board = new Board();
            var loc1 = new GridLocation(6, 6);
            var loc2 = new GridLocation(7, 6);
            var loc3 = new GridLocation(8, 6);

            board.PlaceStone(loc1, PlayerColor.White);
            var id1 = board.Analysis.IdAt(loc1);
            var locs = board.Analysis.GroupLocations(id1);
            locs.Count.ShouldBe(1);
            locs[0].ShouldBe(loc1);

            board.PlaceStone(loc3, PlayerColor.White);
            var id3 = board.Analysis.IdAt(loc3);
            id1.ShouldNotBe(id3);
            locs = board.Analysis.GroupLocations(id3);
            locs.Count.ShouldBe(1);
            locs[0].ShouldBe(loc3);
            board.Analysis.ChainCount(PlayerColor.White).ShouldBe(2);

            board.PlaceStone(loc2, PlayerColor.White);
            var id = board.Analysis.IdAt(loc2);
            locs = board.Analysis.GroupLocations(id);
            locs.Count.ShouldBe(3);
            IsPermutation(locs, ToLocs(6, 6, 7, 6, 8, 6)).ShouldBeTrue();
            board.Analysis.ChainCount(PlayerColor.White).ShouldBe(1);

            board.Undo();
            id3 = board.Analysis.IdAt(loc3);
            locs = board.Analysis.GroupLocations(id3);
            locs.Count.ShouldBe(1);
            locs[0].ShouldBe(loc3);
            board.Analysis.ChainCount(PlayerColor.White).ShouldBe(2);
        }
    }
}
