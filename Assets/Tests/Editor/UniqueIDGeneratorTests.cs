using Amanita.VScripting;
using NUnit.Framework;
using System.Collections.Generic;

namespace Amanita.Tests.EditMode
{
    [TestFixture]
    public class UniqueIDGeneratorTests
    {
        [Test]
        public void NoOthers_Returns_HighestSoFarPlusOne()
        {
            var target = new IntMuscariable { ItemID = 100 };
            IList<IHasItemID> others = new List<IHasItemID>();
            int highestSoFar = 5;

            int result = UniqueIDGenerator.GetUniqueIDFor(target, others, highestSoFar);

            Assert.AreEqual(highestSoFar + 1, result);
        }

        [Test]
        public void OthersContainHigher_Returns_HighestAmongOthersPlusOne()
        {
            var target = new IntMuscariable { ItemID = 3 };
            IList<IHasItemID> others = new List<IHasItemID>
            {
                new IntMuscariable { ItemID = 7 },
                new FloatMuscariable { ItemID = 10 },
                new DoubleMuscariable { ItemID = 2 }
            };
            int highestSoFar = 5;

            int result = UniqueIDGenerator.GetUniqueIDFor(target, others, highestSoFar);

            // highest among others is 10 -> expect 11
            Assert.AreEqual(11, result);
        }

        [Test]
        public void TargetHasHighest_Returns_TargetItemIDPlusOne()
        {
            var target = new IntMuscariable { ItemID = 20 };
            IList<IHasItemID> others = new List<IHasItemID>
            {
                new IntMuscariable { ItemID = 7 },
                new FloatMuscariable { ItemID = 10 }
            };
            int highestSoFar = 5;

            int result = UniqueIDGenerator.GetUniqueIDFor(target, others, highestSoFar);

            Assert.AreEqual(21, result);
        }

        [Test]
        public void HighestSoFarDominates_WhenLargest_Returns_HighestSoFarPlusOne()
        {
            var target = new IntMuscariable { ItemID = 7 };
            IList<IHasItemID> others = new List<IHasItemID>
            {
                new IntMuscariable { ItemID = 4 },
                new FloatMuscariable { ItemID = 9 }
            };
            int highestSoFar = 30;

            int result = UniqueIDGenerator.GetUniqueIDFor(target, others, highestSoFar);

            Assert.AreEqual(31, result);
        }
    }
}