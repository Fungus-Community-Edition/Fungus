using System.Collections.Generic;

namespace Amanita
{
    public static class StackExtensions 
    {
        public static void PushRange<T>(this Stack<T> stack, IList<T> toPush)
        {
            for (int i = 0; i < toPush.Count; i++)
            {
                stack.Push(toPush[i]);
            }
        }

        public static void PushRange<T>(this Stack<T> stack, IEnumerable<T> toPush)
        {
            foreach (var elem in toPush)
            {
                stack.Push(elem);
            }
        }
    }
}