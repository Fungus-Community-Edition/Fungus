using AtMycelia.SaveSys;
using AtMycelia.SaveSys.EditorUtils;
using UnityEngine;

public class FakeDropdownController : SaveSysDropdownController
{
    public override ScriptableObject GetInstanceForChoice(string choice, bool isReader)
    {
        return isReader
        ? ScriptableObject.CreateInstance<SaveReader>()
        : ScriptableObject.CreateInstance<SaveWriter>();

    }
}