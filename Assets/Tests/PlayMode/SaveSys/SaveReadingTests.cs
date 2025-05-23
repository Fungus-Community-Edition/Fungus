using NUnit.Framework;
using UnityEngine;
using System.Collections;
using Amanita.SaveSys;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
using System.Linq;
using UnityEngine.TestTools;

namespace Amanita.SaveSystemTests
{
    public class SaveReadingTests
    {
        protected string toVarStateTests = "ScenePrefabs/VarStateTests";

        [SetUp]
        public virtual void DoSetUp()
        {
            PrepScene();
        }

        protected virtual void PrepScene()
        {
            varStateTestPrefab = Resources.Load<GameObject>(toVarStateTests);
            varStateTestScene = UnityObject.Instantiate(varStateTestPrefab);
            flowchart = varStateTestScene.GetComponentInChildren<Flowchart>();
        }

        protected GameObject varStateTestPrefab;
        protected GameObject varStateTestScene;

        protected Flowchart flowchart;

        [TearDown]
        public virtual void DoTearDown()
        {
            UnityObject.DestroyImmediate(varStateTestScene);
        }

        public virtual void SavesToDisk()
        {

        }

    }
}