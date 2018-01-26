using Microsoft.VisualStudio.TestTools.UnitTesting;
using HexLibrary;
using Shouldly;
using static HexTest.Utilities;

namespace HexTest
{
    [TestClass]
    public class ChainTest
    {
        [TestMethod]
        public void TestOneInsert()
        {
            var board = new Board();
            var loc = new GridLocation(6, 6);
            board.PlaceStone(loc, PlayerColor.White);
            var id = board.Analysis.IdAt(loc);
            id.ShouldNotBe(0);
            var chain = board.Analysis.ChainLocations(id);
            chain.Count.ShouldBe(1);
            chain[0].ShouldBe(loc);
        }

        [TestMethod]
        public void TestAmalgamation()
        {
            var board = new Board();
            var loc1 = new GridLocation(6, 6);
            var loc2 = new GridLocation(7, 6);
            var loc3 = new GridLocation(8, 6);

            board.PlaceStone(loc1, PlayerColor.White);
            var id1 = board.Analysis.IdAt(loc1);
            var locs = board.Analysis.ChainLocations(id1);
            locs.Count.ShouldBe(1);
            locs[0].ShouldBe(loc1);

            board.PlaceStone(loc3, PlayerColor.White);
            var id3 = board.Analysis.IdAt(loc3);
            id1.ShouldNotBe(id3);
            locs = board.Analysis.ChainLocations(id3);
            locs.Count.ShouldBe(1);
            locs[0].ShouldBe(loc3);
            board.Analysis.ChainCount(PlayerColor.White).ShouldBe(2);

            board.PlaceStone(loc2, PlayerColor.White);
            var id = board.Analysis.IdAt(loc2);
            locs = board.Analysis.ChainLocations(id);
            locs.Count.ShouldBe(3);
            IsPermutation(locs, ToLocs(6, 6, 7, 6, 8, 6)).ShouldBeTrue();
            board.Analysis.ChainCount(PlayerColor.White).ShouldBe(1);

            board.Undo();
            id3 = board.Analysis.IdAt(loc3);
            locs = board.Analysis.ChainLocations(id3);
            locs.Count.ShouldBe(1);
            locs[0].ShouldBe(loc3);
            board.Analysis.ChainCount(PlayerColor.White).ShouldBe(2);
        }

        [TestMethod]
        public void TestEdgeIds()
        {
            var board = new Board();
            var loc1 = new GridLocation(0, 0);
            var loc2 = new GridLocation(10, 0);
            var loc3 = new GridLocation(10, 10);
            var loc4 = new GridLocation(0, 10);

            board.PlaceStone(loc1, PlayerColor.White);
            board.Analysis.IdAt(loc1).ShouldBe(-4);
            board.RemoveStone(loc1);
            board.Analysis.ChainCount().ShouldBe(0);
            board.PlaceStone(loc1, PlayerColor.Black);
            board.Analysis.IdAt(loc1).ShouldBe(-1);
            board.RemoveStone(loc1);

            board.PlaceStone(loc2, PlayerColor.White);
            board.Analysis.IdAt(loc2).ShouldBe(-2);
            board.RemoveStone(loc2);
            board.Analysis.ChainCount().ShouldBe(0);
            board.PlaceStone(loc2, PlayerColor.Black);
            board.Analysis.IdAt(loc2).ShouldBe(-1);
            board.RemoveStone(loc2);

            board.PlaceStone(loc3, PlayerColor.White);
            board.Analysis.IdAt(loc3).ShouldBe(-2);
            board.RemoveStone(loc3);
            board.Analysis.ChainCount().ShouldBe(0);
            board.PlaceStone(loc3, PlayerColor.Black);
            board.Analysis.IdAt(loc3).ShouldBe(-3);
            board.RemoveStone(loc3);

            board.PlaceStone(loc4, PlayerColor.White);
            board.Analysis.IdAt(loc4).ShouldBe(-4);
            board.RemoveStone(loc4);
            board.Analysis.ChainCount().ShouldBe(0);
            board.PlaceStone(loc4, PlayerColor.Black);
            board.Analysis.IdAt(loc4).ShouldBe(-3);
            board.RemoveStone(loc4);

        }
    }
}
