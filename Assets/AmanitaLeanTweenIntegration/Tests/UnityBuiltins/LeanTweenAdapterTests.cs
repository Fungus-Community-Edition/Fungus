using NUnit.Framework;
using UnityEngine;
using UnityObj = UnityEngine.Object;
using Amanita.LeanTweenIntegration;


public class LeanTweenAdapterTests
{
    protected const float Duration = 0.5f;
    protected const float Epsilon = 0.01f;

    protected GameObject _testGo;
    protected AmaniLeanTweenAdapter _adapter;

    [SetUp]
    public virtual void SetUp()
    {
        _testGo = new GameObject("LT_TestGO");
        _adapter = ScriptableObject.CreateInstance<AmaniLeanTweenAdapter>();
    }

    [TearDown]
    public virtual void TearDown()
    {
        // Ensure all tweens are stopped before test teardown to avoid cross-test leakage.
        // Prefer the adapter's KillAll so the adapter-level semantics run, then call LeanTween.cancelAll
        // to clear any runtime state that might remain in the LeanTween system.
        try
        {
            if (_adapter != null)
            {
                _adapter.KillAll();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("LeanTweenAdapterTests TearDown: adapter.KillAll() threw: " + e.Message);
        }

        try
        {
            // Defensive: clear any LeanTween runtime state/timers that could affect the next test.
            LeanTween.cancelAll();
        }
        catch
        {
            // Ignore: if LeanTween isn't available or cancelAll fails, continue teardown.
        }

        if (_testGo != null) UnityObj.DestroyImmediate(_testGo);
        if (_adapter != null) UnityObj.DestroyImmediate(_adapter);
        _testGo = null;
        _adapter = null;
    }
}