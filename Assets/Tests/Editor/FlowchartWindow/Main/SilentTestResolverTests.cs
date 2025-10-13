using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace VScriptingTests.VariableOperations
{
    // Dummy handler types for testing
    public class GoodHandler { }
    public class GenericHandler { }

    // Sample content types for testing
    public class SomeContentType { }
    public class OtherContentType { }

    [TestFixture]
    public class SilentTestResolverTests
    {
        [TestCase(typeof(SomeContentType), typeof(FakeHandlerWithBadPath), false, TestName = "Exact match → FakeHandlerWithBadPath should be excluded")]
        [TestCase(typeof(OtherContentType), typeof(GoodHandler), true, TestName = "Exact match → GoodHandler should be returned")]
        [TestCase(typeof(string), typeof(GenericHandler), true, TestName = "No match → should return generic handler")]
        public void ResolveHandler_BehavesAsExpected(Type queryType, Type expectedHandler, bool shouldMatch)
        {
            var resolver = new SilentTestResolver();

            var visualHandlerLookup = new Dictionary<Type, Type>
        {
            { typeof(SomeContentType), typeof(FakeHandlerWithBadPath) }, // should be excluded
            { typeof(OtherContentType), typeof(GoodHandler) },           // valid handler
            { typeof(object), typeof(GenericHandler) }                   // fallback
        };

            var handler = resolver.ResolveHandler(visualHandlerLookup, queryType);

            if (shouldMatch)
                Assert.AreEqual(expectedHandler, handler);
            else
                Assert.AreNotEqual(expectedHandler, handler);
        }
    }
}