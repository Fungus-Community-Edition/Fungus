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

        public CompositeSaveData() { }

        public CompositeSaveData(IList<SaveDataUnit> startingUnits)
        {
            units.AddRange(startingUnits);
        }

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

        public virtual void AddRange(IList<SaveDataUnit> toAdd)
        {
            units.AddRange(toAdd);
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

        public virtual void RemoveRange(IList<SaveDataUnit> toRemove)
        {
            for (int i = 0; i < toRemove.Count; i++)
            {
                SaveDataUnit currentUnitToRemove = toRemove[i];
                Remove(currentUnitToRemove);
            }
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

        public virtual SaveDataUnit GetSingle<T>() where T : ISaveData
        {
            string typeName = typeof(T).Name.ToLower();
            SaveDataUnit result = units.FirstOrDefault(unit => unit.DataTypeName.ToLower() == typeName);
            return result;
        }

        public virtual SaveDataUnit GetSingle(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                Debug.LogError("Cannot get a unit with a null or empty type name.");
                return null;
            }
            SaveDataUnit result = units.FirstOrDefault(unit => unit.DataTypeName.ToLower() == typeName.ToLower());
            return result;
        }

        /// <summary>
        /// Returns a list of all SaveDataUnits of the specified type that this has.
        /// </summary>
        public virtual IList<SaveDataUnit> GetMulti<T>() where T: ISaveData
        {
            string typeName = typeof(T).Name.ToLower();
            IList<SaveDataUnit> result;
            result = units.Where(unit => unit.DataTypeName.ToLower() == typeName).ToList();
            return result;
        }

        public virtual IList<SaveDataUnit> GetMulti(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                Debug.LogError("Cannot get multiple units with a null or empty type name.");
                return new List<SaveDataUnit>();
            }
            IList<SaveDataUnit> result;
            result = units.Where(unit => unit.DataTypeName.ToLower() == typeName.ToLower()).ToList();
            return result;
        }

    }

    public interface ICompositeSaveData : ISaveData
    {
        IList<SaveDataUnit> Units { get; }
    }
}