using UnityEditor;
using UnityEngine;

namespace AtMycelia.Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// A ScriptableObject used to cache editor selection data for the flowchart window.
    /// </summary>
    public class EditorSelectionCache : ScriptableObject
    {
        [SerializeField] private string _lastSelectedFcUid = string.Empty;
        public string LastSelectedFcUid
        {
            get => _lastSelectedFcUid;
            set
            {
                Debug.Log($"Updating LastSelectedFcUid: '{_lastSelectedFcUid}' -> '{value}'");
                _lastSelectedFcUid = value;
            }
        }
    }
}