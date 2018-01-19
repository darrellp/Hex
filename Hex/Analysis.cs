using System;
using System.Collections.Generic;
using System.Linq;

namespace Hex
{
    internal class Analysis
    {
        #region Private variables
        private readonly Board _board;
        internal int[,] ChainIds { get; private set; }
		private readonly Dictionary<int, List<GridLocation>> _mapChainIdToLocations =
            new Dictionary<int, List<GridLocation>>();
        #endregion

        #region Properties
        private int Size => _board.Size;
        #endregion

        #region Constructor
        internal Analysis(Board board)
        {
            _board = board;
            ChainIds = new int[Size, Size];
        }
        #endregion

        #region Internal functions
        internal void Clear()
        {
            ChainIds = new int[Size, Size];
	        _mapChainIdToLocations.Clear();
            _nextChainId = 0;
        }

        internal void PlaceStone(GridLocation loc, Player player)
        {
            CheckChainIds(loc, player);
        }

        internal void RemoveStone(GridLocation loc, Player player)
        {
            CheckIdsOnRemoval(loc, player);
        }

        internal int ChainCount(Player player)
        {
            return player == Player.Unoccupied ?
                _mapChainIdToLocations.Count :
                _mapChainIdToLocations.Count(entry => _board.Player(entry.Value[0]) == player);
        }
        #endregion

