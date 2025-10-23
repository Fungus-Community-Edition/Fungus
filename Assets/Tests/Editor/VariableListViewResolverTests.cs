using Amanita.EditorUtils;
using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System.Reflection;
using UITKLabel = UnityEngine.UIElements.Label;
using UnityObj = UnityEngine.Object;
using System;

namespace VScriptingTests.VariableOperations
{
    /// <summary>
    /// Tests for VariableListView interacting with IEditorAssetResolver without touching AssetDatabase.
    /// </summary>
    public class VariableListViewResolverTests
    {
        GameObject _host;
        VariableListView _view;
        VariableRowFactory _factory;
        VariableRowPool _rowPool;
        RowVisualHandlerPool _handlerPool;
        RowVisualHandlerResolver _rvResolver;
        FakeEditorAssetResolver _fakeResolver;

        List<UnityObj> _toDestroy = new();

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("VarsHost");

            // Visual handler / factory plumbing (minimal, mirrors existing tests)
            _rvResolver = new RowVisualHandlerResolver();
            _handlerPool = new RowVisualHandlerPool(_rvResolver, RowVisualHandlerRegistry.VisualHandlerLookup);
            _rowPool = new VariableRowPool();

            _factory = new VariableRowFactory();
            var factoryArgs = new VariableRowFactoryInitArgs
            {
                RowPool = _rowPool,
                HandlerPool = _handlerPool,
                Holder = null
            };
            _factory.Init(factoryArgs);

            _fakeResolver = new FakeEditorAssetResolver();

            var listViewArgs = new VariableListViewInitArgs
            {
                List = new ListView(),
                CountLabel = new UITKLabel(),
                RowFactory = _factory,
                AssetResolver = _fakeResolver
            };

            // Use the test subclass that uses the fake resolver to resolve holders
            _view = new TestVariableListView(listViewArgs, ResolveHolder);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var o in _toDestroy)
            {
                if (o != null)
                    UnityObj.DestroyImmediate(o);
            }

            _toDestroy.Clear();

            _view?.Dispose();
            _factory?.Dispose();

            if (_host != null) UnityObj.DestroyImmediate(_host);
        }

        // Minimal concrete Muscariable used only for testing.
        public class TestMuscariable : Muscariable
        {
            public override Type ContentType => typeof(object);
            public override object BoxedValue
            {
                get => val;
                set => val = value;
            }

            object val;
            public TestMuscariable() : base("tm", 1, VariableScope.Private) { }

            public TestMuscariable(string key, int itemID, VariableScope scope) : base(key, itemID, scope) { }

            // Minimal evaluation implementation — sufficient for tests that don't exercise comparisons deeply.
            public override bool Evaluate(CompareOperator compareOperator, object toCompareTo)
            {
                var val = BoxedValue;
                if (val == null && toCompareTo == null) return true;
                if (val == null || toCompareTo == null) return false;
                return val.Equals(toCompareTo);
            }

            public override void Apply(SetOperator setOperator, object toApply)
            {
                // No Op
            }
        }

        // Minimal fake resolver used to inject controlled asset data into VariableListView
        public class FakeEditorAssetResolver : IEditorAssetResolver
        {
            public List<UnityObj> ResourcesAssets { get; set; } = new List<UnityObj>();
            public Dictionary<string, List<UnityObj>> AssetsAtPath { get; } = new Dictionary<string, List<UnityObj>>();
            public Dictionary<UnityObj, string> AssetPathForObject { get; } = new Dictionary<UnityObj, string>();

            public IEnumerable<T> LoadAllFromResources<T>(string path) where T : UnityObj
            {
                return ResourcesAssets.OfType<T>();
            }

            public IEnumerable<T> LoadAllAssetsAtPath<T>(string assetPath) where T : UnityObj
            {
                if (AssetsAtPath.TryGetValue(assetPath, out var list))
                    return list.OfType<T>();
                return Enumerable.Empty<T>();
            }

            public string GetAssetPath(UnityObj obj)
            {
                if (AssetPathForObject.TryGetValue(obj, out var p))
                    return p;
                return string.Empty;
            }

            public void StartAssetEditing() { }
            public void StopAssetEditing() { }
            public void RefreshAssets() { }

            public void AddObjectToAsset(UnityObj objToAdd, UnityObj asset)
            {
                var p = GetAssetPath(asset) ?? string.Empty;
                if (!AssetsAtPath.TryGetValue(p, out var list))
                {
                    list = new List<UnityObj>();
                    AssetsAtPath[p] = list;
                }
                list.Add(objToAdd);
            }
        }

        // Helper used by the TestVariableListView to find a holder for a given IVariable via the fake resolver.
        UnityObj ResolveHolder(IVariable variable)
        {
            foreach (var kv in _fakeResolver.AssetsAtPath)
            {
                foreach (var obj in kv.Value)
                {
                    if (obj is MuscariableHolder mh)
                    {
                        // Match by ItemID (holder.Init/setfrom should set ItemID to musc.ItemID)
                        if (mh.ItemID == variable.ItemID)
                            return mh;
                    }
                }
            }
            return null;
        }

        // Test subclass that overrides binding resolution to use the provided delegate.
        class TestVariableListView : VariableListView
        {
            readonly System.Func<IVariable, UnityObj> _resolverFunc;

            public TestVariableListView(VariableListViewInitArgs initArgs, System.Func<IVariable, UnityObj> resolverFunc)
                : base(initArgs)
            {
                _resolverFunc = resolverFunc;
            }

        }
    }
}