#if UNITY_2019_2_OR_NEWER
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

namespace AtMycelia.Amanita.Tests
{
    [TestFixture]
    public class FungusPlayModeTest
    {
        [UnityTest]
        public IEnumerator Looping()
        {
            yield return Hyphlow.EditorExt.TestUtils.RunPrefabFlowchartTests("LoopTest", true, 200);
        }

        [UnityTest]
        public IEnumerator ControlFlow()
        {
            yield return Hyphlow.EditorExt.TestUtils.RunPrefabFlowchartTests("FlowTest", true);
        }

        [UnityTest]
        public IEnumerator VariableSets()
        {
            yield return Hyphlow.EditorExt.TestUtils.RunPrefabFlowchartTests("VarSetTest", true);
        }
    }
}
#endif