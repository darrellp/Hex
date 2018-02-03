using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HexLibrary;
using Shouldly;
using static HexTest.Utilities;
// ReSharper disable InconsistentNaming


namespace HexTest
{
    [TestClass]
    public class EdgeTemplateTest
    {
        private readonly Board board = new Board();

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
			IsPermutation(templates.Select(v => v.TemplateType).ToArray(), new[] { CheckState.IIIa, CheckState.IIIb }).ShouldBeTrue();

			// Placing a black at (0, 2) should force us to recognize a IIIc
			board.PlaceStone(0, 2, PlayerColor.Black);
			templates = EdgeTemplateCheck.Check(board, 3);
			templates.Count.ShouldBe(1);
			templates[0].Location.ShouldBe(new GridLocation(2, 1));
			templates[0].TemplateType.ShouldBe(CheckState.IIIc);

			// Placing a black one two to the right should still be okay
			board.PlaceStone(2, 3, PlayerColor.Black);
		    templates = EdgeTemplateCheck.Check(board, 3);
		    templates.Count.ShouldBe(1);
		    templates[0].Location.ShouldBe(new GridLocation(2, 1));
		    templates[0].TemplateType.ShouldBe(CheckState.IIIc);

			// But putting it one away should eliminate it
		    board.PlaceStone(2, 2, PlayerColor.Black);
		    templates = EdgeTemplateCheck.Check(board, 3);
		    templates.Count.ShouldBe(0);
	    }

	    [TestMethod]
	    public void TestTypeIV()
	    {
		    board.PlaceStone(3, 2, PlayerColor.White);
		    var templates = EdgeTemplateCheck.Check(board, 3);
		    templates.Count.ShouldBe(2);
		    IsPermutation(templates.Select(v => v.Location).ToArray(), ToLocs(3, 2, 3, 2)).ShouldBeTrue();
		    IsPermutation(templates.Select(v => v.TemplateType).ToArray(), new[] { CheckState.IVa, CheckState.IVb }).ShouldBeTrue();

		    // Placing a black at (1, 3) should force us to recognize a IVc
		    board.PlaceStone(1, 3, PlayerColor.Black);
		    templates = EdgeTemplateCheck.Check(board, 3);
		    templates.Count.ShouldBe(1);
		    templates[0].Location.ShouldBe(new GridLocation(3, 2));
		    templates[0].TemplateType.ShouldBe(CheckState.IVc);

		    // Placing a black one two to the right should still be okay
		    board.PlaceStone(3, 4, PlayerColor.Black);
		    templates = EdgeTemplateCheck.Check(board, 3);
		    templates.Count.ShouldBe(1);
		    templates[0].Location.ShouldBe(new GridLocation(3, 2));
		    templates[0].TemplateType.ShouldBe(CheckState.IVc);

		    // But putting it one away should eliminate it
		    board.PlaceStone(3, 3, PlayerColor.Black);
		    templates = EdgeTemplateCheck.Check(board, 3);
		    templates.Count.ShouldBe(0);
	    }

	    [TestMethod]
	    public void TestTypeV()
	    {
		    board.PlaceStone(4, 3, PlayerColor.White);
		    var templates = EdgeTemplateCheck.Check(board, 3);
		    templates.Count.ShouldBe(2);
		    IsPermutation(templates.Select(v => v.Location).ToArray(), ToLocs(4, 3, 4, 3)).ShouldBeTrue();
		    IsPermutation(templates.Select(v => v.TemplateType).ToArray(), new[] { CheckState.Va, CheckState.Vb }).ShouldBeTrue();

			// Placing a black one two to the right should still be okay
			board.PlaceStone(4, 5, PlayerColor.Black);
			templates = EdgeTemplateCheck.Check(board, 3);
		    templates.Count.ShouldBe(2);
		    IsPermutation(templates.Select(v => v.Location).ToArray(), ToLocs(4, 3, 4, 3)).ShouldBeTrue();
		    IsPermutation(templates.Select(v => v.TemplateType).ToArray(), new[] { CheckState.Va, CheckState.Vb }).ShouldBeTrue();

			// But putting it one away should eliminate it
			board.PlaceStone(4, 4, PlayerColor.Black);
			templates = EdgeTemplateCheck.Check(board, 3);
			templates.Count.ShouldBe(0);
		}

	}
}
