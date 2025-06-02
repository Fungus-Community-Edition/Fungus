using Amanita.SaveSys;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Amanita.SaveSys
{
    /// <summary>
    /// For save data made up of other instances of save data (which are to be stored
    /// in SaveDataUnit form). One of these should be the main data written to disk.
    /// </summary>
    public class CompositeSaveData : SaveData, IEquatable<CompositeSaveData>
    {
        [SerializeField] protected List<SaveDataUnit> units = new List<SaveDataUnit>();

        public virtual IReadOnlyList<SaveDataUnit> Units => units;

        public virtual void Add(SaveDataUnit unit)
        {
            if (unit == null)
            {
                Debug.LogError($"Cannot add a null SaveDataUnit to {TypeName}.");
                return;
            }
            units.Add(unit);
        }

        public virtual void Remove(SaveDataUnit unit)
        {
            if (unit == null)
            {
                Debug.LogError($"Cannot remove a null SaveDataUnit from {TypeName}.");
                return;
            }
            units.Remove(unit);
        }

        public virtual void Clear()
        {
            units.Clear();
        }

        public virtual bool Equals(CompositeSaveData other)
        {
            bool result = this.Units.SequenceEqual(other.Units);
            return result;
        }

        public override SaveDataUnit Serialized()
        {
            string jsonText = JsonUtility.ToJson(this);
            SaveDataUnit unit = new SaveDataUnit(TypeName, jsonText);
            return unit;
        }
    }

    public interface ICompositeSaveData : ISaveData
    {
        IList<SaveDataUnit> Units { get; }
    }
}