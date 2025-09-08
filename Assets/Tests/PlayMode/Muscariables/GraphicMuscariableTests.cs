using System;
using NUnit.Framework;
using UnityEngine;
using Amanita.VScripting;

namespace Amanita.MuscariableTests.DataOnly
{
    [TestFixture]
    public class GraphicMuscariableTests
    {
        [SetUp]
        public void Setup()
        {
            // Create two distinct textures
            firstTex = new Texture2D(2, 2);
            secondTex = new Texture2D(2, 2);

            // Create sprites from those textures
            firstSprite = Sprite.Create(firstTex, new Rect(0, 0, 2, 2), Vector2.zero);
            secondSprite = Sprite.Create(secondTex, new Rect(0, 0, 2, 2), Vector2.zero);

            var shader = Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
            firstMaterial = new Material(shader);
            secondMaterial = new Material(shader);

            firstGameObjectForAnimator = new GameObject("AnimA");
            secondGameObjectForAnimator = new GameObject("AnimB");
            firstAnimator = firstGameObjectForAnimator.AddComponent<Animator>();
            secondAnimator = secondGameObjectForAnimator.AddComponent<Animator>();


        }

        protected Texture2D firstTex;
        protected Texture2D secondTex;
        protected Sprite firstSprite;
        protected Sprite secondSprite;

        protected Material firstMaterial;
        protected Material secondMaterial;

        protected GameObject firstGameObjectForAnimator;
        protected GameObject secondGameObjectForAnimator;
        protected Animator firstAnimator;
        protected Animator secondAnimator;

        [TearDown]
        public void Teardown()
        {
            UnityEngine.Object.DestroyImmediate(firstSprite);
            UnityEngine.Object.DestroyImmediate(secondSprite);

            UnityEngine.Object.DestroyImmediate(firstTex);
            UnityEngine.Object.DestroyImmediate(secondTex);

            UnityEngine.Object.DestroyImmediate(firstMaterial);
            UnityEngine.Object.DestroyImmediate(secondMaterial);

            UnityEngine.Object.DestroyImmediate(firstGameObjectForAnimator);
            UnityEngine.Object.DestroyImmediate(secondGameObjectForAnimator);

        }

        #region Color
        [Test]
        public void ColorMuscariable_InitAndNullGuard()
        {
            var colVar = new ColorMuscariable();
            var ex = Assert.Throws<Exception>(() => colVar.Init());
            StringAssert.Contains("needs a valid key", ex.Message);
            StringAssert.Contains("needs a valid ID", ex.Message);

            colVar.Key = "col";
            colVar.ItemID = 201;
            Assert.DoesNotThrow(() => colVar.Init());
        }

        [Test]
        public void ColorMuscariable_ValueAssignmentAndEvent()
        {
            var colVar = new ColorMuscariable { Key = "col", ItemID = 202 };
            colVar.Init();

            Color captured = default;
            colVar.OnValueChanged += c => captured = c;

            var c1 = new Color(0.1f, 0.2f, 0.3f, 0.4f);
            colVar.Value = c1;
            Assert.AreEqual(c1, colVar.Value);
            Assert.AreEqual(c1, captured);
        }

        [Test]
        public void ColorMuscariable_EqualityAndEvaluate()
        {
            var firstCol = new Color(0.5f, 0.5f, 0.5f, 1f);
            var secondCol = new Color(0.5f, 0.5f, 0.5f, 1f);
            var thirdCol = new Color(1f, 0f, 0f, 1f);

            var firstColVar = new ColorMuscariable { Key = "a", ItemID = 203, Value = firstCol };
            var secondColVar = new ColorMuscariable { Key = "b", ItemID = 204, Value = secondCol };
            var thirdColVar = new ColorMuscariable { Key = "c", ItemID = 205, Value = thirdCol };

            // operator==
            Assert.IsTrue(firstColVar == secondColVar);
            Assert.IsFalse(firstColVar != secondColVar);
            Assert.IsFalse(firstColVar == thirdColVar);
            Assert.IsTrue(firstColVar != thirdColVar);

            // Evaluate via CompareOperator
            Assert.IsTrue(firstColVar.Evaluate(CompareOperator.Equals, secondCol));
            Assert.IsFalse(firstColVar.Evaluate(CompareOperator.Equals, thirdCol));

            // Unsupported relational operator
            Assert.Throws<ArgumentException>(
                () => firstColVar.Evaluate(CompareOperator.GreaterThan, secondCol)
            );
        }

        [Test]
        public void ColorMuscariable_WrongTypeAssignment_Throws()
        {
            var colorVar = new ColorMuscariable { Key = "col", ItemID = 206 };
            colorVar.Init();
            Muscariable baseVar = colorVar;
            Assert.Throws<ArgumentException>(() => baseVar.Value = "not a color");
        }

        #endregion

        #region Sprites
        [Test]
        public void SpriteMuscariable_ValueAssignmentAndEvent()
        {
            var v = new SpriteMuscariable { Key = "spr", ItemID = 210 };
            v.Init();

            Sprite captured = null;
            v.OnValueChanged += s => captured = s;

            v.Value = firstSprite;
            Assert.AreEqual(firstSprite, v.Value);
            Assert.AreEqual(firstSprite, captured);
        }

