using NUnit.Framework;
using Amanita.VScripting;

namespace Amanita.Tests
{
    [TestFixture]
    public class PrioritySignalsTest
    {
        private int changeCallCount, startCallCount, endCallCount;

        [Test]
        public void CountsAndSignals()
        {
            FungusPrioritySignals.OnFungusPriorityStart += FungusPrioritySignals_OnFungusPriorityStart;
            FungusPrioritySignals.OnFungusPriorityEnd += FungusPrioritySignals_OnFungusPriorityEnd;
            FungusPrioritySignals.OnFungusPriorityChange += FungusPrioritySignals_OnFungusPriorityChange;

            Assert.Zero(FungusPrioritySignals.CurrentPriorityDepth);

            FungusPrioritySignals.DoIncreasePriorityDepth();
            //one start, one change, no end, 1 depth
            Assert.AreEqual(0, endCallCount);
            Assert.AreEqual(1, startCallCount);
            Assert.AreEqual(1, changeCallCount);
            Assert.AreEqual(1, FungusPrioritySignals.CurrentPriorityDepth);

            FungusPrioritySignals.DoIncreasePriorityDepth();
            //one start, 2 change, no end, 2 depth
            Assert.AreEqual(0, endCallCount);
            Assert.AreEqual(1, startCallCount);
            Assert.AreEqual(2, changeCallCount);
            Assert.AreEqual(2, FungusPrioritySignals.CurrentPriorityDepth);

            FungusPrioritySignals.DoIncreasePriorityDepth();
            //one start, 3 change, no end, 3 depth
            Assert.AreEqual(0, endCallCount);
            Assert.AreEqual(1, startCallCount);
            Assert.AreEqual(3, changeCallCount);
            Assert.AreEqual(3, FungusPrioritySignals.CurrentPriorityDepth);

            FungusPrioritySignals.DoDecreasePriorityDepth();
            //one start, 4 change, no end, 2 depth
            Assert.AreEqual(0, endCallCount);
            Assert.AreEqual(1, startCallCount);
            Assert.AreEqual(4, changeCallCount);
            Assert.AreEqual(2, FungusPrioritySignals.CurrentPriorityDepth);

            FungusPrioritySignals.DoDecreasePriorityDepth();
            FungusPrioritySignals.DoDecreasePriorityDepth();
            //one start, 6 change, 1 end, 0 depth
            Assert.AreEqual(1, endCallCount);
            Assert.AreEqual(1, startCallCount);
            Assert.AreEqual(6, changeCallCount);
            Assert.AreEqual(0, FungusPrioritySignals.CurrentPriorityDepth);

            FungusPrioritySignals.OnFungusPriorityStart -= FungusPrioritySignals_OnFungusPriorityStart;
            FungusPrioritySignals.OnFungusPriorityEnd -= FungusPrioritySignals_OnFungusPriorityEnd;
            FungusPrioritySignals.OnFungusPriorityChange -= FungusPrioritySignals_OnFungusPriorityChange;

            //unsubbed so all the same
            FungusPrioritySignals.DoIncreasePriorityDepth();
            FungusPrioritySignals.DoDecreasePriorityDepth();
            //one start, 6 change, 1 end, 0 depth
            Assert.AreEqual(1, endCallCount);
            Assert.AreEqual(1, startCallCount);
            Assert.AreEqual(6, changeCallCount);
            Assert.AreEqual(0, FungusPrioritySignals.CurrentPriorityDepth);
        }

        private void FungusPrioritySignals_OnFungusPriorityChange(int previousActiveDepth, int newActiveDepth)
        {
            changeCallCount++;
        }

        private void FungusPrioritySignals_OnFungusPriorityEnd()
        {
            endCallCount++;
        }

        private void FungusPrioritySignals_OnFungusPriorityStart()
        {
            startCallCount++;
        }
    }
}