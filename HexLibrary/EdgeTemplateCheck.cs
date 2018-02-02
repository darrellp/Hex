using System;
using System.Collections.Generic;

namespace HexLibrary
{
    internal struct EdgeTemplateConnection
    {
        internal int TemplateType;
        internal GridLocation Location;

        internal EdgeTemplateConnection(int templateType, GridLocation location)
        {
            TemplateType = templateType;
            Location = location;
        }
    }

    internal static class EdgeTemplateCheck
    {
        internal static List<EdgeTemplateConnection> Check(Board board, int side)
        {
            var ret = new List<EdgeTemplateConnection>();
            var state = new CheckState(board, side);
            var us = side == 0 || side == 2 ? PlayerColor.Black : PlayerColor.White;

            // One bit for each of the 10 template types - don't indicate template 0 - it's
            // handled specially
            while (!state.Done)
            {
                var stone = state.Stone();

                if (stone == PlayerColor.Unoccupied)
                {
                    state.ProcessUnoccupied(ret);
                    continue;
                }

                if (stone == us)
                {
                    state.ProcessFriendly(ret);
                    continue;
                }

                state.ProcessUnfriendly(ret);
            }

            return ret;
        }
    }
}
