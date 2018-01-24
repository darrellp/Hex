namespace HexLibrary
{
    public struct GridLocation
    {
        public int Row { get; }
        public int Column { get; }
        public bool IsNowhere => Row < 0;

        public static GridLocation Nowhere()
        {
            return new GridLocation(-1, -1);
        }

        public GridLocation(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public override string ToString()
        {
            return $"{(char)('A' + Row)}{Column + 1}";
        }

        public static GridLocation operator +(GridLocation loc1, GridLocation loc2)
        {
            return new GridLocation(loc1.Row + loc2.Row, loc1.Column + loc2.Column);
        }

        public static GridLocation operator -(GridLocation loc1, GridLocation loc2)
        {
            return new GridLocation(loc1.Row - loc2.Row, loc1.Column - loc2.Column);
        }

        internal bool IsValid(int size)
        {
            return Row >= 0 && Column >= 0 && Row < size && Column < size;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Returns the hash code for this location. </summary>
        ///
        /// <remarks>   We're going to be hashing locations a lot so we want this to be efficient and to
        ///             yield different values for different locations.
        ///             Darrell Plank, 1/17/2018. </remarks>
        ///
        /// <returns>   A 32-bit signed integer that is the hash code for this instance. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public override int GetHashCode()
        {
            return Row + (Column << 5);
        }
    }
}
