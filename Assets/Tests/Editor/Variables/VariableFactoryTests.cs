using NUnit.Framework;
using System;
using UnityEngine;
using AtMycelia.Hyphlow;
using UnityEngine.TestTools;

namespace VScriptingTests.VariableOperations
{
    public class VariableFactoryTests
    {
        [SetUp]
        public void SetUp()
        {
            GetTheRightTypesRegistered();
            static void GetTheRightTypesRegistered()
            {
                VariableTypeRegistry.Clear();
                VariableDataTypeRegistry.Clear();

                VariableTypeRegistry.RegisterVariableType(typeof(IntMuscariable), new VariableTypeActions());
                VariableTypeRegistry.RegisterVariableType(typeof(IntegerVariable), new VariableTypeActions());
                VariableTypeRegistry.RegisterVariableType(typeof(HookedIntMuscariable), new VariableTypeActions());
                VariableTypeRegistry.RegisterVariableType(typeof(FakeIntLegacyVar), new VariableTypeActions());
            }
        }

        [TearDown]
        public virtual void TearDown()
        {
            Debug.unityLogger.logEnabled = true;
        }

        [Test]
        public void Create_Muscariable_From_ContentType_CopiesFields()
        {
            // Arrange
            var contentType = typeof(int);
            
            var source = new IntMuscariable
            {
                Key = "TestKey",
                Scope = VariableScope.Public,
                ItemId = 42,
                Value = 99
            };

            // Act
            var created = VariableFactory.CreateByContentType(contentType, source) as IntMuscariable;

            // Assert
            Assert.NotNull(created);
            Assert.AreEqual("TestKey", created.Key);
            Assert.AreEqual(VariableScope.Public, created.Scope);
            Assert.AreEqual(42, created.ItemId);
            Assert.AreEqual(99, created.Value);
        }

        [Test]
        public void CreateMuscari_SourceHasMismatchedContentType_ReturnsNull()
        {
            // Arrange
            Debug.unityLogger.logEnabled = false;
            
            var source = new HookedIntMuscariable { Value = 123 };

            // Act
            Type wrongContentType = typeof(int); // Since the int is the source while the string is the intended output
            Type rightContentType = typeof(string);

            var result = VariableFactory.CreateByContentType(rightContentType, source);

            // Assert
            Assert.IsNull(result, "Factory should not create when source content type mismatches that of the intended result");
        }

        [Test]
        public void CreateMuscari_SourceHasMismatchedContentType_LogsWarning()
        {
            // Arrange
            var source = new HookedIntMuscariable { Value = 123 };

            // Act
            Type wrongContentType = typeof(int); // Since the int is the source while the string is the intended output
            Type rightContentType = typeof(string);

            string expectedLogMessage = $"Cannot copy over the values of a variable of ContentType " +
                    $"{wrongContentType.Name} when creating a Muscariable of ContentType {rightContentType.Name}. "
                    + "Returning null.";
            LogAssert.Expect(LogType.Warning, expectedLogMessage);
            var result = VariableFactory.CreateByContentType(rightContentType, source);

        }

        [Test]
        public void AddLegacyVarTo_WhenFlowchartAddFails_LogsError()
        {
            // Arrange
            var fcHolder = new GameObject("FlowchartHolder");
            var flowchart = fcHolder.AddComponent<FakeFlowchartThatFailsAdd>();
            Type desiredLegacyType = typeof(IntegerVariable);

            // Act
            string errorMessage = $"Failed to add legacy variable component of type " +
                    $"{desiredLegacyType.Name} to Flowchart {flowchart.name}. Returning null.";
            LogAssert.Expect(LogType.Warning, errorMessage);
            var result = VariableFactory.AddLegacyVarTo(flowchart, typeof(int));
            
        }

        [Test]
        public void AddLegacyVarTo_WhenFlowchartAddFails_ReturnsNull()
        {
            Debug.unityLogger.logEnabled = false;
            // Arrange
            var fcHolder = new GameObject("FlowchartHolder");
            var flowchart = fcHolder.AddComponent<FakeFlowchartThatFailsAdd>();

            // Act
            var result = VariableFactory.AddLegacyVarTo(flowchart, typeof(int));

            // Assert
            Assert.IsNull(result, "Should return null when Flowchart refuses to add variable");
        }

        // --- Fakes for testing ---
        [VariableInfo("", "", typeof(int))]
        public class FakeIntLegacyVar : IntegerVariable { }

        public class FakeFlowchartThatFailsAdd : Flowchart
        {
            public override IVariable AddVariable(IVariable v)
            {
                throw new System.Exception("Simulated failure");
            }
        }

        // Cases: (contentType, expectedMuscariType)
        public static object[] MuscariCreationCases =
        {
            new object[] { typeof(float),  typeof(FloatMuscariable) },
            new object[] { typeof(int),    typeof(IntMuscariable) },
            
            new object[] { typeof(bool), typeof(BoolMuscariable) },
            new object[] { typeof(Vector2), typeof(VectorTwoMuscariable) },
            new object[] { typeof(Vector3), typeof(VectorThreeMuscariable) },

            new object[] { typeof(string), typeof(StringMuscariable) },
            new object[] { typeof(GameObject), typeof(GameObjectMuscariable) },
            new object[] { typeof(Transform), typeof(TransformMuscariable) },
            
            new object[] { typeof(AudioClip), typeof(AudioClipMuscariable) },
            new object[] { typeof(AudioSource), typeof(AudioSourceMuscariable) },

            new object[] { typeof(Sprite), typeof(SpriteMuscariable) },
            new object[] { typeof(Animator), typeof(AnimatorMuscariable) },
            new object[] { typeof(Material), typeof(MaterialMuscariable) },
            new object[] { typeof(Texture), typeof(TextureMuscariable) },

        };

        [TestCaseSource(nameof(MuscariCreationCases))]
        public void Create_Returns_Registered_Muscariable_Type(Type contentType, Type expectedMuscariType)
        {
            VariableTypeDiscovery.DiscoverAndRegister();
            var created = VariableFactory.CreateByContentType(contentType, null);
            Assert.IsNotNull(created, "VariableFactory.Create returned null for contentType " + contentType.Name);
            Assert.IsTrue(expectedMuscariType.IsAssignableFrom(created.GetType()),
                $"Factory did not return a Muscariable type assignable to {expectedMuscariType.Name} for {contentType.Name}");
            Assert.AreEqual(contentType, created.ContentType, "Created Muscariable did not report correct ContentType.");
        }

    }

}