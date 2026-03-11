using AtMycelia.SaveSys.EditorUtils;
using UnityEngine;
using AtMycelia.SaveSys;

public class FakeDropdownController : SaveSysDropdownController
{
    public override ScriptableObject GetInstanceForChoice(string choice, bool isReader)
    {
        return isReader
        ? ScriptableObject.CreateInstance<SaveReader>()
        : ScriptableObject.CreateInstance<SaveWriter>();

    }
}