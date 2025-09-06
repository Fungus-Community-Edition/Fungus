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
    [Test, Ignore(""), Explicit("Run manually when auditing coverage")]
    public void All_Public_Tween_Methods_Have_TweenCase_Coverage()
    {
        var adapterType = typeof(AmaniDoTweenAdapter);

        // All public instance methods declared on the adapter that return ITweenHandle
        var tweenMethods = adapterType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(m => typeof(ITweenHandle).IsAssignableFrom(m.ReturnType))
            .Select(m => m.Name)
            .Distinct()
            .ToList();

        // All TweenCase<,>.Name values from static fields/properties in the test assembly
        var testAssembly = typeof(CoverageCheck_TweenCases).Assembly;
        var tweenCaseNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var type in testAssembly.GetTypes())
        {
            foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (IsTweenCase(field.FieldType) && field.GetValue(null) is object caseObj)
                {
                    var nameProp = caseObj.GetType().GetProperty("Name");
                    if (nameProp?.GetValue(caseObj) is string name && !string.IsNullOrWhiteSpace(name))
                        tweenCaseNames.Add(name);
                }
            }

            foreach (var prop in type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (IsTweenCase(prop.PropertyType) && prop.GetValue(null) is object caseObj)
                {
                    var nameProp = caseObj.GetType().GetProperty("Name");
                    if (nameProp?.GetValue(caseObj) is string name && !string.IsNullOrWhiteSpace(name))
                        tweenCaseNames.Add(name);
                }
            }
        }

        // Loosened match: method is covered if ANY case name contains it (case-insensitive)
        var missing = tweenMethods
            .Where(methodName => !tweenCaseNames.Any(caseName =>
                caseName.IndexOf(methodName, StringComparison.OrdinalIgnoreCase) >= 0))
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
        return def.Name.StartsWith("TweenCase", StringComparison.Ordinal);
    }
}