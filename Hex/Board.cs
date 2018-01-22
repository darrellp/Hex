using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Hex
{
	internal enum Player
	{
		Unoccupied = 0,
		Black,
		White
	}

	internal class Board
	{
        #region Public properties
        internal Player CurPlayer { get; private set; }
	    internal int Size { get; }
        internal Player[,] Players { get; private set; }
        [DefaultValue(Hex.Player.Unoccupied)]
	    internal Player Winner { get; private set; }
	    internal Analysis Analysis { get; }

	    #endregion

        #region Private variables
        private readonly BoardDrawing _boardDrawing;
	    private readonly List<GridLocation> _moves = new List<GridLocation>();
	    #endregion

        #region Constructor
        public Board(int size = 11)
		{
		    if (size > 26)
		    {
		        throw new ArgumentException("Sizes cannot exceed 26");
		    }
			Size = size;

			Players = new Player[Size, Size];
			CurPlayer = Hex.Player.White;
            Analysis = new Analysis(this);
			_boardDrawing = new BoardDrawing(this);
		}
        #endregion

        #region Modifiers
        internal void Clear()
	    {
            Players = new Player[Size, Size];
	        CurPlayer = Hex.Player.White;
	        Winner = Hex.Player.Unoccupied;
	        _boardDrawing.ClearBoard();
            Analysis.Clear();
        }

        internal void Resize()
		{
			_boardDrawing.Redraw();
		}

	    internal void Undo()
	    {
	        if (_moves.Count == 0)
	        {
	            return;
	        }
	        var lastLocation = _moves[_moves.Count - 1];
	        _moves.RemoveAt(_moves.Count - 1);
	        Players[lastLocation.Row, lastLocation.Column] = Hex.Player.Unoccupied;
	        ChangePlayer();
            Analysis.RemoveStone(lastLocation, CurPlayer);
	        Winner = Hex.Player.Unoccupied;
            _boardDrawing.DrawStone(lastLocation, Hex.Player.Unoccupied);
        }

	    public void SetWinner(Player player)
	    {
	        Winner = player;
	    }
#endregion

        #region Event handlers
        internal void Clicked(GridLocation location)
		{
		    if (Winner != Hex.Player.Unoccupied)
		    {
		        // The game is over
		        return;
		    }
			var curState = Players[location.Row, location.Column];
			if (curState != Hex.Player.Unoccupied)
			{
				return;
			}
			Players[location.Row, location.Column] = CurPlayer;
            Analysis.PlaceStone(location, CurPlayer);
            //CheckChainIds(location);
            _moves.Add(location);
			_boardDrawing.DrawStone(location, CurPlayer);
			ChangePlayer();
		}
#endregion

        #region Utilities
        internal Player Player(GridLocation loc)
	    {
	        return Players[loc.Row, loc.Column];
	    }

	    private void ChangePlayer()
	    {
	        CurPlayer = CurPlayer == Hex.Player.Black ? Hex.Player.White : Hex.Player.Black;
	    }

	    private static readonly GridLocation[] Offsets = new[]
	    {
	        new GridLocation( 1, -1),
	        new GridLocation( 1,  0),
	        new GridLocation( 0,  1),
	        new GridLocation(-1,  1),
	        new GridLocation(-1,  0),
	        new GridLocation( 0, -1),
	    };

	    internal IEnumerable<GridLocation> Adjacent(GridLocation loc)
	    {
	        for (var i = 0; i < 6; i++)
	        {
	            var cur = loc + Offsets[i];
	            if (cur.IsValid(Size))
	            {
	                yield return cur;
	            }
	        }
	    }

	    internal bool IsAdjacentTo(GridLocation loc1, GridLocation loc2)
	    {
	        return Offsets.Contains(loc1 - loc2);
	    }
#endregion
	}
}
