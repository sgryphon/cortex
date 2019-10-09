﻿using System.Collections.Generic;
using System.Linq;
using Cortex.Containers;
using Cortex.SimpleSerialize;

namespace Cortex.BeaconNode.Ssz
{
    public static class DepositExtensions
    {
        public static SszContainer ToSszContainer(this Deposit item)
        {
            return new SszContainer(GetValues(item));
        }

        public static SszElement ToSszList(this IEnumerable<Deposit> list, int limit)
        {
            return new SszContainer(list.Select(x => x.ToSszContainer()));
        }

        private static IEnumerable<SszElement> GetValues(Deposit item)
        {
            // TODO: fill in
            yield return new SszBasicElement((byte)0);
        }
    }
}
