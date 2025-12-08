using Amanita.SaveSys;
using UnityEngine;

[System.Serializable]
public class RawStringSaveData : SaveData
{
    [SerializeField] public string Value;

    public RawStringSaveData() { }
    public RawStringSaveData(string value) { Value = value; }

}

[System.Serializable]
public class IndexSaveData : SaveData
{
    [SerializeField] public int index;

    public IndexSaveData() { }
    public IndexSaveData(int value) { index = value; }

}