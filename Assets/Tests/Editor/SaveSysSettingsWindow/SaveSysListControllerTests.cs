using Amanita.SaveSys;
using Amanita.SaveSys.EditorUtils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityObj = UnityEngine.Object;
using System.Linq;


// Helper class for test setup
internal static class _controllerTestHelpers
{
    public static VisualElement MakeAndBindItem<T>(SaveSysListController<T> controller, ListView listView, int index)
    {
        var item = listView.makeItem.Invoke();
        listView.bindItem.Invoke(item, index);
        return item;
    }
}

// Dummy types for testing
public class DummyApplier : ScriptableObject, ISaveDataApplier
{
    public int Order => 0;

    public void Apply() { }

    public Task Apply(SaveData saveData)
    {
        return Task.CompletedTask;
    }

    public Task ApplyRange(IList<SaveData> datas)
    {
        return Task.CompletedTask;
    }

    public bool CanApply(SaveData saveData)
    {
        return false;
    }

    public void PreInstallInit()
    {
       
    }
}

public class DummyCodec : ScriptableObject, IMainSaveCodec
{
    public int Order => 0;

    public object ToMakeFrom
    {
        get => null;
        set { }
    }

    public bool NeedsInput => false;

    public void Encode() { }
    public void Decode() { }

    public void PreInstallInit()
    {
        
    }

    public IList<SaveData> FindAndCreateAll(System.Action<IList<SaveData>> onComplete = null)
    {
        return new List<SaveData>();
    }

    public bool CanHandle(object toMakeFrom)
    {
        return false;
    }

    public bool CanHandle(string typeName)
    {
        return false;
    }
}