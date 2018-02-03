using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace HexLibrary
{
    public enum PlayerColor
    {
        Unoccupied = 0,
        Black,
        White
    }

    public class Board
    {
        #region Public properties
        public PlayerColor CurPlayer { get; private set; }
        public int Size { get; }
        public PlayerColor[,] Players { get; private set; }
        [DefaultValue(PlayerColor.Unoccupied)]
        internal PlayerColor Winner { get; private set; }
        public Analysis Analysis { get; }
        public bool IsWon => Winner != PlayerColor.Unoccupied;
        #endregion

        #region Private variables
        private readonly List<BoardChange> _moves = new List<BoardChange>();
        #endregion

        #region Constructor
        public Board(int size = 11)
        {
            if (size > 26)
            {
                throw new ArgumentException("Sizes cannot exceed 26");
            }
            Size = size;

            Players = new PlayerColor[Size, Size];
            CurPlayer = PlayerColor.White;
            Analysis = new Analysis(this);
        }
        #endregion

        #region Modifiers

        public void Clear()
        {
            Players = new PlayerColor[Size, Size];
            CurPlayer = PlayerColor.White;
            Winner = PlayerColor.Unoccupied;
            Analysis.Clear();
        }

        public BoardChange Undo()
        {
            if (_moves.Count == 0)
            {
                return BoardChange.NullChange();
            }
            var change = _moves[_moves.Count - 1];
            _moves.RemoveAt(_moves.Count - 1);

            change.Undo(this);
            Winner = PlayerColor.Unoccupied;
            return change;
        }

        public void RemoveStone(GridLocation loc, bool undoable = true)
        {
            if (undoable)
            {
                _moves.Add(new BoardChange(loc, CurPlayer, false, this[loc]));
            }
            this[loc] = PlayerColor.Unoccupied;
            Analysis.RemoveStone(loc, CurPlayer);
        }

        public void SetWinner(PlayerColor player)
        {
            Winner = player;
        }
        #endregion

        #region Event handlers

        internal void PlaceStone(int row, int col, PlayerColor player, bool undoable = true)
        {
            PlaceStone(new GridLocation(row, col), player, undoable);
        }

        internal void PlaceStone(GridLocation loc, PlayerColor player, bool undoable = true)
        {
            if (Winner != PlayerColor.Unoccupied)
            {
                // The game is over
                return;
            }
            var curState = this[loc];
            if (curState != PlayerColor.Unoccupied)
            {
                return;
            }
            this[loc] = player;
            Analysis.PlaceStone(loc, player);
            if (undoable)
            {
                _moves.Add(new BoardChange(loc, CurPlayer));
            }
        }

        public void Clicked(GridLocation loc)
        {
            PlaceStone(loc, CurPlayer);
            ChangePlayer();
        }
        #endregion

        #region Utilities

        internal PlayerColor this[int row, int col]
        {
            get => Players[row, col];
        }

        internal PlayerColor this[GridLocation l]
        {
            get => Players[l.Row, l.Column];
            private set => Players[l.Row, l.Column] = value;

        }

        internal void ChangePlayer(PlayerColor to = PlayerColor.Unoccupied)
        {
            if (to == PlayerColor.Unoccupied)
            {
                to = CurPlayer == PlayerColor.Black ? PlayerColor.White : PlayerColor.Black;
            }
            CurPlayer = to;
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

        // Relative locations of bridges and their supports
        private static readonly BridgeInfo[] BridgeOffsets = new BridgeInfo[]
        {
	        // Starting from upper right and proceeding clockwise
	        new BridgeInfo(2, -1, 1, -1, 1, 0),
            new BridgeInfo(1, 1, 1, 0, 0, 1),
            new BridgeInfo(-1, 2, 0, 1, -1, 1),
            new BridgeInfo(-2, 1, -1, 1, -1, 0),
            new BridgeInfo(-1, -1, -1, 0, 0, -1),
            new BridgeInfo(1, -2, 0, -1, 1, -1)
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

        internal IEnumerable<GridLocation> BridgedTo(GridLocation loc, bool includeFriendlyOccupied = true)
        {
            if (!loc.IsValid(Size))
            {
                yield break;
            }

            var player = this[loc];
            if (player == PlayerColor.Unoccupied)
            {
                yield break;
            }
            for (var iBridge = 0; iBridge < BridgeOffsets.Length; iBridge++)
            {
                if (CheckBridge(loc, player, iBridge, includeFriendlyOccupied))
                {
                    yield return loc + BridgeOffsets[iBridge].Location;
                }
            }
        }

        private bool CheckBridge(GridLocation loc, PlayerColor player, int iBridge, bool includeFriendlyOccupied)
        {
            return loc.IsValid(Size) && CheckBridge(loc, player, BridgeOffsets[iBridge], includeFriendlyOccupied);
        }

        private bool CheckBridge(GridLocation loc, PlayerColor player, BridgeInfo bridgeOffset, bool includeFriendlyOccupied)
        {
            var to = loc + bridgeOffset.Location;
            if (!to.IsValid(Size) || !loc.IsValid(Size))
            {
                return false;
            }

            var playerAtLoc = this[loc + bridgeOffset.Location];

            return (playerAtLoc == PlayerColor.Unoccupied || includeFriendlyOccupied && player == playerAtLoc) &&
                   this[loc + bridgeOffset.Support1] == PlayerColor.Unoccupied &&
                   this[loc + bridgeOffset.Support2] == PlayerColor.Unoccupied;

        }

        internal bool IsAdjacentTo(GridLocation loc1, GridLocation loc2)
        {
            return Offsets.Contains(loc1 - loc2);
        }

        private struct BridgeInfo
        {
            public readonly GridLocation Location;
            public readonly GridLocation Support1;
            public readonly GridLocation Support2;

            public BridgeInfo(int locRow, int locCol, int support1Row, int support1Col, int support2Row, int support2Col)
            {
                Location = new GridLocation(locRow, locCol);
                Support1 = new GridLocation(support1Row, support1Col);
                Support2 = new GridLocation(support2Row, support2Col);
            }
        }
        #endregion
    }
}
