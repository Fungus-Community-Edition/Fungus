using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

public static class VariableDataMigrationUtility
{
    [MenuItem("Tools/Migrate VariableData Nulls")]
    public static void MigrateAllVariableData()
    {
        string[] assetPaths = AssetDatabase.GetAllAssetPaths();
        int fixedCount = 0;

        foreach (string path in assetPaths)
        {
            if (!path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) &&
                !path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase) &&
                !path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                continue;

            UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset == null) continue;

            bool modified = false;

            // Handle main asset
            modified |= FixObject(asset);

            // Handle sub-assets (e.g., ScriptableObjects inside .asset files)
            UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var sub in subAssets)
            {
                if (sub != null && sub != asset)
                    modified |= FixObject(sub);
            }

            if (modified)
            {
                EditorUtility.SetDirty(asset);
                fixedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"VariableData migration complete. Fixed {fixedCount} assets.");
    }

    private static bool FixObject(UnityEngine.Object obj)
    {
        bool modified = false;
        var so = new SerializedObject(obj);
        var sp = so.GetIterator();

        while (sp.NextVisible(true))
        {
            if (sp.propertyType == SerializedPropertyType.ManagedReference && sp.managedReferenceValue == null)
            {
                var fieldType = GetManagedReferenceFieldType(obj, sp.propertyPath);
                if (fieldType != null && typeof(object).IsAssignableFrom(fieldType) && fieldType.Name.EndsWith("Data"))
                {
                    try
                    {
                        var instance = Activator.CreateInstance(fieldType);
                        sp.managedReferenceValue = instance;
                        modified = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Could not instantiate {fieldType}: {ex.Message}");
                    }
                }
            }
        }

        if (modified) so.ApplyModifiedProperties();
        return modified;
    }

    private static Type GetManagedReferenceFieldType(UnityEngine.Object obj, string propertyPath)
    {
        Type objType = obj.GetType();
        FieldInfo field = null;
        foreach (var part in propertyPath.Split('.'))
        {
            field = objType.GetField(part, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field == null) break;
            objType = field.FieldType;
        }
        return field?.FieldType;
    }
}