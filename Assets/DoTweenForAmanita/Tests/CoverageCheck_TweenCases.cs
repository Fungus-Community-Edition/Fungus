using Amanita.ThirdPartyInt.DGDOTween;
using Amanita.Tweening;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[TestFixture]
public class CoverageCheck_TweenCases
{
    [Test]
    public void All_Public_Tween_Methods_Have_TweenCase_Coverage()
    {
        // 1) Collect adapter methods that return ITweenHandle (declared on the adapter)
        var adapterType = typeof(AmaniDoTweenAdapter);
        var tweenMethods = adapterType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(m => typeof(ITweenHandle).IsAssignableFrom(m.ReturnType))
            .Select(m => m.Name)
            .Distinct()
            .ToList();

        // 2) Collect all TweenCase<,>.Name values (static fields/properties) from this test assembly
        var testAssembly = typeof(CoverageCheck_TweenCases).Assembly;
        var tweenCaseNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var type in testAssembly.GetTypes())
        {
            // Fields
            foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (IsTweenCase(field.FieldType) && field.GetValue(null) is object caseObj)
                {
                    var nameProp = caseObj.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.Public);
                    if (nameProp?.GetValue(caseObj) is string name && !string.IsNullOrWhiteSpace(name))
                        tweenCaseNames.Add(name);
                }
            }
            // Properties
            foreach (var prop in type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (IsTweenCase(prop.PropertyType) && prop.GetValue(null) is object caseObj)
                {
                    var nameProp = caseObj.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.Public);
                    if (nameProp?.GetValue(caseObj) is string name && !string.IsNullOrWhiteSpace(name))
                        tweenCaseNames.Add(name);
                }
            }
        }

        // 3) Coverage: a method is covered if ANY case name StartsWith(methodName)
        var missing = tweenMethods
            .Where(methodName => !tweenCaseNames.Any(caseName =>
                caseName.StartsWith(methodName, StringComparison.Ordinal)))
            .OrderBy(n => n)
            .ToList();

        if (missing.Count > 0)
        {
            Assert.Fail("Missing TweenCase coverage for: " + string.Join(", ", missing));
        }
    }

    private static bool IsTweenCase(Type t)
    {
        if (!t.IsGenericType) return false;
        var def = t.GetGenericTypeDefinition();
        // Supports TweenCase<TComponent, TValue> and any future variant that starts with "TweenCase"
        return def.Name.StartsWith("TweenCase", StringComparison.Ordinal);
    }
}