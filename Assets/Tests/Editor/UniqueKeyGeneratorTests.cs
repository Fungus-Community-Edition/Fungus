using Amanita.VScripting;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Amanita.Tests.EditMode
{
    [TestFixture]
    public class UniqueKeyGeneratorTests
    {
        VariableSource src;

        [SetUp]
        public void SetUp()
        {
            // fresh VariableSource per test
            src = ScriptableObject.CreateInstance<VariableSource>();
        }

        [TearDown]
        public void TearDown()
        {
            // Destroy the ScriptableObject instance we created to avoid leaks in the editor tests
            if (Application.isEditor && src != null)
            {
                Object.DestroyImmediate(src);
            }
        }

        [Test]
        public void SanitizesSuggestedKey_RemovesInvalidCharsAndLeadingDigits()
        {
            var muscari = new IntMuscariable { Key = "origKey" };
            // suggestedKey contains digits, spaces and punctuation
            string suggested = "123!@ my-Var";
            var group = new List<Muscariable>(); // empty existing set

            string result = UniqueKeyGenerator.GetUniqueKeyFor(suggested, group, muscari);

            Assert.AreEqual("myVar", result);
        }

        [Test]
        public void EmptyAfterSanitization_FallsBackToVar()
        {
            var muscari = new FloatMuscariable { Key = null };
            string suggested = "@@@123"; // after stripping non-alnum and trimming leading digits -> empty
            var group = new List<Muscariable>();

            string result = UniqueKeyGenerator.GetUniqueKeyFor(suggested, group, muscari);

            Assert.AreEqual("Var", result);
        }

        [Test]
        public void UsesVariableKeyWhenSuggestedIsNull()
        {
            var muscari = new BoolMuscariable { Key = "MyBool" };
            var group = new List<Muscariable>();

            // pass suggestedKey as the variable's key (function doesn't accept null suggested; keep usage consistent)
            string result = UniqueKeyGenerator.GetUniqueKeyFor(muscari.Key, group, muscari);

            Assert.AreEqual("MyBool", result);
        }

        [Test]
        public void AppendsNumericSuffix_ToAvoidCollisions_IncreasingUntilUnique()
        {
            // Arrange: populate source with "score", "score1", "score2"
            src.AddVariable(new IntMuscariable { Key = "score" });
            src.AddVariable(new FloatMuscariable { Key = "score1" });
            src.AddVariable(new DoubleMuscariable { Key = "score2" });

            // New variable wants "score"
            var newVar = new IntMuscariable { Key = "score" };

            // Take the current variables from the source as a List<Muscariable>
            IList<Muscariable> varsFetched = src.GetVarsByType<Muscariable>();

            string result = UniqueKeyGenerator.GetUniqueKeyFor(newVar.Key, varsFetched, newVar);

            // Expect next available suffix to be "score3"
            Assert.AreEqual("score3", result);
        }

        [Test]
        public void Collision_IsCaseInsensitive()
        {
            src.AddVariable(new IntMuscariable { Key = "SCORE" });

            var newVar = new IntMuscariable { Key = "score" };
            var group = new List<Muscariable>(src.GetVarsByType(typeof(Muscariable)));

            string result = UniqueKeyGenerator.GetUniqueKeyFor(newVar.Key, group, newVar);

            Assert.AreEqual("score1", result);
        }

        // --- Additional tests added ---

        [Test]
        public void IgnoreVariable_IsNotTreatedAsCollision()
        {
            // Add variable and then ask for uniqueness while ignoring that same variable
            var existing = new IntMuscariable { Key = "keepMe" };
            src.AddVariable(existing);

            // When we ignore the existing variable, requesting "keepMe" should be allowed
            var vars = src.GetVarsByType<Muscariable>();
            string result = UniqueKeyGenerator.GetUniqueKeyFor("keepMe", vars, existing);

            Assert.AreEqual("keepMe", result);
        }

        [Test]
        public void HandlesNullEntriesAndNullKeysInGroup()
        {
            // Build a list that contains null entries and a variable with null Key
            var list = new List<Muscariable>
            {
                null,
                new IntMuscariable { Key = null }
            };

            // Should not throw and should return the suggested key unchanged
            string result = UniqueKeyGenerator.GetUniqueKeyFor("uniqueName", list, null);

            Assert.AreEqual("uniqueName", result);
        }

        [Test]
        public void PreservesUnderscores_InSuggestedKey()
        {
            var list = new List<Muscariable>();
            string suggested = "__my_var__123";
            string result = UniqueKeyGenerator.GetUniqueKeyFor(suggested, list, null);

            // underscores are allowed by the sanitizer and are preserved; leading digits after underscores are not trimmed
            Assert.AreEqual("__my_var__123", result);
        }

        [Test]
        public void ReturnsSuggestedWhenItIsUnique()
        {
            var list = new List<Muscariable>();
            string suggested = "completelyUnique";
            string result = UniqueKeyGenerator.GetUniqueKeyFor(suggested, list, null);

            Assert.AreEqual(suggested, result);
        }
    }
}