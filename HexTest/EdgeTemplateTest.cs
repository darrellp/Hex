using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HexLibrary;
using Shouldly;
using static HexTest.Utilities;


namespace HexTest
{
    [TestClass]
    public class EdgeTemplateTest
    {
        private Board board = new Board();

        [TestMethod]
        public void TestFit()
        {
            board.PlaceStone(1, 4, PlayerColor.Black);
            board.PlaceStone(3, 3, PlayerColor.White); 
            EdgeTemplateOld.EdgeTemplatesOld[7].Fit(3, 1, board).ShouldBeTrue();
	        board.Clear();
	        board.PlaceStone(4, 1, PlayerColor.White);
	        board.PlaceStone(3, 3, PlayerColor.Black);
	        EdgeTemplateOld.EdgeTemplatesOld[7].Fit(0, 1, board).ShouldBeTrue();
        }

        [TestMethod]
        public void TestEmptyCheck()
        {
            EdgeTemplateCheck.Check(board, 0).Count.ShouldBe(0);
            EdgeTemplateCheck.Check(board, 1).Count.ShouldBe(0);
            EdgeTemplateCheck.Check(board, 2).Count.ShouldBe(0);
            EdgeTemplateCheck.Check(board, 3).Count.ShouldBe(0);
        }

	    [TestMethod]
	    public void TestTypeI()
	    {
		    board.PlaceStone(0, 0, PlayerColor.White);
		    board.PlaceStone(0, 3, PlayerColor.White);

		    var templates = EdgeTemplateCheck.Check(board, 3);
			templates.Count.ShouldBe(2);
		    IsPermutation(templates.Select(v => v.Location).ToArray(), ToLocs(0, 0, 0, 3)).ShouldBeTrue();
			templates.All(v => v.TemplateType == 0).ShouldBeTrue();
	    }

	    [TestMethod]
	    public void TestTypeII()
	    {
		    board.PlaceStone(1, 0, PlayerColor.White);
		    var templates = EdgeTemplateCheck.Check(board, 3);
		    templates.Count.ShouldBe(1);
		    templates[0].Location.ShouldBe(new GridLocation(1, 0));
			templates[0].TemplateType.ShouldBe(1);

			// Placing another beneath this one should ruin it
			board.PlaceStone(0, 1, PlayerColor.White);
		    templates = EdgeTemplateCheck.Check(board, 3);
		    templates.Count.ShouldBe(1);
		    templates[0].Location.ShouldBe(new GridLocation(0, 1));
		    templates[0].TemplateType.ShouldBe(0);
		}

	    [TestMethod]
	    public void TestTypeIII()
	    {
		    board.PlaceStone(2, 1, PlayerColor.White);
		    var templates = EdgeTemplateCheck.Check(board, 3);
		    templates.Count.ShouldBe(2);
		    IsPermutation(templates.Select(v => v.Location).ToArray(), ToLocs(2, 1, 2, 1)).ShouldBeTrue();
	    }
	}
}
