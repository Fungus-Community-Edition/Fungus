using System;
using System.Collections.Generic;
using System.Linq;

namespace AtMycelia.Hyphlow
{
    public static class UniqueKeyGenerator
    {
        public static string GetUniqueKeyFor(string suggestedKey, IList<IVariable> varGroup, IVariable ignoreVariable = null)
        {
            string baseKey = suggestedKey;

            // Only letters and digits allowed
            char[] arr = baseKey.Where(c => (char.IsLetterOrDigit(c) || c == '_')).ToArray();
            baseKey = new string(arr);

            // No leading digits allowed
            baseKey = baseKey.TrimStart('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');

            // No empty keys allowed
            if (baseKey.Length == 0)
            {
                baseKey = "Var";
            }

            string resultKey = baseKey;
            int suffix = 0;
            while (true)
            {
                bool collision = false;
                for (int i = 0; i < varGroup.Count; i++)
                {
                    var variable = varGroup[i];
                    if (variable == null || (variable) == ignoreVariable || variable.Key == null)
                    {
                        continue;
                    }
                    if (variable.Key.Equals(resultKey, StringComparison.CurrentCultureIgnoreCase))
                    {
                        collision = true;
                        suffix++;
                        resultKey = baseKey + suffix;
                    }
                }

                if (!collision)
                {
                    return resultKey;
                }
            }
        }
    
    }
}