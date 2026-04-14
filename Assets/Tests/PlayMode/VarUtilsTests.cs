using AtMycelia.Hyphlow;
using NUnit.Framework;
using UnityEngine;
using System.Collections;
using Type = System.Type;
using UnityObj = UnityEngine.Object;

namespace VScriptingTests
{
    public class VarUtilsTests
    {
        private GameObject flowchartGO;
        private Flowchart flowchart;

        [SetUp]
        public void SetUp()
        {
            flowchartGO = new GameObject("TestFlowchart");
            flowchart = flowchartGO.AddComponent<Flowchart>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityObj.DestroyImmediate(flowchartGO);
        }

        [TestCaseSource(nameof(TestMuscaris))]
        public void ReturnsSameInstance_WhenAlreadyMuscariable(Muscariable muscariable)
        {
            var result = muscariable.ToMuscariable();
            Assert.AreSame(muscariable, result);
        }

        public static readonly Muscariable[] TestMuscaris = new Muscariable[]
        {
            new IntMuscariable { Key = "intVar", Value = 10 },
            new FloatMuscariable { Key = "floatVar", Value = 3.14f },
            new VectorTwoMuscariable { Key = "vector2Var", Value = new Vector2(1, 2) },
            new VectorThreeMuscariable { Key = "vector3Var", Value = new Vector3(1, 2, 3) },
            new BoolMuscariable { Key = "boolVar", Value = true },

            new StringMuscariable { Key = "stringVar", Value = "test" },

            new ColorMuscariable { Key = "colorVar", Value = Color.red },
            new SpriteMuscariable { Key = "spriteVar", Value = null },

            new UnityObjectMuscariable { Key = "unityObjectVar", Value = null },
            new GameObjectMuscariable { Key = "gameObjectVar", Value = null },
            new TransformMuscariable {Key = "transformVar", Value = null },
            new AudioClipMuscariable { Key = "audioVar", Value = null },
            
        };


        [Test, TestCaseSource(nameof(VarTypeLinks))]
        public void ConvertsLegacyToCorrectMuscariType(Type legacyType, Type expectedContentType,
            Type muscariType)
        {
            var legacyVar = flowchartGO.AddComponent(legacyType) as Variable;
            legacyVar.Key = $"{legacyType.Name}_Var";
            legacyVar.Value = default;

            var result = legacyVar.ToMuscariable();

            Assert.IsNotNull(result, "Conversion resulted in a null");
            Assert.IsInstanceOf(muscariType, result, $"Var of legacy type {legacyType.Name} was not " +
                $"successfully converted to a var of type {muscariType.Name}");
            Assert.AreEqual(expectedContentType, result.ContentType,  "Wrong content type");
        }

        public static IEnumerable VarTypeLinks()
        {
            yield return new TestCaseData(typeof(IntegerVariable), typeof(int), typeof(IntMuscariable));
            yield return new TestCaseData(typeof(FloatVariable), typeof(float), typeof(FloatMuscariable));
            yield return new TestCaseData(typeof(Vector2Variable), typeof(Vector2), typeof(VectorTwoMuscariable));
            yield return new TestCaseData(typeof(Vector3Variable), typeof(Vector3), typeof(VectorThreeMuscariable));
            yield return new TestCaseData(typeof(BooleanVariable), typeof(bool), typeof(BoolMuscariable));

            yield return new TestCaseData(typeof(StringVariable), typeof(string), typeof(StringMuscariable));

            yield return new TestCaseData(typeof(SpriteVariable), typeof(Sprite), typeof(SpriteMuscariable));
            yield return new TestCaseData(typeof(ColorVariable), typeof(Color), typeof(ColorMuscariable));

            yield return new TestCaseData(typeof(ObjectVariable), typeof(UnityObj), typeof(UnityObjectMuscariable));
            yield return new TestCaseData(typeof(GameObjectVariable), typeof(GameObject), typeof(GameObjectMuscariable));
            yield return new TestCaseData(typeof(TransformVariable), typeof(Transform), typeof(TransformMuscariable));
            yield return new TestCaseData(typeof(AudioClipVariable), typeof(AudioClip), typeof(AudioClipMuscariable));
        }

    }
}