        #region Utilities
        internal Player PlayerAtLoc(GridLocation loc)
        {
            return _board.Players[loc.Row, loc.Column];
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
        #endregion

        #region Chain IDs
        // Chain IDs are just successive integers except for ones meeting winning edges which are
        // negative with the high bits being 11000, 10100, 10010, 10001 starting from upper left and
        // going clockwise respectively.  These IDs will only be applied for chains of the proper
        // color - i.e., a white stone on a black side will receive a normal chain ID and vice
        // versa.  These upper bits will be paired with a normal lower set of bits - i.e., the
        // next successive chain ID.  This is to distinguish between different chains both
        // adjacent to a winning edge.
        // 
        // High Bits/Sides:
        //           /\
        //  10001/0 /  \ 10010/1
        //         /    \
        //         \    /
        //  11000/3 \  / 10100/2
        //           \/
        private int _nextChainId = 1;

        private int NextId()
        {
            return _nextChainId++;
        }

        private int ChainId(GridLocation loc)
        {
            return ChainIds[loc.Row, loc.Column];
        }

        internal bool IsEdgeChain(int id)
        {
            return id < 0;
        }

        internal int LowBitsFromId(int id)
        {
            return id & 0x07ffffff;
        }

        internal int SideFromId(int id)
        {
            switch (HighBitsFromId(id))
            {
                case 1:
                    return 0;
                case 2:
                    return 1;
                case 4:
                    return 2;
                default:
                    return 3;
            }
        }

        // This includes the sign bit
        internal int IdFromSide(int side)
        {
            return ((1 << side) | 0b10000) << 27;
        }

        // This doesn't include the sign bit
        internal int HighBitsFromId(int id)
        {
            return (id >> 27) & 0b1111;
        }

        private string IdToString(int id)
        {
            return !IsEdgeChain(id) ? id.ToString() : $"{LowBitsFromId(id)}:S{SideFromId(id)}";
        }

		// TODO: Can I do all the _mapChainIdToLocations setting here?
		private void SetChainId(GridLocation loc, int id)
		{
            Console.WriteLine($"ChainId at {loc} set to {IdToString(id)}");
			ChainIds[loc.Row, loc.Column] = id;
        }

        private int HighBitsFromLocation(GridLocation loc)
        {
            // See what the high bits are required to be for our location
            var side = -1;

            if (PlayerAtLoc(loc) == Player.Black)
            {
                if (loc.Column == 0)
                {
                    side = 0;
                }
                else if (loc.Column == Size - 1)
                {
                    side = 2;
                }
            }
            else
            {
                if (loc.Row == 0)
                {
                    side = 3;
                }
                else if (loc.Row == Size - 1)
                {
                    side = 1;
                }
            }
            return side < 0 ? 0 : IdFromSide(side);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        ///  <summary>   Check chain identifiers after a single change to the board </summary>
        /// 
        ///  <remarks>   TODO: Check on Win conditions
        ///              I'm not sure what to do with win conditions so that they'll undo correctly.
        ///              Undo probably needs to check if we're in a winning condition and take appropriate
        ///              action. The problem is that there's no really good way of making proper chain
        ///              IDs on a win since we have two different ids from two different sides meeting and
        ///              therefore not being promulgated into each other.  Probably, the undo can simply
        ///              recognize the win, reset winner, remove the piece without trying to do any fixups
        ///              on chain IDs.
        ///              
        ///              Darrell Plank, 1/17/2018. </remarks>
        /// 
        ///  <param name="loc">     The location where the stone was placed. </param>
        ///  <param name="player">  Player that placed the stone. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal void CheckChainIds(GridLocation loc, Player player)
        {
            if (player == Player.Unoccupied)
            {
                throw new ArgumentException("Checking chain ids on unoccupied cell");
            }

            // See what the high bits are required to be for our location
            var highBitsWithSign = HighBitsFromLocation(loc);
            var connections = Adjacent(loc).Where(l => PlayerAtLoc(l) == player).ToArray();

            if (connections.Length == 0)
            {
                // We're not connected to anything so start our own chain ID...
                SetChainId(loc, highBitsWithSign + NextId());
				_mapChainIdToLocations[ChainId(loc)] = new List<GridLocation> {loc};
                return;
            }

            // We are actually connected to like colors
            var ids = connections.Select(c => ChainIds[c.Row, c.Column]).ToArray();

            if (ids.Skip(1).All(id => id == ids[0]))
            {
                // All our neighbors have the same ID
                var commonId = ids[0];

                if (highBitsWithSign == 0)
                {
                    // We're not connected to an edge so just take on the common ID of our neighbors
                    SetChainId(loc, commonId);
					// Add ourselves to the list for this chainId
	                _mapChainIdToLocations[commonId].Add(loc);
                    return;
                }

                // At this point we're edge connected and we're connected to like colored
                // neighbors with the same ID.  Three possibilities exist:
                // 
                // 1. They have no highbits
                //      Give our highbits to all members of the chain and take their low chain ID
                //      
                // 2. They have the same highbits as us
                //      Just record our chain ID as the same as theirs
                // 
                // 3. They have different highbits from us
                //      There are only two sets of highbits our color can possess so we have just
                //      won the game!
                if (HighBitsFromId(commonId) == 0)
                {
                    // Case 1
                    SetChainId(loc, commonId + highBitsWithSign);
                    PromulgateId(loc);
                    return;
                }
                if (HighBitsFromId(commonId) == highBitsWithSign)
                {
                    // Case 2
                    SetChainId(loc, commonId);
	                _mapChainIdToLocations[commonId].Add(loc);
                    return;
                }
                // Case 3: We won!
                _board.SetWinner(_board.CurPlayer);
                return;
            }

            // Okay - we have more than one ID adjacent to us.  The cases are:
            //
            // 1. All our neighbors are either unconnected or have the same connection as us.
            //      Promulgate our newly created connection to all our neighbors.  Note that
            //      if we are unconnected then all our neighbors must be unconnected in this case.
            // 
            // 2. There is at least one neighbor with a different connection than us or a neighbor
            //      We've won!
            // 
            // 3. We're unconnected and at least one neighbor has a connection
            //      We take that neighbor's ID and promulgate it among our other neighbors
            if (ids.All(id =>
            {
                var highBits = HighBitsFromId(id);
                return highBits == 0 || highBits == highBitsWithSign;
            }))
            {
                // Case 1
                SetChainId(loc, highBitsWithSign + NextId());
                PromulgateId(loc);
                return;
            }
            var highBitCollection = new HashSet<int>(ids.Where(id => !IsEdgeChain(id)).Select(HighBitsFromId).ToArray());
            if (highBitsWithSign < 0)
            {
                highBitCollection.Add(highBitsWithSign);
            }
            if (highBitCollection.Count > 1)
            {
                // Case 2
                _board.SetWinner(_board.CurPlayer);
                return;
            }
            // Case 3
            SetChainId(loc, ids.First(IsEdgeChain));
            PromulgateId(loc);
        }

        private void PromulgateId(GridLocation loc)
        {
            var player = PlayerAtLoc(loc);
            var id = ChainId(loc);

            var ourLocList = _mapChainIdToLocations[id] = new List<GridLocation>();
            var queue = new Queue<GridLocation>();
            queue.Enqueue(loc);

			// We delete all adjacent id lists here since we only have to perform the deletion at the starting
			// location
	        foreach (var adjLoc in Adjacent(loc).Where(l => PlayerAtLoc(l) == player && ChainId(l) != id))
	        {
		        _mapChainIdToLocations.Remove(ChainId(adjLoc));
	        }

            while (queue.Count != 0)
            {
                // Queued locations have the proper chain id.  We just have to promulgate them to neighbors.
                var curLoc = queue.Dequeue();
				ourLocList.Add(curLoc);
				

                // For every like colored neighbor which has a different chain id from ours
                foreach (var neighbor in Adjacent(curLoc).Where(l => PlayerAtLoc(l) == player && ChainId(l) != id))
                {
                    // Set the proper chain id and queue him up for promulgation
                    SetChainId(neighbor, id);
                    queue.Enqueue(neighbor);
                }
            }
        }

        private void CheckIdsOnRemoval(GridLocation loc, Player player)
        {
            var oldId = ChainId(loc);
			// We're gonna totally lose the old ID
	        _mapChainIdToLocations.Remove(oldId);
            SetChainId(loc, 0);
            var checks = Adjacent(loc).Where(l => PlayerAtLoc(l) == player).ToArray();

            GridLocation[] nextLocations;
            while ((nextLocations = checks.Where(l => ChainId(l) == oldId).ToArray()).Length > 0)
            {
                var nextLocation = nextLocations[0];
	            var id = NextId();
                SetChainId(nextLocation, id);
				_mapChainIdToLocations[id] = new List<GridLocation>();
                PromulgateIdAfterDelete(nextLocation);
            }
        }

        private void PromulgateIdAfterDelete(GridLocation loc)
        {
            var player = PlayerAtLoc(loc);
            var id = ChainId(loc);
            var queue = new Queue<GridLocation>();
	        var ourLocList = _mapChainIdToLocations[id];
            queue.Enqueue(loc);
            var chainLocations = new List<GridLocation>();

            while (queue.Count != 0)
            {
                // Queued locations have the proper chain id.  We just have to promulgate them to neighbors.
                var curLoc = queue.Dequeue();
	            ourLocList.Add(curLoc);

                // For every like colored neighbor which has a different chain id from ours
                foreach (var neighbor in Adjacent(curLoc).Where(l => PlayerAtLoc(l) == player && ChainId(l) != id))
                {
                    // When promulgating for deletions we can run into a situation where the current ID is
                    // unconnected but we find out during promulgation that the chain is, in fact, connected.
                    // When this happens we have to back up, reset all the previous ID's to the connected ID
                    // and continue on from there.  This check is the primary difference between PromulgateId()
                    // and ProulgateIdAfterDelete().  Check for that case here.
                    if (!IsEdgeChain(id) && HighBitsFromLocation(neighbor) != 0)
                    {
						// TODO: Can I use chainLocations here?  Can I just set _mapChainIdToLocations using it when it's all done?
	                    _mapChainIdToLocations.Remove(id);
                        id |= HighBitsFromLocation(neighbor);
	                    _mapChainIdToLocations[id] = ourLocList;
                        foreach (var prevLocation in chainLocations)
                        {
                            SetChainId(prevLocation, id);
                        }
                    }
                    // Set the proper chain id and queue him up for promulgation
                    SetChainId(neighbor, id);
                    chainLocations.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
        #endregion
    }
}
