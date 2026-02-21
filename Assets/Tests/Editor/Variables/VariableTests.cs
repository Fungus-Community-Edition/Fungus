using NUnit.Framework;
using UnityEngine;
using Amanita.VScripting;
using System.Collections.Generic;
using UnityObj = UnityEngine.Object;
using Amanita;

namespace VScriptingTests.VariableOperations
{
    public abstract class VariableTests 
    {
        [SetUp]
        public virtual void SetUp()
        {
            VariableTypeDiscovery.DiscoverAndRegister();
            manager = AmanitaManager.EnsureExists();
            fcHolder = new GameObject("FlowchartHolder");
            flowchart = fcHolder.AddComponent<Flowchart>();
            _toDestroy.Add(fcHolder);
            _toDestroy.Add(manager.gameObject);
        }

        protected readonly List<UnityObj> _toDestroy = new();
        protected GameObject fcHolder;
        protected Flowchart flowchart;
        protected AmanitaManager manager;

        [TearDown]
        public virtual void TearDown()
        {
            foreach (var obj in _toDestroy)
            {
                if (obj != null && obj != AmanitaManager.S.gameObject)
                {
                    UnityObj.DestroyImmediate(obj);
                }
            }
        
            _toDestroy.Clear();

            fcHolder = null;
            flowchart = null;
            manager = null;
        }
    }
}