        [Test]
        public void SpriteMuscariable_NullAndDestroyedBehavior()
        {
            var spriteVar = new SpriteMuscariable { Key = "spr", ItemID = 211 };
            spriteVar.Init();

            // Null assignment is allowed
            spriteVar.Value = null;
            Assert.IsNull(spriteVar.Value);

            // Assign and then destroy
            spriteVar.Value = firstSprite;
            UnityEngine.Object.DestroyImmediate(firstSprite);
            // UnityEngine.Object== returns true for destroyed objects
            Assert.IsTrue(spriteVar.Value == null);
        }

        [Test]
        public void SpriteMuscariable_EqualityAndEvaluate()
        {
            var firstSpriteVar = new SpriteMuscariable { Key = "a", ItemID = 212, Value = firstSprite };
            var secondSpriteVar = new SpriteMuscariable { Key = "b", ItemID = 213, Value = firstSprite };
            var thirdSpriteVar = new SpriteMuscariable { Key = "c", ItemID = 214, Value = secondSprite };

            Assert.IsTrue(firstSpriteVar == secondSpriteVar);
            Assert.IsFalse(firstSpriteVar != secondSpriteVar);
            Assert.IsFalse(firstSpriteVar == thirdSpriteVar);
            Assert.IsTrue(firstSpriteVar != thirdSpriteVar);

            Assert.IsTrue(firstSpriteVar.Evaluate(CompareOperator.Equals, firstSprite));
            Assert.IsFalse(firstSpriteVar.Evaluate(CompareOperator.Equals, secondSprite));
        }

        [Test]
        public void SpriteMuscariable_WrongTypeAssignment_Throws()
        {
            var spriteVar = new SpriteMuscariable { Key = "spr", ItemID = 215 };
            spriteVar.Init();
            Muscariable baseVar = spriteVar;
            Assert.Throws<ArgumentException>(() => baseVar.Value = firstTex);
        }

        #endregion

        #region Textures
        [Test]
        public void TextureMuscariable_ValueAssignmentAndEvent()
        {
            var texVar = new TextureMuscariable { Key = "tex", ItemID = 220 };
            texVar.Init();

            Texture captured = null;
            texVar.OnValueChanged += t => captured = t;

            texVar.Value = firstTex;
            Assert.AreEqual(firstTex, texVar.Value);
            Assert.AreEqual(firstTex, captured);
        }

        [Test]
        public void TextureMuscariable_NullAndDestroyedBehavior()
        {
            var texVar = new TextureMuscariable { Key = "tex", ItemID = 221 };
            texVar.Init();

            // Null assignment
            texVar.Value = null;
            Assert.IsNull(texVar.Value);

            // Assign and then destroy
            texVar.Value = secondTex;
            UnityEngine.Object.DestroyImmediate(secondTex);
            Assert.IsTrue(texVar.Value == null);
        }

        [Test]
        public void TextureMuscariable_EqualityAndEvaluate()
        {
            var firstTexVar = new TextureMuscariable { Key = "a", ItemID = 222, Value = firstTex };
            var secondTexVar = new TextureMuscariable { Key = "b", ItemID = 223, Value = firstTex };
            var thirdTexVar = new TextureMuscariable { Key = "c", ItemID = 224, Value = secondTex };

            Assert.IsTrue(firstTexVar == secondTexVar);
            Assert.IsFalse(firstTexVar != secondTexVar);
            Assert.IsFalse(firstTexVar == thirdTexVar);
            Assert.IsTrue(firstTexVar != thirdTexVar);

            Assert.IsTrue(firstTexVar.Evaluate(CompareOperator.Equals, firstTex));
            Assert.IsFalse(firstTexVar.Evaluate(CompareOperator.Equals, secondTex));
        }

        [Test]
        public void TextureMuscariable_WrongTypeAssignment_Throws()
        {
            var texVar = new TextureMuscariable { Key = "tex", ItemID = 225 };
            texVar.Init();
            Muscariable baseVar = texVar;
            Assert.Throws<ArgumentException>(() => baseVar.Value = firstSprite);
        }

        #endregion

        #region Materials

        [Test]
        public void MaterialMuscariable_InitAndNullGuard()
        {
            var matVar = new MaterialMuscariable();
            var ex = Assert.Throws<Exception>(() => matVar.Init());
            StringAssert.Contains("needs a valid key", ex.Message);
            StringAssert.Contains("needs a valid ID", ex.Message);

            matVar.Key = "mat";
            matVar.ItemID = 230;
            Assert.DoesNotThrow(() => matVar.Init());
        }

        [Test]
        public void MaterialMuscariable_ValueAssignmentAndEvent()
        {
            var matVar = new MaterialMuscariable { Key = "mat", ItemID = 231 };
            matVar.Init();

            Material captured = null;
            matVar.OnValueChanged += m => captured = m;

            matVar.Value = firstMaterial;
            Assert.AreEqual(firstMaterial, matVar.Value);
            Assert.AreEqual(firstMaterial, captured);
        }

