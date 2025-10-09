using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Amanita.SaveSys
{
    [DisallowMultipleComponent]
    public class SaveIdentifier : MonoBehaviour
    {
        [SerializeField] private string uniqueID = "";

        public string UniqueID => uniqueID;

        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(uniqueID))
            {
                uniqueID = latestValidGenerated = GenerateID();
                //uniqueID = theOneGeneratedOnAwake;
            }
            else
            {
                latestValidGenerated = uniqueID;
            }
        }

        private void Reset()
        {
            if (latestValidGenerated.Length == 0)
            {
                uniqueID = latestValidGenerated = GenerateID();
                //uniqueID = theOneGeneratedOnAwake;
            }
        }

        protected string latestValidGenerated = string.Empty;

        private string GenerateID()
        {
            string result = System.Guid.NewGuid().ToString();

#if UNITY_EDITOR
            EditorUtility.SetDirty(this); // Marks object as changed in the editor
#endif
            return result;
        }

        public virtual void GetSelfNewID()
        {
            uniqueID = latestValidGenerated = GenerateID();
        }
    }
}