using System.Collections.Generic;
using System.Linq;
using HexLibrary;

namespace HexTest
{
    static class Utilities
    {

        internal static bool IsPermutation<T>(ICollection<T> l1, ICollection<T> l2)
        {
            return l1.Count == l2.Count && !l1.Except(l2).Any();
        }

        internal static GridLocation[] ToLocs(params int[] crds)
        {
            var size = crds.Length / 2;
            var ret = new GridLocation[size];

            for (var iloc = 0; iloc < size; iloc++)
            {
                ret[iloc] = new GridLocation(crds[2 * iloc], crds[2 * iloc + 1]);
            }

            return ret;
        }
    }
}
