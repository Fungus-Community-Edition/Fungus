using Amanita.SaveSys;
using UnityEngine;
using Amanita.FSExt;

[System.Serializable]
public class RawStringSaveData : SaveData
{
    [SerializeField] public string Value;

    public RawStringSaveData() { }
    public RawStringSaveData(string value) { Value = value; }

    public override SaveDataUnit Serialized()
    {
        var json = SaveSystem.DefaultSerializer.ToJson(this, true);
        return new SaveDataUnit(GetType().Name, json);
    }
}

[System.Serializable]
public class IndexSaveData : SaveData
{
    [SerializeField] public int index;

    public IndexSaveData() { }
    public IndexSaveData(int value) { index = value; }

    public override SaveDataUnit Serialized()
    {
        var json = SaveSystem.DefaultSerializer.ToJson(this, true);
        return new SaveDataUnit(GetType().Name, json);
    }
}