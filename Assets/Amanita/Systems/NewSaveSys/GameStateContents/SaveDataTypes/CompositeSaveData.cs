using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Amanita.FSExt;

namespace Amanita.SaveSys
{
    /// <summary>
    /// For save data made up of other instances of save data.
    /// One of these should be the main data written to disk.
    /// </summary>
    public class CompositeSaveData : SaveData, IEquatable<CompositeSaveData>
    {
        [SerializeField] protected List<SaveData> items = new List<SaveData>();

        public CompositeSaveData() { }

        public CompositeSaveData(IList<SaveData> startingItems)
        {
            if (startingItems != null && startingItems.Count > 0)
            {
                items.AddRange(startingItems);
            }
        }

        public virtual IReadOnlyList<SaveData> Items => items;

        public virtual void Add(SaveData item)
        {
            if (item == null)
            {
                Debug.LogError($"Cannot add a null SaveData to {TypeName}.");
                return;
            }
            items.Add(item);
        }

        public virtual void AddRange(IList<SaveData> toAdd)
        {
            if (toAdd == null) return;
            for (int i = 0; i < toAdd.Count; i++)
            {
                var elem = toAdd[i];
                if (elem != null)
                {
                    items.Add(elem);
                }
            }
        }

        public virtual void Remove(SaveData item)
        {
            if (item == null)
            {
                Debug.LogError($"Cannot remove a null SaveData from {TypeName}.");
                return;
            }
            items.Remove(item);
        }

        public virtual void RemoveRange(IList<SaveData> toRemove)
        {
            if (toRemove == null) return;
            for (int i = 0; i < toRemove.Count; i++)
            {
                var current = toRemove[i];
                Remove(current);
            }
        }

        public virtual void Clear()
        {
            items.Clear();
        }

        public virtual bool Equals(CompositeSaveData other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (items.Count != other.items.Count) return false;

            var serializer = SaveSystem.DefaultSerializer;
            for (int i = 0; i < items.Count; i++)
            {
                var ourItem = items[i];
                var theirItem = other.items[i];
                if (ourItem?.GetType() != theirItem?.GetType()) return false;
                string oursAsJson = serializer.ToJson(ourItem, prettyPrint: false);
                string theirsAsJson = serializer.ToJson(theirItem, prettyPrint: false);
                if (!string.Equals(oursAsJson, theirsAsJson, StringComparison.Ordinal))
                    return false;
            }
            return true;
        }

        public virtual T GetSingle<T>() where T : SaveData
        {
            return items.OfType<T>().FirstOrDefault();
        }

        public virtual SaveData GetSingle(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                Debug.LogError("Cannot get a SaveData with a null or empty type name.");
                return null;
            }
            typeName = typeName.ToLower();
            return items.FirstOrDefault(elem => elem != null && elem.GetType().Name.ToLower() == typeName);
        }

        public virtual IList<T> GetMulti<T>() where T : SaveData
        {
            return items.OfType<T>().ToList();
        }

        public virtual IList<SaveData> GetMulti(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                Debug.LogError("Cannot get multiple SaveData with a null or empty type name.");
                return new List<SaveData>();
            }
            typeName = typeName.ToLower();
            return items.Where(elem => elem != null && elem.GetType().Name.ToLower() == typeName).ToList();
        }

        public static CompositeSaveData CreateFrom(CompositeSaveData other)
        {
            if (other == null) return null;
            return new CompositeSaveData(other.items);
        }
    }

    public interface ICompositeSaveData : ISaveData
    {
        IList<SaveData> Items { get; }
    }
}