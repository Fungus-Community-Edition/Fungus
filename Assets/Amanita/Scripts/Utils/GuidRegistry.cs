using Amanita.VScripting;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Amanita
{
    [CreateAssetMenu(menuName = "Atelier Mycelia/GuidRegistry")]
    public class GuidRegistry : ScriptableObject
    {
        [SerializeField] protected List<string> typesStoredFor = new List<string>();
        [SerializeField] protected List<string> guids = new List<string>();
        protected Dictionary<string, int> guidToNumericId;
        protected Dictionary<int, string> numericIdToGuid;

        public virtual IReadOnlyList<string> TypesStoredFor => typesStoredFor.AsReadOnly();

        public virtual void AddTypeStoredFor<T>()
        {
            var typeName = typeof(T).FullName;
            if (!typesStoredFor.Contains(typeName))
            {
                typesStoredFor.Add(typeName);
            }
        }

        public virtual void RemoveTypeStoredFor<T>()
        {
            var typeName = typeof(T).FullName;
            if (typesStoredFor.Contains(typeName))
            {
                typesStoredFor.Remove(typeName);
            }
        }

        protected virtual void OnEnable()
        {
            Refresh();
            ToggleSubs(false); 
            // ^Since Unity's serialization can get a bit finicky, we toggle subs off then on to ensure
            // we don't double-subscribe.
            ToggleSubs(true);
        }

        public virtual void Refresh() // This is public so we can ensure instances have their stuff ready.
        {
            BuildDictionaries();
        }

        protected virtual void BuildDictionaries(bool forceRebuild = false)
        {
            bool alreadyBuilt = guidToNumericId != null && numericIdToGuid != null;
            if (alreadyBuilt && !forceRebuild)
            {
                return;
            }

            guidToNumericId = new Dictionary<string, int>();
            numericIdToGuid = new Dictionary<int, string>();

            for (int i = 0; i < guids.Count; i++)
            {
                guidToNumericId[guids[i]] = i;
                numericIdToGuid[i] = guids[i];
            }
        }

        protected virtual void ToggleSubs(bool on)
        {
            
        }

        public virtual void RegisterUidOf(IHasUniqueID uidHaver)
        {
            if (!StoresForType(uidHaver.GetType().FullName))
            {
                return;
            }

            // Since this callback might fire between domain reloads, we need to be extra careful
            // about our dictionaries being built.
            Refresh();
            if (!guids.Contains(uidHaver.UniqueId))
            {
                Debug.Log($"[{name}]: Registering unique ID {uidHaver.UniqueId} for {uidHaver}");
                GetOrAddNumericId(uidHaver.UniqueId);
            }
        }

        public virtual bool StoresForType(string typeName)
        {
            for (int i = 0; i < typesStoredFor.Count; i++)
            {
                if (typesStoredFor[i].Equals(typeName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// If the guid is not registered, returns InvalidNumericId.
        /// </summary>
        public virtual int GetNumericId(string guid)
        {
            Refresh();
            if (guid == null)
            {
                return InvalidNumericId;
            }
            bool valueFound = guidToNumericId.TryGetValue(guid, out int id);
            if (valueFound)
            {
                return id;
            }
            return InvalidNumericId;
        }

        public static readonly int InvalidNumericId = -1;

        public virtual int GetOrAddNumericId(string guid)
        {
            if (guidToNumericId.TryGetValue(guid, out int id))
            {
                return id;
            }

            // Add new GUID
            int newId = guids.Count;
            guids.Add(guid);
            guidToNumericId[guid] = newId;
            numericIdToGuid[newId] = guid;
            return newId;
        }

        public virtual string GetGuid(int numericId)
        {
            return numericIdToGuid.TryGetValue(numericId, out var guid) ? guid : null;
        }

        public virtual bool HasIndexTiedTo(string guid)
        {
            return guidToNumericId.ContainsKey(guid);
        }

        /// <summary>
        /// Removes the GUID and its associated numeric ID from the registry. Use with caution! This might
        /// mess with the indices of other GUIDs.
        /// </summary>
        public virtual void RemoveGuid(string guid)
        {
            if (guidToNumericId.TryGetValue(guid, out int id))
            {
                guidToNumericId.Remove(guid);
                numericIdToGuid.Remove(id);
                guids.RemoveAt(id);
                // Rebuild dictionaries to ensure indices are correct
                BuildDictionaries(true);
            }
        }

        /// <summary>
        /// Removes the GUID associated with the given numeric ID from the registry. Use with caution! This might
        /// mess with the indices of other GUIDs.
        /// </summary>
        public virtual void RemoveNumericId(int numericId)
        {
            if (numericIdToGuid.TryGetValue(numericId, out var guid))
            {
                RemoveGuid(guid);
            }
        }

        protected virtual void OnDisable()
        {
            ToggleSubs(false);
        }

        protected virtual void OnDestroy()
        {
            ToggleSubs(false);
        }

        protected virtual void OnValidate()
        {
            Refresh();
        }

        /// <summary>
        /// Clears all registered GUIDs. Use with caution!
        /// </summary>
        public virtual void Clear()
        {
            guids.Clear();
            guidToNumericId.Clear();
            numericIdToGuid.Clear();
        }
    }
}