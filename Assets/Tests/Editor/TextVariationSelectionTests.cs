using NUnit.Framework;
using TextVariationHandler = AtMycelia.Amanita.TextVariationHandler;

namespace DialogueSys
{
    [TestFixture]
    public class TextVariationSelectionTests
    {
        [Test]
        public void SimpleSequenceSelection()
        {
            TextVariationHandler.ClearHistory();

            string startingText = @"This is test [a|b|c]";
            string startingTextA = @"This is test a";
            string startingTextB = @"This is test b";
            string startingTextC = @"This is test c";

            string res = string.Empty;

            res = TextVariationHandler.SelectVariations(startingText);
            Assert.AreEqual(res, startingTextA);
            res = TextVariationHandler.SelectVariations(startingText);
            Assert.AreEqual(res, startingTextB);
            res = TextVariationHandler.SelectVariations(startingText);
            Assert.AreEqual(res, startingTextC);
            res = TextVariationHandler.SelectVariations(startingText);
            Assert.AreEqual(res, startingTextC);
        }

        [Test]
        public void SimpleCycleSelection()
        {
            TextVariationHandler.ClearHistory();

            string startingText = @"This is test [&a|b|c]";
            string startingTextA = @"This is test a";
            string startingTextB = @"This is test b";
            string startingTextC = @"This is test c";

            string res = string.Empty;

            res = TextVariationHandler.SelectVariations(startingText);
            Assert.AreEqual(res, startingTextA);
            res = TextVariationHandler.SelectVariations(startingText);
            Assert.AreEqual(res, startingTextB);
            res = TextVariationHandler.SelectVariations(startingText);
            Assert.AreEqual(res, startingTextC);
            res = TextVariationHandler.SelectVariations(startingText);
            Assert.AreEqual(res, startingTextA);
            res = TextVariationHandler.SelectVariations(startingText);
            Assert.AreEqual(res, startingTextB);
            res = TextVariationHandler.SelectVariations(startingText);
            Assert.AreEqual(res, startingTextC);
        }

        [Test]
        public void SimpleOnceSelection()
        {
            TextVariationHandler.ClearHistory();

            string startingText = @"This is test [!a|b|c]";
            string startingTextA = @"This is test a";
            string startingTextB = @"This is test b";
            string startingTextC = @"This is test c";
            string startingTextD = @"This is test ";

            string res = string.Empty;

            res = TextVariationHandler.SelectVariations(startingText);
            Assert.AreEqual(res, startingTextA);
            res = TextVariationHandler.SelectVariations(startingText);
            Assert.AreEqual(res, startingTextB);
            res = TextVariationHandler.SelectVariations(startingText);
            Assert.AreEqual(res, startingTextC);
            res = TextVariationHandler.SelectVariations(startingText);
            Assert.AreEqual(res, startingTextD);
            res = TextVariationHandler.SelectVariations(startingText);
            Assert.AreEqual(res, startingTextD);
        }

        [Test]
        public void NestedSelection()
        {
            TextVariationHandler.ClearHistory();

            string startingText = @"This is test [a||sub [~a|b]|[!b|[~c|d]]]";
            string startingTextA = @"This is test a";
            string startingTextBlank = @"This is test ";
            string startingTextSubA = @"This is test sub a";
            string startingTextSubB = @"This is test sub b";
            string startingTextB = @"This is test b";
            string startingTextC = @"This is test c";
            string startingTextD = @"This is test d";

            string res = string.Empty;

            res = TextVariationHandler.SelectVariations(startingText);
            Assert.AreEqual(res, startingTextA);
            res = TextVariationHandler.SelectVariations(startingText);
            Assert.AreEqual(res, startingTextBlank);
            res = TextVariationHandler.SelectVariations(startingText);
            if (res != startingTextSubA && res != startingTextSubB)
            {
                Assert.Fail();
            }
            res = TextVariationHandler.SelectVariations(startingText);
            Assert.AreEqual(res, startingTextB);
            res = TextVariationHandler.SelectVariations(startingText);
            if (res != startingTextC && res != startingTextD)
            {
                Assert.Fail();
            }
        }

        [Test]
        public void SquareBracketsWithoutTypeNoImpact()
        {
            TextVariationHandler.ClearHistory();

            string startingText = @"This is test a [of changing nothing]";
            const string expected = @"This is test a [of changing nothing]";

            string res = string.Empty;

            res = TextVariationHandler.SelectVariations(startingText);

            Assert.AreEqual(expected, res);
        }
    }
}