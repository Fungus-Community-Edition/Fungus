using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Amanita
{
    public static class UniqueIdGenerator
    {
        public static int GetUniqueIdFor(IHasItemID toGetFor, IList<IHasItemID> othersToConsider, int highestIDSoFar)
        {
            int result = highestIDSoFar;
            if (othersToConsider.Count > 0)
            {
                IHasItemID hasHighestAmongOthers = othersToConsider.OrderBy((elem) => elem.ItemId).LastOrDefault();
                int highestAmongOthers = Mathf.Max(hasHighestAmongOthers.ItemId, highestIDSoFar, toGetFor.ItemId);
                result = highestAmongOthers;
            }
            return result;
        }
    }
}