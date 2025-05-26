using UnityEngine;
using System.Collections.Generic;
using System;

namespace Amanita.SaveSys
{
    /// <summary>
    /// Holds the state for everything that is saved in Amanita's default save system. This
    /// includes the state of the Flowchart and all its variables, as well as
    /// the state of Myceliaudio.
    /// </summary>
    [System.Serializable]
    public class AmanitaSaveData : SaveData, IEquatable<AmanitaSaveData>
    {
        [SerializeField] protected List<SaveDataUnit> units = new List<SaveDataUnit>();
        public virtual IList<SaveDataUnit> Units { get { return units; } }
        // ^For pretty much everything that is to be saved. Flowchart state, Myceliaudio's state, etc.

        public override SaveDataUnit Serialized()
        {
            string json = JsonUtility.ToJson(this, true);
            SaveDataUnit result = new SaveDataUnit(TypeName, json);
            return result;
        }

        public virtual void Add(SaveDataUnit unit)
        {
            if (unit == null)
            {
                Debug.LogError("Cannot add a null SaveDataUnit to AmanitaSaveData.");
                return;
            }
            Units.Add(unit);
        }

        public virtual void RemoveUnit(SaveDataUnit unit)
        {
            if (unit == null)
            {
                Debug.LogError("Cannot remove a null SaveDataUnit from AmanitaSaveData.");
                return;
            }
            Units.Remove(unit);
        }

        public virtual void ClearAllUnits()
        {
            Units.Clear();
        }

        public virtual bool Equals(AmanitaSaveData other)
        {
            bool result = true;
            bool sameUnitCount = this.units.Count == other.units.Count;

            if (!sameUnitCount)
            {
                result = false;
            }
            else
            {
                for (int i = 0; i < this.units.Count; i++)
                {
                    SaveDataUnit unit = this.units[i];
                    SaveDataUnit otherUnit = other.units[i];
                    if (!unit.Equals(otherUnit))
                    {
                        result = false;
                        break;
                    }
                }
            }

            return result;
        }
    }
}