        [Test]
        public void MaterialMuscariable_EqualityAndEvaluate()
        {
            var firstMatVar = new MaterialMuscariable { Key = "a", ItemID = 232, Value = firstMaterial };
            var secondMatVar = new MaterialMuscariable { Key = "b", ItemID = 233, Value = firstMaterial };
            var thirdMatVar = new MaterialMuscariable { Key = "c", ItemID = 234, Value = secondMaterial };

            Assert.IsTrue(firstMatVar == secondMatVar);
            Assert.IsFalse(firstMatVar != secondMatVar);
            Assert.IsFalse(firstMatVar == thirdMatVar);
            Assert.IsTrue(firstMatVar != thirdMatVar);

            Assert.IsTrue(firstMatVar.Evaluate(CompareOperator.Equals, firstMaterial));
            Assert.IsFalse(firstMatVar.Evaluate(CompareOperator.Equals, secondMaterial));
            Assert.Throws<ArgumentException>(
                () => firstMatVar.Evaluate(CompareOperator.LessThan, firstMaterial)
            );
        }

        [Test]
        public void MaterialMuscariable_NullAssignmentAllowed()
        {
            var matVar = new MaterialMuscariable { Key = "mat", ItemID = 235 };
            matVar.Init();

            Assert.DoesNotThrow(() => matVar.Value = null);
            Assert.IsNull(matVar.Value);
        }

        [Test]
        public void MaterialMuscariable_DestroyedMaterialBehavesAsNull()
        {
            var matVar = new MaterialMuscariable { Key = "mat", ItemID = 236 };
            matVar.Init();

            matVar.Value = firstMaterial;
            UnityEngine.Object.DestroyImmediate(firstMaterial);
            Assert.IsTrue(matVar.Value == null);
        }

        [Test]
        public void MaterialMuscariable_WrongTypeAssignmentThrows()
        {
            var matVar = new MaterialMuscariable { Key = "mat", ItemID = 237 };
            matVar.Init();
            Muscariable baseVar = matVar;
            Assert.Throws<ArgumentException>(() => baseVar.Value = "not a material");
        }

        #endregion

        [Test]
        public void AnimatorMuscariable_InitAndNullGuard()
        {
            var animVar = new AnimatorMuscariable();
            var ex = Assert.Throws<Exception>(() => animVar.Init());
            StringAssert.Contains("needs a valid key", ex.Message);
            StringAssert.Contains("needs a valid ID", ex.Message);

            animVar.Key = "anim";
            animVar.ItemID = 240;
            Assert.DoesNotThrow(() => animVar.Init());
        }

        [Test]
        public void AnimatorMuscariable_ValueAssignmentAndEvent()
        {
            var animVar = new AnimatorMuscariable { Key = "anim", ItemID = 241 };
            animVar.Init();

            Animator captured = null;
            animVar.OnValueChanged += a => captured = a;

            animVar.Value = firstAnimator;
            Assert.AreEqual(firstAnimator, animVar.Value);
            Assert.AreEqual(firstAnimator, captured);
        }

        [Test]
        public void AnimatorMuscariable_EqualityAndEvaluate()
        {
            var firstAnimVar = new AnimatorMuscariable { Key = "a", ItemID = 242, Value = firstAnimator };
            var secondAnimVar = new AnimatorMuscariable { Key = "b", ItemID = 243, Value = firstAnimator };
            var thirdAnimVar = new AnimatorMuscariable { Key = "c", ItemID = 244, Value = secondAnimator };

            Assert.IsTrue(firstAnimVar == secondAnimVar);
            Assert.IsFalse(firstAnimVar != secondAnimVar);
            Assert.IsFalse(firstAnimVar == thirdAnimVar);
            Assert.IsTrue(firstAnimVar != thirdAnimVar);

            Assert.IsTrue(firstAnimVar.Evaluate(CompareOperator.Equals, firstAnimator));
            Assert.IsFalse(firstAnimVar.Evaluate(CompareOperator.Equals, secondAnimator));
            Assert.Throws<ArgumentException>(
                () => firstAnimVar.Evaluate(CompareOperator.GreaterThan, firstAnimator)
            );
        }

        [Test]
        public void AnimatorMuscariable_NullAndDestroyedBehavior()
        {
            var animVar = new AnimatorMuscariable { Key = "anim", ItemID = 245 };
            animVar.Init();

            animVar.Value = null;
            Assert.IsNull(animVar.Value);

            animVar.Value = secondAnimator;
            UnityEngine.Object.DestroyImmediate(secondAnimator);
            Assert.IsTrue(animVar.Value == null);
        }

        [Test]
        public void AnimatorMuscariable_WrongTypeAssignment_Throws()
        {
            Muscariable animVar = new AnimatorMuscariable { Key = "anim", ItemID = 246 };
            animVar.Init();
            Assert.Throws<ArgumentException>(() => animVar.Value = 42);
        }


        protected const float Epsilon = 1e-5f;
    }
}