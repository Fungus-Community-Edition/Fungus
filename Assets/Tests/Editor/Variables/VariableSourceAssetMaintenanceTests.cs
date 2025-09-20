using Amanita.EditorUtils;
using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using UnityObj = UnityEngine.Object;

namespace Amanita.Tests.EditMode
{
    /// <summary>
    /// Unit tests for VariableSourceAssetMaintenance. These tests replace the real
    /// AssetResolver with a fake one to avoid touching AssetDatabase/Resources.
    /// </summary>
    public class VariableSourceAssetMaintenanceTests
    {
        List<UnityObj> _toDestroy;
        FakeEditorAssetResolver _fakeResolver;

        [SetUp]
        public void SetUp()
        {
            _toDestroy = new List<UnityObj>();
            _fakeResolver = new FakeEditorAssetResolver();

            // Inject the fake resolver into the static maintenance class via reflection.
            var pi = typeof(VariableSourceAssetMaintenance).GetProperty(
                "AssetResolver",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(pi, "Could not find AssetResolver property on VariableSourceAssetMaintenance");
            pi.SetValue(null, _fakeResolver);
        }

        [TearDown]
        public void TearDown()
        {
            // Restore default resolver to avoid leaking test state
            var pi = typeof(VariableSourceAssetMaintenance).GetProperty(
                "AssetResolver",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (pi != null)
            {
                pi.SetValue(null, new DefaultEditorAssetResolver());
            }

            foreach (var o in _toDestroy)
                if (o != null)
                    UnityObj.DestroyImmediate(o);

            _toDestroy.Clear();
        }

        [Test]
        public void Refresh_RelinksHoldersToMuscariables()
        {
            // Arrange: create VariableSourceAsset, two muscariables, and two holders.
            var vsa = ScriptableObject.CreateInstance<VariableSourceAsset>();
            _toDestroy.Add(vsa);

            var m1 = new TestMuscariable("one", 111, VariableScope.Private);
            var m2 = new TestMuscariable("two", 222, VariableScope.Private);

            // Attach the muscariable list onto the asset (via reflection to be robust)
            SetAssetVariables(vsa, new List<IVariable> { m1, m2 });

            // Create holders that simulate serialized sub-assets; ensure ItemID matches
            var holder1 = ScriptableObject.CreateInstance<MuscariableHolder>();
            var holder2 = ScriptableObject.CreateInstance<MuscariableHolder>();
            _toDestroy.Add(holder1);
            _toDestroy.Add(holder2);

            // Give each holder a lightweight internal muscariable with the ItemID we expect.
            // We inject the muscariable into the private 'muscariable' field to avoid calling
            // the ItemID property (which would call Ensure() and create other side effects).
            InjectInnerMuscariableWithItemId(holder1, 111, "one");
            InjectInnerMuscariableWithItemId(holder2, 222, "two");

            // Precondition: holders were injected with inner muscariables with expected ItemIDs
            var preInner1 = GetHolderInner(holder1);
            var preInner2 = GetHolderInner(holder2);
            Assert.NotNull(preInner1, "Precondition: holder1 should have an inner muscariable after injection");
            Assert.NotNull(preInner2, "Precondition: holder2 should have an inner muscariable after injection");
            Assert.AreEqual(111, ((IVariable)preInner1).ItemID, "Precondition: holder1 inner ItemID mismatch");
            Assert.AreEqual(222, ((IVariable)preInner2).ItemID, "Precondition: holder2 inner ItemID mismatch");

            // Configure fake resolver to return our asset and the holders at a fake path
            var fakePath = "Assets/Fake/vsa.asset";
            _fakeResolver.ResourcesAssets = new List<UnityObj> { vsa };
            _fakeResolver.AssetPathForObject[vsa] = fakePath;
            _fakeResolver.AssetsAtPath[fakePath] = new List<UnityObj> { holder1, holder2 };

            // Act: call the private RefreshVariableSourceAssets via reflection
            var mi = typeof(VariableSourceAssetMaintenance).GetMethod(
                "RefreshVariableSourceAssets",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(mi, "Could not find RefreshVariableSourceAssets method");
            mi.Invoke(null, null);

            // Assert: holders have been initialized/linked to the corresponding muscariables
            var inner1 = GetHolderInner(holder1);
            var inner2 = GetHolderInner(holder2);

            Assert.NotNull(inner1, "Holder1 should have been linked to a muscariable");
            Assert.NotNull(inner2, "Holder2 should have been linked to a muscariable");

            Assert.IsInstanceOf<IVariable>(inner1);
            Assert.IsInstanceOf<IVariable>(inner2);

            Assert.AreEqual(111, ((IVariable)inner1).ItemID);
            Assert.AreEqual(222, ((IVariable)inner2).ItemID);
        }

        [Test]
        public void OnRightBeforeAnyAssetAddVariable_AddsHolderUnderAsset()
        {
            // Arrange: create VariableSourceAsset and muscariable whose Owner is that asset
            var vsa = ScriptableObject.CreateInstance<VariableSourceAsset>();
            _toDestroy.Add(vsa);

            var musc = new TestMuscariable("x", 555, VariableScope.Private);
            musc.Owner = vsa;

            // Ensure resolver knows the asset path so AddObjectToAsset stores under that key
            var fakePath = "Assets/Fake/vsa2.asset";
            _fakeResolver.AssetPathForObject[vsa] = fakePath;

            // Act: invoke private OnRightBeforeAnyAssetAddVariable via reflection
            var mi = typeof(VariableSourceAssetMaintenance).GetMethod(
                "OnRightBeforeAnyAssetAddVariable",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(mi, "Could not find OnRightBeforeAnyAssetAddVariable method");
            mi.Invoke(null, new object[] { musc });

            // Assert: the fake resolver recorded a holder under the fake path
            Assert.IsTrue(_fakeResolver.AssetsAtPath.ContainsKey(fakePath), "Resolver did not record added objects");
            var list = _fakeResolver.AssetsAtPath[fakePath];
            Assert.IsNotEmpty(list, "No objects added to the asset by AddObjectToAsset");

            // The last added should be a MuscariableHolder linked to our musc
            var addedHolder = list.Last() as MuscariableHolder;
            Assert.NotNull(addedHolder, "Added object was not a MuscariableHolder");

            // Holder should have been initialized with the musc variable
            var inner = GetHolderInner(addedHolder);
            Assert.NotNull(inner, "Holder was not initialized with the muscariable");
            Assert.IsInstanceOf<IVariable>(inner);
            Assert.AreEqual(555, ((IVariable)inner).ItemID);
        }

        // -------------------------
        // Helpers and test doubles
        // -------------------------

        static void SetAssetVariables(VariableSourceAsset asset, List<IVariable> vars)
        {
            var t = asset.GetType();
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            // Prefer property 'Variables'
            var pi = t.GetProperty("Variables", flags);
            if (pi != null && pi.CanWrite)
            {
                var propType = pi.PropertyType;

                // If direct assignable, do it
                if (propType.IsAssignableFrom(vars.GetType()))
                {
                    pi.SetValue(asset, vars);
                    return;
                }

                // If it's a generic collection, attempt to create a concrete List<Elem> and populate it
                if (propType.IsGenericType)
                {
                    var elemType = propType.GetGenericArguments()[0];
                    var concreteListType = typeof(List<>).MakeGenericType(elemType);
                    var listInstance = Activator.CreateInstance(concreteListType);
                    var addMethod = concreteListType.GetMethod("Add", new[] { elemType });

                    foreach (var v in vars)
                    {
                        object toAdd = v;
                        // If target element type expects Muscariable and v is IVariable but actually Muscariable, cast
                        if (!elemType.IsAssignableFrom(v.GetType()))
                        {
                            if (v is Muscariable m && elemType.IsAssignableFrom(m.GetType()))
                                toAdd = m;
                            else
                                toAdd = Convert.ChangeType(v, elemType);
                        }

                        addMethod.Invoke(listInstance, new[] { toAdd });
                    }

                    // If property accepts the concrete list or an interface implemented by it, assign
                    pi.SetValue(asset, listInstance);
                    return;
                }

                // Last resort: try to set (will throw if incompatible)
                pi.SetValue(asset, vars);
                return;
            }

            // Fallback to field named 'variables', 'Variables', or 'legacyVariables'
            var fi = t.GetField("variables", flags) ??
                     t.GetField("Variables", flags) ??
                     t.GetField("legacyVariables", flags);
            if (fi != null)
            {
                var fieldType = fi.FieldType;

                if (fieldType.IsAssignableFrom(vars.GetType()))
                {
                    fi.SetValue(asset, vars);
                    return;
                }

                if (fieldType.IsGenericType)
                {
                    var elemType = fieldType.GetGenericArguments()[0];
                    var concreteListType = typeof(List<>).MakeGenericType(elemType);
                    var listInstance = Activator.CreateInstance(concreteListType);
                    var addMethod = concreteListType.GetMethod("Add", new[] { elemType });

                    foreach (var v in vars)
                    {
                        object toAdd = v;
                        if (!elemType.IsAssignableFrom(v.GetType()))
                        {
                            if (v is Muscariable m && elemType.IsAssignableFrom(m.GetType()))
                                toAdd = m;
                            else
                                toAdd = Convert.ChangeType(v, elemType);
                        }
                        addMethod.Invoke(listInstance, new[] { toAdd });
                    }

                    fi.SetValue(asset, listInstance);
                    return;
                }

                fi.SetValue(asset, vars);
                return;
            }

            Assert.Fail("Could not find a Variables member to set on VariableSourceAsset. Reflection names tried: Variables, variables, legacyVariables.");
        }

        static object GetHolderInner(MuscariableHolder holder)
        {
            var t = holder.GetType();
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            // Try property named 'Inner'
            var pi = t.GetProperty("Inner", flags);
            if (pi != null && pi.CanRead)
                return pi.GetValue(holder);

            // Try field named 'inner' or '_inner'
            var fi = t.GetField("inner", flags) ?? t.GetField("_inner", flags);
            if (fi != null)
                return fi.GetValue(holder);

            // Try method 'GetInner' or 'Get'
            var mi = t.GetMethod("GetInner", flags) ?? t.GetMethod("Get", flags);
            if (mi != null)
                return mi.Invoke(holder, null);

            return null;
        }

        static void SetMemberIfExists(object target, string memberName, object value)
        {
            var t = target.GetType();
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var pi = t.GetProperty(memberName, flags);
            if (pi != null && pi.CanWrite)
            {
                pi.SetValue(target, value);
                return;
            }
            var fi = t.GetField(memberName, flags);
            if (fi != null)
            {
                fi.SetValue(target, value);
            }
        }

        static void InjectInnerMuscariableWithItemId(MuscariableHolder holder, int itemId, string key = "")
        {
            // Create a concrete GenericMuscariable and set identifying fields
            var inner = new GenericMuscariable();
            inner.Key = key;
            inner.ItemID = itemId;

            // Inject into the protected/serialized field named 'muscariable'
            var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var fi = holder.GetType().GetField("muscariable", flags);
            if (fi != null)
            {
                fi.SetValue(holder, inner);
                return;
            }

            // If the backing field has a different name, try common alternatives
            fi = holder.GetType().GetField("_muscariable", flags) ?? holder.GetType().GetField("inner", flags);
            if (fi != null)
            {
                fi.SetValue(holder, inner);
            }
        }

        // Minimal concrete Muscariable used only for testing.
        public class TestMuscariable : Muscariable
        {
            public TestMuscariable(string key, int itemID, VariableScope scope) : base(key, itemID, scope)
            {
                // Ensure ContentType is non-null so VariableFactory.Create and related logic won't NRE
            }

            public override Type ContentType => typeof(object);

            // Minimal evaluation implementation — sufficient for these tests.
            public override bool Evaluate(CompareOperator compareOperator, object toCompareTo)
            {
                var val = Value;
                if (val == null && toCompareTo == null) return true;
                if (val == null || toCompareTo == null) return false;
                return val.Equals(toCompareTo);
            }
        }

        // Fake resolver that stores and returns test-provided assets without calling AssetDatabase/Resources.
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

            public void RefreshAssets() { }
        }
    }
}