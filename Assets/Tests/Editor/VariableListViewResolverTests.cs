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

namespace VariableOperations
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

        [Test]
        public void ForceMaterializeAllRowsForTests_UsesResolver_FindHolder_And_AssignsSerializedObject()
        {
            // Arrange: create a VariableSourceAsset and a TestMuscariable + holder.
            var vsa = ScriptableObject.CreateInstance<VariableSourceAsset>();
            // Muscariables are plain C# objects (not ScriptableObjects) — construct directly
            var musc = new TestMuscariable();
            musc.Key = "tm";
            // Keep references for cleanup (only UnityEngine.Object instances)
            _toDestroy.Add(vsa);

            // Create a MuscariableHolder and initialize it to point at our musc instance.
            var holder = ScriptableObject.CreateInstance<MuscariableHolder>();
            _toDestroy.Add(holder);
            TryInitHolder(holder, musc);

            // Configure fake resolver to return our VariableSourceAsset and the holder at a fake path.
            var fakePath = "Assets/Fake/path.asset";
            _fakeResolver.ResourcesAssets = new List<UnityObj> { vsa };
            _fakeResolver.AssetPathForObject[vsa] = fakePath;
            _fakeResolver.AssetsAtPath[fakePath] = new List<UnityObj> { holder };

            // Set the VariableSource context (so FindPersistentHolderFor will query resolver)
            var listViewArgs = new VariableListViewInitArgs
            {
                List = new ListView(),
                CountLabel = new UITKLabel(),
                RowFactory = _factory,
                VariableSource = vsa,
                AssetResolver = _fakeResolver
            };

            // Recreate view with VariableSource set (ensures _variableSourceContext is assigned)
            _view.Dispose();
            _view = new TestVariableListView(listViewArgs, ResolveHolder);

            // Add the musc variable to the view and materialize
            _view.AddVariable(musc);
            // Precondition: row should not be materialized yet
            Assert.IsNull(_view.RowAtIndex(0), "Row should not be materialized before ForceMaterializeAllRowsForTests.");

            // Act
            _view.ForceMaterializeAllRowsForTests();

            // Assert: row materialized and handler has a SerializedObject assigned (from the holder)
            var row = _view.RowAtIndex(0);
            Assert.NotNull(row, "Expected a materialized VariableRow for the musc variable.");
            Assert.NotNull(row.VisualHandler, "Row should have a visual handler.");
            Assert.NotNull(row.VisualHandler.SerializedVar, "Visual handler should have a SerializedObject assigned.");

            // Additionally verify the SerializedObject targets our holder instance
            Assert.AreSame(holder, row.VisualHandler.SerializedVar.targetObject,
                "SerializedObject target should be the MuscariableHolder returned by the resolver.");
        }

        // Robust reflection helper: prefer an Init/SetFrom overload that accepts the variable,
        // avoid AmbiguousMatchException by enumerating overloads and selecting the best fit.
        static void TryInitHolder(MuscariableHolder holder, object inner)
        {
            var t = holder.GetType();
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            // Try various Init overloads: prefer single-parameter overload compatible with 'inner'
            var initCandidates = t.GetMethods(flags).Where(m => m.Name == "Init").ToArray();
            MethodInfo chosen = null;

            if (initCandidates.Length > 0)
            {
                // Prefer single-parameter overload assignable from inner's type (or IVariable/object)
                foreach (var m in initCandidates)
                {
                    var ps = m.GetParameters();
                    if (ps.Length == 1)
                    {
                        var pType = ps[0].ParameterType;
                        if (inner != null)
                        {
                            var innerType = inner.GetType();
                            if (pType.IsAssignableFrom(innerType) ||
                                pType == typeof(object) ||
                                pType == typeof(IVariable))
                            {
                                chosen = m;
                                break;
                            }
                        }
                        else
                        {
                            // inner is null — accept any single-parameter Init
                            chosen = m;
                            break;
                        }
                    }
                }

                // Fallback to parameterless Init if we didn't find a single-param match
                if (chosen == null)
                    chosen = initCandidates.FirstOrDefault(m => m.GetParameters().Length == 0);

                if (chosen != null)
                {
                    var ps = chosen.GetParameters();
                    if (ps.Length == 0)
                        chosen.Invoke(holder, null);
                    else
                        chosen.Invoke(holder, new object[] { inner });
                    return;
                }
            }

            // Try SetFrom(...) overloads the same way
            var setFromCandidates = t.GetMethods(flags).Where(m => m.Name == "SetFrom").ToArray();
            if (setFromCandidates.Length > 0)
            {
                foreach (var m in setFromCandidates)
                {
                    var ps = m.GetParameters();
                    if (ps.Length == 1)
                    {
                        var pType = ps[0].ParameterType;
                        if (inner != null)
                        {
                            var innerType = inner.GetType();
                            if (pType.IsAssignableFrom(innerType) ||
                                pType == typeof(object) ||
                                pType == typeof(IVariable))
                            {
                                chosen = m;
                                break;
                            }
                        }
                        else
                        {
                            chosen = m;
                            break;
                        }
                    }
                }

                if (chosen == null)
                    chosen = setFromCandidates.FirstOrDefault(m => m.GetParameters().Length == 1);

                if (chosen != null)
                {
                    chosen.Invoke(holder, new object[] { inner as IVariable });
                    return;
                }
            }

            // Fallback: try property/field named Inner (existing behavior)
            var pi = t.GetProperty("Inner", flags);
            if (pi != null && pi.CanWrite)
            {
                pi.SetValue(holder, inner);
                return;
            }

            var fi = t.GetField("inner", flags) ?? t.GetField("_inner", flags);
            if (fi != null)
            {
                fi.SetValue(holder, inner);
            }
        }

        // Minimal concrete Muscariable used only for testing.
        public class TestMuscariable : Muscariable
        {
            public TestMuscariable() : base("tm", 1, VariableScope.Private) { }

            public TestMuscariable(string key, int itemID, VariableScope scope) : base(key, itemID, scope) { }

            // Minimal evaluation implementation — sufficient for tests that don't exercise comparisons deeply.
            public override bool Evaluate(CompareOperator compareOperator, object toCompareTo)
            {
                var val = Value;
                if (val == null && toCompareTo == null) return true;
                if (val == null || toCompareTo == null) return false;
                return val.Equals(toCompareTo);
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

            // The production code made GetBindingTarget virtual — use the delegate to resolve holders.
            protected override UnityObj GetBindingTarget(IVariable variable)
            {
                var fromDelegate = _resolverFunc?.Invoke(variable);
                if (fromDelegate != null) return fromDelegate;
                return base.GetBindingTarget(variable);
            }
        }
    }
}