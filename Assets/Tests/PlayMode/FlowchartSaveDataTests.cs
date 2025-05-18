using NUnit.Framework;
using UnityEngine;
using Amanita.SaveSys;
using Fungus;
using System.Collections.Generic;
using System;
using UnityObject = UnityEngine.Object;
using System.Linq;

namespace Amanita.SaveSystemTests
{
    public class FlowchartSaveDataTests
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
            PrepVars();
        }

        protected GameObject varStateTestPrefab;
        protected GameObject varStateTestScene;

        protected Flowchart flowchart;

        protected virtual void PrepVars()
        {
            nameVar = (StringVariable)flowchart.GetVariable("name");
            scoreVar = (IntegerVariable)flowchart.GetVariable("score");
            newPlayerVar = (BooleanVariable)flowchart.GetVariable("newPlayer");
            fastestTimeVar = (FloatVariable)flowchart.GetVariable("fastestTimeInSeconds");
            threeDPosVar = (Vector3Variable)flowchart.GetVariable("threeDPos");
            twoDPosVar = (Vector2Variable)flowchart.GetVariable("twoDPos");

            stringVar = flowchart.gameObject.AddComponent<StringVariable>();
            stringVar.Value = "Hello, World!";
            flowchart.Variables.Add(stringVar);

            transformVar = (TransformVariable)flowchart.GetVariable("someTrans");
        }

        protected StringVariable nameVar = null;
        protected IntegerVariable scoreVar = null;
        protected BooleanVariable newPlayerVar = null;
        protected FloatVariable fastestTimeVar = null;
        protected Vector3Variable threeDPosVar = null;
        protected Vector2Variable twoDPosVar = null;
        protected StringVariable stringVar = null;
        protected TransformVariable transformVar = null;


        [TearDown]
        public virtual void DoTearDown()
        {
            UnityObject.DestroyImmediate(varStateTestScene);
        }

        [Test]
        public virtual void FlowchartSaveData_Constructor_SetsUniqueId()
        {
            FlowchartSaveData flowchartSaveData = new(flowchart);
            Assert.AreEqual(flowchart.UniqueId, flowchartSaveData.UniqueId);
        }

        [Test]
        public virtual void FlowchartSaveData_Constructor_SetsFlowchartName()
        {
            FlowchartSaveData flowchartSaveData = new(flowchart);
            Assert.AreEqual(flowchart.name, flowchartSaveData.FlowchartName);
        }

        [Test]
        public virtual void FlowchartSaveData_Constructor_SetsSavedVars()
        {
            FlowchartSaveData flowchartSaveData = new(flowchart);
            foreach (Variable var in flowchart.Variables)
            {
                IVarEncoder forThisVar = EncoderRegistry.GetEncoder(var);
                if (forThisVar == null)
                {
                    continue;
                }

                bool hasVarWithTheId = flowchartSaveData.SavedVars.Any(v => v.UniqueID == var.UniqueId);
                Assert.IsTrue(hasVarWithTheId);
            }
        }


    }
}