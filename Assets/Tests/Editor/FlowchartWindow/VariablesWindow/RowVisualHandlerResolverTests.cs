using AtMycelia.Hyphlow.EditorUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using DefaultHandler = AtMycelia.Hyphlow.EditorUtils.DefaultRowVisualHandler;

namespace VScriptingTests.VariableOperations
{
    [TestFixture]
    public class RowVisualHandlerResolverTests
    {
        private RowVisualHandlerResolver _resolver;
        private Dictionary<Type, Type> _lookup;

        [SetUp]
        public void SetUp()
        {
            _resolver = new RowVisualHandlerResolver();
            _lookup = new Dictionary<Type, Type>
            {
                { typeof(Animal),  typeof(AnimalHandler)  },
                { typeof(Mammal),  typeof(MammalHandler)  },
                { typeof(Dog),     typeof(DogHandler)     },
                { typeof(object),  typeof(DefaultHandler) }
            };
        }

        [Test]
        public void ResolveHandler_ExactMatch_ReturnsMappedHandler()
        {
            var handlerType = _resolver.ResolveHandler(_lookup, typeof(Dog));
            Assert.AreEqual(typeof(DogHandler), handlerType);
        }

        [Test]
        public void ResolveHandler_InheritanceClosestBaseType_PicksNearestAncestor()
        {
            // Remove exact Dog mapping to force inheritance lookup
            _lookup.Remove(typeof(Dog));

            // Dog → Mammal is closer than Dog → Animal
            var handlerType = _resolver.ResolveHandler(_lookup, typeof(Dog));
            Assert.AreEqual(typeof(MammalHandler), handlerType);
        }

        [Test]
        public void ResolveHandler_GenericFallback_WhenOnlyObjectRegistered()
        {
            // Clear everything except object
            _lookup.Clear();
            _lookup[typeof(object)] = typeof(DefaultHandler);

            var handlerType = _resolver.ResolveHandler(_lookup, typeof(Reptile));
            Assert.AreEqual(typeof(DefaultHandler), handlerType);
        }

        [Test]
        public void ResolveHandler_Throws_WhenNoMappingAndNoGenericFallback()
        {
            // Remove the object fallback
            _lookup.Remove(typeof(object));
            _lookup.Remove(typeof(Animal));
            _lookup.Remove(typeof(Mammal));
            _lookup.Remove(typeof(Dog));

            Assert.Throws<InvalidOperationException>(() =>
                _resolver.ResolveHandler(_lookup, typeof(Reptile))
            );
        }

        [Test]
        public void ResolveHandler_ChoosesAnimalOverObject_WhenBothAssignable()
        {
            // Register only Animal and object
            _lookup.Clear();
            _lookup[typeof(Animal)] = typeof(AnimalHandler);
            _lookup[typeof(object)] = typeof(DefaultHandler);

            // Mammal inherits from Animal
            var handlerType = _resolver.ResolveHandler(_lookup, typeof(Mammal));
            Assert.AreEqual(typeof(AnimalHandler), handlerType);
        }
    }
}