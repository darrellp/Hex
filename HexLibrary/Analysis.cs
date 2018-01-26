using System;
using System.Collections.Generic;
using System.Linq;

namespace HexLibrary
{
    public class Analysis
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

        internal Analysis(Analysis parent)
        {
            _board = parent._board;
            ChainIds = new int[Size, Size];
            Array.Copy(parent.ChainIds, ChainIds, Size * Size);
        }
        #endregion

        #region Internal functions
        internal void Clear()
        {
            ChainIds = new int[Size, Size];
            _mapChainIdToLocations.Clear();
            _nextChainId = 0;
        }

        internal void PlaceStone(GridLocation loc, PlayerColor player)
        {
            CheckChainIds(loc, player);
        }

        internal void RemoveStone(GridLocation loc, PlayerColor player)
        {
            CheckIdsOnRemoval(loc, player);
        }

        public int ChainCount(PlayerColor player)
        {
            return player == PlayerColor.Unoccupied ?
                _mapChainIdToLocations.Count :
                _mapChainIdToLocations.Count(entry => _board.Player(entry.Value[0]) == player);
        }
        #endregion

        #region Queries
        internal List<GridLocation> GroupLocations(int id)
        {
            return _mapChainIdToLocations[id];
        }

        internal int IdAt(GridLocation loc)
        {
            return ChainIds[loc.Row, loc.Column];
        }
        #endregion

        #region Utilities
        internal PlayerColor PlayerAtLoc(GridLocation loc)
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
            return ++_nextChainId;
        }

        private int ChainId(GridLocation loc)
        {
            return ChainIds[loc.Row, loc.Column];
        }

        internal bool IsEdgeChain(int id)
        {
            return id < 0;
        }

        // TODO: Can I do all the _mapChainIdToLocations setting here?
        private void SetChainId(GridLocation loc, int id)
        {
            Console.WriteLine($"ChainId at {loc} set to {id}");
            ChainIds[loc.Row, loc.Column] = id;
        }

        private int EdgeGroup(GridLocation loc, PlayerColor player)
        {
            var side = NextId();

            if (player == PlayerColor.Black)
            {
                if (loc.Column == 0)
                {
                    side = -1;
                }
                else if (loc.Column == Size - 1)
                {
                    side = -3;
                }
            }
            else
            {
                if (loc.Row == 0)
                {
                    side = -4;
                }
                else if (loc.Row == Size - 1)
                {
                    side = -2;
                }
            }
            return side;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        ///  <summary>   Check chain identifiers after a single change to the board </summary>
        /// 
        ///  <remarks>   Darrell Plank, 1/17/2018. </remarks>
        /// 
        ///  <param name="loc">     The location where the stone was placed. </param>
        ///  <param name="player">  Player that placed the stone. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal void CheckChainIds(GridLocation loc, PlayerColor player)
        {
            if (player == PlayerColor.Unoccupied)
            {
                throw new ArgumentException("Checking chain ids on unoccupied cell");
            }

            // Get a new potential Group ID (or the proper edge ID if we're connected to an edge)
            var newGroupId = EdgeGroup(loc, PlayerAtLoc(loc));
            var connections = Adjacent(loc).Where(l => PlayerAtLoc(l) == player).ToArray();
            var ids = connections.Select(c => ChainIds[c.Row, c.Column]).ToArray();

            // Okay - we have more than one ID adjacent to us.  We look at all the neighboring
            // group IDs along with our own.  If we find two different negative values, we've
            // won.  Otherwise, we take the smallest of the groupcodes ourselves and propogate
            // that to all our neighbors.  This means edge codes, which are negative, will get
            // preferentially promulgated.  Otherwise, we'll just use the smallest non-edge code.
            // TODO: minID had ought to take the place of foundNegId below.
            // After all, if minID is negative then it's the only negative value we've found
            // 
            var foundNegId = newGroupId;        // NonNegative => no negative ID located yet
            var minId = newGroupId;

            foreach (var id in ids)
            {
                if (id < 0)
                {
                    if (foundNegId < 0 && foundNegId != id)
                    {
                        // We won!
                        _board.SetWinner(_board.CurPlayer);
                        return;
                    }
                    foundNegId = id;
                }
                if (id < minId)
                {
                    minId = id;
                }
            }
            SetChainId(loc, minId);
            PromulgateId(loc);
        }

        // TODO: Mull over whether we can use disjoint set forest here for promulgation
        // TODO: Mull over whether we ought to use linked lists rather than normal ones for ID lists
        // https://en.wikipedia.org/wiki/Disjoint-set_data_structure
        // I think maybe the disjoint forest thing saves time by not visiting all the
        // children but we have to visit all the children if we want to update the
        // ChainId map so maybe it isn't even applicable.  Mull on this.  I think at
        // best it might save some time by eliminating the AddRange call in PromulgateId.
        // That might actually be sped up by using linked lists rather than plain lists.
        
        private void PromulgateId(GridLocation loc)
        {
            var player = PlayerAtLoc(loc);
            var id = ChainId(loc);

            // Get all the Id's we're gonna eliminate
            var eliminatedIds = new HashSet<int>(
                Adjacent(loc).
                Where(l => PlayerAtLoc(l) == player && ChainId(l) != id).
                Select(ChainId));

            var ourLocList = _mapChainIdToLocations.ContainsKey(id) ? _mapChainIdToLocations[id] : new List<GridLocation>();
            _mapChainIdToLocations[id] = ourLocList;

            // Add ourselves
            ourLocList.Add(loc);

            // For each ID we eliminate...
            foreach (var idElim in eliminatedIds)
            {
                // For all stones in the group
                foreach (var alterLocation in _mapChainIdToLocations[idElim])
                {
                    // Set the value in the chainID map to the new ID
                    SetChainId(alterLocation, id);
                }
                // Put all the elements in the old list in our new list
                ourLocList.AddRange(_mapChainIdToLocations[idElim]);
                // Drop the old list
                _mapChainIdToLocations.Remove(idElim);
            }
        }

        private void CheckIdsOnRemoval(GridLocation loc, PlayerColor player)
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
                    // and continue on from there.  Check for that case here.
                    if (!IsEdgeChain(id) && EdgeGroup(neighbor, PlayerAtLoc(neighbor)) != 0)
                    {
                        // TODO: Can I use chainLocations here?  Can I just set _mapChainIdToLocations using it when it's all done?
                        _mapChainIdToLocations.Remove(id);
                        id |= EdgeGroup(neighbor, PlayerAtLoc(neighbor));
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

        #region Path determination
        // Based on chapter 8 of "Hex Strategy: Making the Right Connections" by Cameron Browne
        private void DeterminePaths()
        {
            if (_board.IsWon)
            {
                return;
            }
            // Make a copy of ourselves to do the analysis in so we don't screw up our chain IDs
            var childAnalysisWhite = new Analysis(this);
            var childAnalysisBlack = new Analysis(this);
            childAnalysisWhite.FindPath(PlayerColor.White);
            childAnalysisBlack.FindPath(PlayerColor.Black);
        }

        private void FindPath(PlayerColor player)
        {
            // We maintained the singleton groups during piece placement so the "GenerateChains" and
            // "SingletonGroups" on p. 128 are already done.
            while (!_board.IsWon)
            {
                TakeAStep();
            }
        }

        private void TakeAStep()
        {

        }

        void TakeInitialSteps(int groupId)
        {

        }
        #endregion
    }
}
