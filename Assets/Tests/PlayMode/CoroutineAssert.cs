using System;
using System.Collections;
using NUnit.Framework;

public static class CoroutineAssert
{
    public static IEnumerator Throws<T>(Func<IEnumerator> enumeratorDelegate,
        string errorMessage = "") where T : Exception
    {
        if (string.IsNullOrEmpty(errorMessage))
        {
            errorMessage = $"Expected exception of type {typeof(T)} to be thrown.";
        }

        bool exceptionThrown = false;
        IEnumerator enumerator = enumeratorDelegate();
        while (true)
        {
            try
            {
                if (!enumerator.MoveNext())
                    break;
            }
            catch (T)
            {
                exceptionThrown = true;
                break;
            }
            // yield the current value so Unity can update the coroutine state
            yield return enumerator.Current;
        }

        Assert.IsTrue(exceptionThrown, errorMessage);
    }
}