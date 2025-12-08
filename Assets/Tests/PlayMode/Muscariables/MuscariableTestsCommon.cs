using UnityEngine;

namespace VScriptingTests.MuscariableTests
{
    public abstract class MuscariableTestsCommon
    {
        protected const int SampleInt = 10;
        protected const float SampleF = 2.5f;
        protected const double SampleD = 3.5;
        protected const string SampleS = "hello";
        protected const string OtherS = "world";

        protected const float Epsilon = 1e-5f;

        // Sample values
        protected static readonly Vector2 V2A = new Vector2(1.5f, -2.0f);
        protected static readonly Vector2 V2B = new Vector2(-1.0f, 3.25f);

        protected static readonly Vector3 V3A = new Vector3(0.5f, 2.0f, -1.0f);
        protected static readonly Vector3 V3B = new Vector3(1.0f, -1.5f, 4.0f);

    }
}