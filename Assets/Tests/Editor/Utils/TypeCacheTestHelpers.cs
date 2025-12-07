using Amanita.SaveSys;
using Amanita.SaveSys.EditorUtils;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using Type = System.Type;

internal static class TypeCacheTestHelpers
{
    public static void SetApplierChoices(SaveSysSettingsTypeCache cache,
                                         Dictionary<string, ISaveDataApplier> dict)
    {
        var field = cacheType.GetField("_validMainApplierChoices", bindingFlags);
        field.SetValue(cache, dict);
    }

    private static readonly Type cacheType = typeof(SaveSysSettingsTypeCache);
    private static readonly BindingFlags bindingFlags =
        BindingFlags.NonPublic | BindingFlags.Instance;

    public static void SetCodecChoices(SaveSysSettingsTypeCache cache,
                                       Dictionary<string, IMainSaveCodec> dict)
    {
        var field = cacheType.GetField("_validMainCodecChoices", bindingFlags);
        field.SetValue(cache, dict);
    }
}