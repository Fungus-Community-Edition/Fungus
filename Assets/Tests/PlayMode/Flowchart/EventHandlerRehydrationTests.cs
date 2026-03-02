using AtMycelia.Amanita.VScripting.EventHandlers;
using AtMycelia.Amanita.VScripting;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using BindingFlags = System.Reflection.BindingFlags;
using Type = System.Type;

namespace SaveSystemTests
{
    /// <summary>
    /// Harness to verify EventHandler variable rehydration.
    /// Inherits all setup/teardown logic from CommonTestFunctionality.
    /// </summary>
    public class EventHandlerRehydrationTests : CommonTestFunctionality
    {
        // No SaveSystem needed; we only need the scene + flowchart.
        protected override bool ReqSaveSystem => false;

        private TestRehydrationEventHandler singleHandler;
        private MultiFieldRehydrationHandler multiHandler;

        [SetUp]
        public override void DoSetUp()
        {
            base.DoSetUp();

            var fcGo = flowchart.gameObject;
            singleHandler = fcGo.AddComponent<TestRehydrationEventHandler>();
            multiHandler = fcGo.AddComponent<MultiFieldRehydrationHandler>();
            multiHandler.ParentBlock = flowchart.GetComponentInChildren<Block>(true);
            singleHandler.ParentBlock = multiHandler.ParentBlock;
        }

        [UnityTest]
        public IEnumerator RehydratesDetachedVariableReference()
        {
            // Arrange: assign a detached copy of the variable
            var detachedCopy = new StringMuscariable { Value = "Detached" };
            detachedCopy.ItemId = nameVar.ItemId;

            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            _rehydroHandlerType
                .GetField("testStringVar", flags)
                .SetValue(singleHandler, detachedCopy);

            Assert.That(detachedCopy.Owner, Is.Null, "Sanity check: detached copy should have null Owner");

            // Act
            singleHandler.ForceRehydrateVariables();
            yield return null;

            // Assert
            var hydrated = singleHandler.TestStringVar;
            Assert.That(hydrated, Is.Not.Null);
            Assert.That(hydrated.Owner, Is.EqualTo(flowchart));
            Assert.That(hydrated.Value, Is.EqualTo(nameVar.Value),
                "Rehydrated variable should match the Flowchart's canonical value");
        }

        private static readonly Type _rehydroHandlerType = typeof(TestRehydrationEventHandler);

        [UnityTest]
        public IEnumerator RehydratesMultipleDetachedVariables()
        {
            #region Assign detached copies with matching ItemIds
            var detachedName = new StringMuscariable { Value = "DetachedName", ItemId = nameVar.ItemId };
            var detachedScore = new IntMuscariable { Value = -999, ItemId = scoreVar.ItemId };
            var detachedIsNew = new BoolMuscariable { Value = true, ItemId = isNewPlayerVar.ItemId };

            typeof(MultiFieldRehydrationHandler)
                .GetField("testNameVar", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(multiHandler, detachedName);

            typeof(MultiFieldRehydrationHandler)
                .GetField("testScoreVar", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(multiHandler, detachedScore);

            typeof(MultiFieldRehydrationHandler)
                .GetField("testIsNewPlayerVar", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(multiHandler, detachedIsNew);

            Assert.That(detachedName.Owner, Is.Null);
            Assert.That(detachedScore.Owner, Is.Null);
            Assert.That(detachedIsNew.Owner, Is.Null);
            #endregion

            // Act
            multiHandler.ForceRehydrateVariables();
            yield return null;

            // Assert: all fields point to canonical Flowchart variables
            Assert.That(multiHandler.TestNameVar, Is.SameAs(nameVar));
            Assert.That(multiHandler.TestScoreVar, Is.SameAs(scoreVar));
            Assert.That(multiHandler.TestIsNewPlayerVar, Is.SameAs(isNewPlayerVar));

            Assert.That(multiHandler.TestNameVar.Owner, Is.EqualTo(flowchart));
            Assert.That(multiHandler.TestScoreVar.Owner, Is.EqualTo(flowchart));
            Assert.That(multiHandler.TestIsNewPlayerVar.Owner, Is.EqualTo(flowchart));
        }

        [UnityTest]
        public IEnumerator AlreadyHydratedVariableIsLeftAlone()
        {
            // Arrange: assign a hydrated variable directly
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            _rehydroHandlerType
                .GetField("testStringVar", flags)
                .SetValue(singleHandler, nameVar);

            var before = singleHandler.TestStringVar;
            var expectedOwner = before.Owner;
            Assert.That(expectedOwner, Is.Not.Null, "Sanity check: variable is already hydrated");

            // Act
            singleHandler.ForceRehydrateVariables();
            yield return null;

            // Assert: reference and owner are unchanged
            var after = singleHandler.TestStringVar;
            Assert.That(after, Is.SameAs(before), "Already hydrated variable should not be replaced");
            Assert.That(after.Owner, Is.SameAs(expectedOwner), "Owner should remain unchanged");
        }

        [UnityTest]
        public IEnumerator InvalidItemIdLogsErrorAndDoesNotRehydrate()
        {
            // Arrange: assign a detached copy with a bogus ItemId
            var bogusCopy = new StringMuscariable { Value = "Bogus", ItemId = 0 };
            _rehydroHandlerType
                .GetField("testStringVar", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(singleHandler, bogusCopy);

            Assert.That(bogusCopy.Owner, Is.Null, "Sanity check: bogus copy should have null Owner");

            // Expect an error log
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Variable .* not found"));

            // Act
            singleHandler.ForceRehydrateVariables();
            yield return null;

            // Assert: field still points to the bogus copy
            var after = singleHandler.TestStringVar;
            Assert.That(after, Is.SameAs(bogusCopy), "Invalid ItemId should not be replaced");
        }
    }

    [EventHandlerInfo("Test", "MultiRehydration", "Triggered for multi-field rehydration tests.")]
    public class MultiFieldRehydrationHandler : EventHandler
    {
        [VariableProperty(typeof(StringVariable))]
        [SerializeReference] protected IVariable<string> testNameVar;

        [VariableProperty(typeof(IntegerVariable))]
        [SerializeReference] protected IVariable<int> testScoreVar;

        [VariableProperty(typeof(BooleanVariable))]
        [SerializeReference] protected IVariable<bool> testIsNewPlayerVar;

        public IVariable<string> TestNameVar => testNameVar;
        public IVariable<int> TestScoreVar => testScoreVar;
        public IVariable<bool> TestIsNewPlayerVar => testIsNewPlayerVar;

        public void ForceRehydrateVariables() => RehydrateVariables();
        protected override bool RehydrateVarInputs => true;
    }

    [EventHandlerInfo("Test", "Rehydration", "Triggered for rehydration tests.")]
    public class TestRehydrationEventHandler : EventHandler
    {
        [VariableProperty(typeof(StringVariable))]
        [SerializeReference] protected IVariable<string> testStringVar;

        public IVariable<string> TestStringVar => testStringVar;
        public virtual void ForceOnEnable() => OnEnable();
        public virtual void ForceRehydrateVariables() => RehydrateVariables();
        protected override bool RehydrateVarInputs => true;
    }
}
