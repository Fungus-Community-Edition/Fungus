using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Amanita
{
    public static class UniqueIDGenerator
    {
        public static int GetUniqueIDFor(IHasItemID toGetFor, IList<IHasItemID> othersToConsider, int highestIDSoFar)
        {
            int result = highestIDSoFar;
            if (othersToConsider.Count > 0)
            {
                IHasItemID hasHighestAmongOthers = othersToConsider.OrderBy((elem) => elem.ItemID).LastOrDefault();
                int highestAmongOthers = Mathf.Max(hasHighestAmongOthers.ItemID, highestIDSoFar, toGetFor.ItemID);
                result = highestAmongOthers;
            }
            return result;
        }
    }
}