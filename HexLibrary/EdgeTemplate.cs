namespace HexLibrary
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   An edge template. </summary>
    ///
    /// <remarks>   This is a work in progress.  Depth is the depth from the edge of the connecting
    ///             stone.  The edge template with the stone on the edge has depth 0.  We specify the
    ///             shape of the template by giving the width of each "row" as we move back from the
    ///             edge and it's left offset from the next nearer row.  This is our general way of
    ///             addressing locations within the template with a GridLocation.  Row is the distance
    ///             back from the edge and Column is the distance from the left side of the edge.
    ///             So, for instance, the template at IVc on p. 72 would have a depth of 4.  The
    ///             ply widths, starting from the edge are {8, 7, 6, 3}, the offsets are {0, 0, 0, 1},
    ///             the connecting piece is at (3, 1) and the don't care position is at (1, 3).
    ///             Darrell Plank, 1/26/2018. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    class EdgeTemplate
    {
        #region Private variables

        internal int[] ColumnHeights { get; }
        internal int ConnectStoneColumn { get; }
        internal GridLocation DontCare { get; }

        #endregion

        #region Templates
        internal static EdgeTemplate[] EdgeTemplates =
        {
            // Single piece on the edge
            new EdgeTemplate(
                new[] {1},
                0),

            // One row off the edge
            new EdgeTemplate(
                new[] {2, 1},
                0),

            // Three rows
            // IIIA
            new EdgeTemplate(
                new[] {3, 3, 2, 1},
                0),

            // IIIB
            new EdgeTemplate(
                new[] {3, 3, 2, 1},
                1),

            // IIIC
            new EdgeTemplate(
                new[] {3, 3, 3, 2, 1},
                1, new GridLocation(0, 2)),

            // Four Rows
            // IVA
            new EdgeTemplate(
                new[] {3, 4, 4, 3, 3, 2, 1},
                1),

            // IVB
            new EdgeTemplate(
                new[] {3, 4, 4, 3, 3, 2, 1},
                2),

            // IVC
            new EdgeTemplate(
                new[] {3, 4, 4, 4, 3, 3, 2, 1},
                2, new GridLocation(1, 3)),

            // Five Rows
            // VA
            new EdgeTemplate(
                new[] {3, 5, 5, 5, 4, 4, 3, 3, 2, 1},
                2),

            // VB
            new EdgeTemplate(
                new[] {3, 4, 5, 5, 5, 4, 3, 3, 2, 1},
                3),
        };
        #endregion

        public EdgeTemplate(int[] columnHeights, int connectStoneColumn, GridLocation dontCare)
        {
            ColumnHeights = columnHeights;
            ConnectStoneColumn = connectStoneColumn;
            DontCare = dontCare;
        }

        public EdgeTemplate(int[] columnHeights, int connectStoneColumn) : this(columnHeights, connectStoneColumn,
            GridLocation.Nowhere())
        {
        }
    }

    class EdgeTemplateOld
    {
        #region Private variables

        internal int Depth { get; }
        internal int[] PlyWidths { get; }
        internal int[] PlyOffsets { get; }
        internal int ConnectStoneOffset { get; }
        internal GridLocation DontCare { get; }

        #endregion

        #region Templates
        internal static EdgeTemplateOld[] EdgeTemplatesOld =
        {
            // Single piece on the edge
            new EdgeTemplateOld(1, 
                new [] {1},
                new [] {0},
                0),

            // One row off the edge
            new EdgeTemplateOld(2,
                new [] {2, 1},
                new [] {0, 0},
                0),

            // Three rows
            // IIIA
            new EdgeTemplateOld(3,
                new [] {4, 3, 2},
                new [] {0, 0, 0},
                0),

            // IIIA
            new EdgeTemplateOld(3,
                new [] {4, 3, 2},
                new [] {0, 0, 0},
                1),

            // IIIC
            new EdgeTemplateOld(3,
                new [] {5, 4, 3},
                new [] {0, 0, 0},
                1, new GridLocation(0, 2)),

            // Four Rows
            // IVA
            new EdgeTemplateOld(4,
                new [] {7, 6, 5, 2},
                new [] {0, 0, 0, 1},
                0),

            // IVB
            new EdgeTemplateOld(4,
                new [] {7, 6, 5, 2},
                new [] {0, 0, 0, 1},
                1),

            // IVC
            new EdgeTemplateOld(4,
                new [] {8, 7, 6, 3},
                new [] {0, 0, 0, 1},
                1, new GridLocation(1, 3)),

            // Five Rows
            // VA
            new EdgeTemplateOld(5,
                new [] {10, 9, 8, 5, 3},
                new [] {0, 0, 1, 0},
                1),
        
            // VB
            new EdgeTemplateOld(5,
                new [] {10, 9, 8, 5, 3},
                new [] {0, 0, 1, 1},
                1),
        };
        #endregion

        #region Constructor
        public EdgeTemplateOld(int depth, int[] plyWidths, int[] plyOffsets, int connectStoneOffset, GridLocation dontCare)
        {
            Depth = depth;
            PlyWidths = plyWidths;
            PlyOffsets = plyOffsets;
            ConnectStoneOffset = connectStoneOffset;
            DontCare = dontCare;
        }

        public EdgeTemplateOld(int depth, int[] plyWidths, int[] plyOffsets, int connectStoneOffset) :
            this(depth, plyWidths, plyOffsets, connectStoneOffset, GridLocation.Nowhere())
        { }
        #endregion

        #region Fitting

        static readonly GridLocation[] SideStarts = new[]
        {
            new GridLocation(0, 0),
            new GridLocation(1, 0),
            new GridLocation(0, 1),
            new GridLocation(0, 0),
        };

        private static readonly GridLocation[] ColIncs = new[]
        {
            new GridLocation(1, 0),
            new GridLocation(0, 1),
            new GridLocation(1, 0),
            new GridLocation(0, 1),
        };

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        ///  <summary>   Fits. </summary>
        /// 
        ///  <remarks>   The sides are numbered as follows for the "side" parameter
        ///              Darrell Plank, 1/26/2018. </remarks>
        /// 
        ///  <param name="side">     The side to fit on. </param>
        ///  <param name="offset">   The offset on that side. </param>
        /// <param name="board">     The board we're fitting on. </param>
        /// <returns>   True if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal bool Fit(int side, int offset, Board board)
        {
            var colInc = ColIncs[side];
            var start = (board.Size - 1) * SideStarts[side] + offset * colInc;
            var rowInc = (side == 2 || side == 3 ? -1 : 1) * (new GridLocation(1, 1) - colInc);
            var player = side == 0 || side == 2 ? PlayerColor.Black : PlayerColor.White;

            for (var iRow = 0; iRow < Depth; iRow++)
            {
                start += PlyOffsets[iRow] * colInc;
                var cur = start;
                for (var iCol = 0; iCol < PlyWidths[iRow]; iCol++)
                {
                    if (new GridLocation(iRow, iCol) != DontCare)
                    {
						var expected = iRow == Depth - 1 && iCol == ConnectStoneOffset
							? player
							: PlayerColor.Unoccupied;
						if (board[cur] != expected)
						{
							return false;
						}
                    }
                    cur += colInc;
                }
                start += rowInc;
            }

            return true;
        }
        #endregion
    }
}
