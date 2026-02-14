using UnityEditor;
using Type = System.Type;
using UnityObj = UnityEngine.Object;

namespace Amanita.EditorUtils
{
    public class ClipboardObject
    {
        internal SerializedObject serializedObject;
        internal Type type;

        internal ClipboardObject(UnityObj obj)
        {
            serializedObject = new SerializedObject(obj);
            type = obj.GetType();
        }
    }
}