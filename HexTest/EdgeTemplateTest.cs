using Microsoft.VisualStudio.TestTools.UnitTesting;
using HexLibrary;
using Shouldly;

namespace HexTest
{
    [TestClass]
    public class EdgeTemplateTest
    {
        [TestMethod]
        public void TestFit()
        {
            var board = new Board();
            board.ShouldNotBeNull();
            board.PlaceStone(1, 4, PlayerColor.Black);
            board.PlaceStone(3, 3, PlayerColor.White);
            EdgeTemplate.EdgeTemplates[7].Fit(3, 1, board).ShouldBeTrue();
        }
    }
}
