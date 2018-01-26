namespace HexLibrary
{
    public struct BoardChange
    {
        // Location
        public GridLocation Location;
        // true => place stone, false => remove stone
        public bool IsPlacement;
        // Player color before change
        public PlayerColor OldPlayer;
        // Color of stone - only if !isPlacement
        public PlayerColor StoneColor;

        public bool IsNull()
        {
            return Location.IsNowhere;
        }

        internal static BoardChange NullChange()
        {
            var ret = new BoardChange {Location = GridLocation.Nowhere()};
            return ret;
        }

        internal BoardChange(
            GridLocation location,
            PlayerColor oldPlayer,
            bool isPlacement = true,
            PlayerColor stoneColor = PlayerColor.Unoccupied)
        {
            Location = location;
            OldPlayer = oldPlayer;
            IsPlacement = isPlacement;
            StoneColor = stoneColor;
        }

        internal void Undo(Board board)
        {
            if (IsPlacement)
            {
                board.RemoveStone(Location, false);
            }
            else
            {
                board.PlaceStone(Location, StoneColor, false);
            }

            board.ChangePlayer(OldPlayer);
        }
    }
}
