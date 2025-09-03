using Amanita.VScripting;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;

namespace Amanita.Tests.EditMode
{
    public class MuscariableHookTests
    {
        [SetUp]
        public virtual void SetUp()
        {
            _hookedInt = new HookedIntMuscariable();
            _baseHookedInt = _hookedInt;
        }

        protected HookedIntMuscariable _hookedInt;
        protected Muscariable _baseHookedInt;
        protected readonly int _startingVal = 0;
        protected string _errorMessage;

        [TearDown]
        public virtual void TearDown()
        {
            _hookedInt = null;
            _baseHookedInt = null;
            _errorMessage = "";
        }

        [Test]
        [TestCaseSource(nameof(SingleTargVals))]
        public virtual void SetBase_KeepsValue(int targVal)
        {
            _baseHookedInt.Value = targVal;
            _errorMessage = "Setting the value through the base should make the base actually stay that val";
            Assert.AreEqual(targVal, _baseHookedInt.Value, _errorMessage);
        }

        public static IEnumerable SingleTargVals()
        {
            yield return new TestCaseData(-1); 
            yield return new TestCaseData(12);
            yield return new TestCaseData(-456);
            yield return new TestCaseData(8976);
            yield return new TestCaseData(-342785);
        }

        [Test]
        [TestCaseSource(nameof(SingleTargVals))]
        public virtual void SetGeneric_KeepsValue(int targVal)
        {
            _hookedInt.Value = targVal;
            _errorMessage = "Setting the value through the generic should make the generic actually stay that val";
            Assert.AreEqual(targVal, _hookedInt.Value, _errorMessage);
        }

        [Test]
        [TestCaseSource(nameof(SingleTargVals))]
        public virtual void SetBase_SyncsToGeneric(int targVal)
        {
            _baseHookedInt.Value = targVal;
            _errorMessage = "Setting the value through the base should set the generic to the same one";
            Assert.AreEqual(targVal, _hookedInt.Value, _errorMessage);
        }

        [Test]
        [TestCaseSource(nameof(SingleTargVals))]
        public virtual void SetGeneric_SyncsToBase(int targVal)
        {
            _hookedInt.Value = targVal;
            _errorMessage = "Setting through the generic should set the base to the same one";
            Assert.AreEqual(targVal, _baseHookedInt.Value, _errorMessage);
        }

        [Test]
        [TestCaseSource(nameof(SingleTargVals))]
        public virtual void Setting_GenericValue_Invokes_OnGenericValueSet_AndSyncsBase(int targVal)
        {
            // Act
            _hookedInt.Value = targVal;

            // Assert
            Assert.AreEqual(1, _hookedInt.GenericSetCount, "Generic hook should be called once");
            Assert.AreEqual(0, _hookedInt.LastGenericPrev, "Prev generic value should be default(int) on first set");
            Assert.AreEqual(targVal, (int)((Muscariable)_hookedInt).Value, "Base getter should reflect generic set");
        }

        public static IEnumerator<TestCaseData> TargValPairs()
        {
            yield return new TestCaseData(0, -12);
            yield return new TestCaseData(12, -349785);
            yield return new TestCaseData(456, -75);
            yield return new TestCaseData(8976, -863);
            yield return new TestCaseData(342785, -5);
        }

        [Test]
        [TestCaseSource(nameof(SingleTargVals))]
        public virtual void SetGeneric_CallsBaseHookOnce(int targVal)
        {
            _hookedInt.Value = targVal;

            _errorMessage = "Setting the value through the generic once " +
                "should only call the base hook once.";
            Assert.AreEqual(1, _hookedInt.BaseSetCount, _errorMessage);
        }

        [Test]
        [TestCaseSource(nameof(SingleTargVals))]
        public virtual void SetGeneric_CallsGenericHookOnce(int targVal)
        {
            _hookedInt.Value = targVal;
            _errorMessage = "Setting through the generic once should only call the generic hook once";
            Assert.AreEqual(1, _hookedInt.GenericSetCount, _errorMessage);
        }

        [Test]
        [TestCaseSource(nameof(SingleTargVals))]
        public virtual void SetGeneric_CallsBaseAndGenericHookOnce(int targVal)
        {
            _hookedInt.Value = targVal;

            _errorMessage = "Setting through the generic once should only call the generic hook once";
            Assert.AreEqual(1, _hookedInt.GenericSetCount, _errorMessage);

            _errorMessage = "Setting the value through the generic once " +
                "should only call the base hook once.";
            Assert.AreEqual(1, _hookedInt.BaseSetCount, _errorMessage);
        }

        [Test]
        [TestCaseSource(nameof(SingleTargVals))]
        public virtual void SetBase_CallsBaseHookOnce(int targVal)
        {
            _baseHookedInt.Value = targVal;
            _errorMessage = "Setting the value through the base once should call the base hook only once.";
            Assert.AreEqual(1, _hookedInt.BaseSetCount);
        }

        [Test]
        [TestCaseSource(nameof(SingleTargVals))]
        public virtual void SetBase_CallsOnlyBaseHook(int targVal)
        {
            _baseHookedInt.Value = targVal;
            _errorMessage = "Setting the value through the base once should not call the generic hook.";
            Assert.AreEqual(0, _hookedInt.GenericSetCount);
        }

        [Test]
        [TestCaseSource(nameof(SingleTargVals))]
        public virtual void SetBase_AssignSameValue_NoHookCalled(int targVal)
        {
            for (int i = 0; i < 5; i++)
            {
                _baseHookedInt.Value = targVal;
            }
            
            // ^Can't set this to _baseHookedInt.Vaue since that's a base C# object, and the generic won't accept that
            _errorMessage = "Setting the base to the value it already has should not trigger either hook.";
            Assert.AreEqual(0, _hookedInt.GenericSetCount, _errorMessage);
            Assert.AreEqual(1, _hookedInt.BaseSetCount, _errorMessage);
            // ^Since the first set to targVal should set only the base set count to 1
        }

        [Test]
        [TestCaseSource(nameof(SingleTargVals))]
        public virtual void SetGeneric_AssignSameValue_NoHookCalled(int targVal)
        {
            for (int i = 0; i < 5; i++)
            {
                _hookedInt.Value = targVal;
            }

            _errorMessage = "Setting the generic to the value it already has should not trigger either hook.";
            Assert.AreEqual(1, _hookedInt.GenericSetCount, _errorMessage);
            Assert.AreEqual(1, _hookedInt.BaseSetCount, _errorMessage);
            // ^Since the first set to targVal should trigger each hook once
        }
    }
}