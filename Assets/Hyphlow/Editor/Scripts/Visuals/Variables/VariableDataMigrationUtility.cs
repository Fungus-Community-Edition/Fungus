using AtMycelia.Hyphlow;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class VariableDataMigrationFiltered
{
    [MenuItem("Tools/Migration/Run VariableData Migration (Prefiltered Dry Run)")]
    public static void DryRun() => RunMigration(dryRun: true);

    [MenuItem("Tools/Migration/Run VariableData Migration (Prefiltered Apply)")]
    public static void Apply() => RunMigration(dryRun: false);

    private static void RunMigration(bool dryRun)
    {
        int scanned = 0, migrated = 0;
        var candidatePaths = GetCandidateAssetPaths();

        foreach (var path in candidatePaths)
        {
            // Load only if text prefilter says it might contain VariableData
            if (!FileContainsVariableData(path))
                continue;

            var mainType = AssetDatabase.GetMainAssetTypeAtPath(path);
            if (mainType == null || (!typeof(GameObject).IsAssignableFrom(mainType) &&
                                     !typeof(ScriptableObject).IsAssignableFrom(mainType)))
                continue;

            var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
            if (!mainAsset) continue;

            scanned++;
            bool changed = false;

            var so = new SerializedObject(mainAsset);
            var sp = so.GetIterator();

            while (sp.NextVisible(true))
            {
                if (sp.propertyType != SerializedPropertyType.ManagedReference) continue;
                if (sp.managedReferenceValue != null) continue;

                var declaredType = GetManagedReferenceFieldType(mainAsset, sp.propertyPath);
                if (declaredType == null || !IsSubclassOfRawGeneric(typeof(VariableData), declaredType) || declaredType.IsAbstract)
                    continue;

                if (dryRun)
                {
                    Debug.Log($"[Dry Run] Would instantiate {declaredType.Name} in {path} :: {mainAsset.name} :: {sp.propertyPath}");
                    continue;
                }

                try
                {
                    var instance = Activator.CreateInstance(declaredType);
                    sp.managedReferenceValue = instance;
                    changed = true;
                    migrated++;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Could not instantiate {declaredType}: {ex.Message}");
                }
            }

            if (changed)
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(mainAsset);
            }
        }

        if (!dryRun) AssetDatabase.SaveAssets();
        Debug.Log($"[VariableData Migration] {(dryRun ? "Dry Run" : "Apply")} — Assets scanned: {scanned}, migrated: {migrated}");
    }

    private static IEnumerable<string> GetCandidateAssetPaths()
    {
        // Limit search to relevant folders
        string[] guids = AssetDatabase.FindAssets("t:Prefab t:ScriptableObject t:Scene",
            new[] { "Assets/FungusExamples",  });
        var result = guids.Select(AssetDatabase.GUIDToAssetPath).Distinct().ToList();
        return result;
    }

    private static bool FileContainsVariableData(string path)
    {
        // Quick text scan for type name before loading
        try
        {
            foreach (var line in File.ReadLines(path))
            {
                if (line.Contains("VariableData") || line.Contains("VariableData`"))
                    return true;
            }
        }
        catch { }
        return false;
    }

    private static Type GetManagedReferenceFieldType(UnityEngine.Object obj, string propertyPath)
    {
        Type type = obj.GetType();
        FieldInfo field = null;

        var tokens = propertyPath.Split('.');
        for (int i = 0; i < tokens.Length; i++)
        {
            string token = tokens[i];
            if (token == "Array")
            {
                i++;
                type = GetIListElementType(type) ?? type.GetElementType() ?? type;
                continue;
            }

            field = GetFieldIncludingBase(type, token);
            if (field == null) continue;
            type = field.FieldType;
        }

        return field?.FieldType;
    }

    private static FieldInfo GetFieldIncludingBase(Type type, string name)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        while (type != null)
        {
            var fi = type.GetField(name, flags);
            if (fi != null) return fi;
            type = type.BaseType;
        }
        return null;
    }

    private static Type GetIListElementType(Type type)
    {
        if (type == null) return null;
        if (type.IsArray) return type.GetElementType();
        if (type.IsGenericType)
        {
            var args = type.GetGenericArguments();
            if (args.Length == 1 && typeof(IList).IsAssignableFrom(type))
                return args[0];

            foreach (var itf in type.GetInterfaces())
            {
                if (itf.IsGenericType && itf.GetGenericTypeDefinition() == typeof(IList<>))
                    return itf.GetGenericArguments()[0];
            }
        }
        return null;
    }

    private static bool IsSubclassOfRawGeneric(Type rawGeneric, Type toCheck)
    {
        while (toCheck != null && toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (rawGeneric == cur) return true;
            toCheck = toCheck.BaseType;
        }
        return false;
    }
}