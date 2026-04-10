using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lorekeeper.EditorCode
{
    [Serializable]
    public class LorekeeperSettings
    {
        #region Serializable Fields and their Properties
        [SerializeField] protected List<string> blacklist = new List<string>() { "/Lorekeeper" };

        /// <summary>
        /// For paths relative to the Assets folder. The setter replaces the list's contents 
        /// but keeps the list reference.
        /// </summary>
        public virtual IList<string> Blacklist
        {
            get { return blacklist.AsReadOnly(); }
            set
            {
                blacklist.Clear();
                if (value == null)
                {
                    return;
                }

                for (int i = 0; i < value.Count; i++)
                {
                    AddExclusion(value[i]);
                }
            }
        }
        #endregion

        #region Asset Folder Exclusion Methods
        public virtual void AddExclusion(string path)
        {
            path = LKUtils.EnsureForwardSlashAtStart(path);
            blacklist.Add(path);
        }

        public virtual void ChangeExclusion(int index, string newPath)
        {
            if (index < 0 || index >= blacklist.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range.");
            }
            newPath = LKUtils.EnsureForwardSlashAtStart(newPath);
            blacklist[index] = newPath;
        }

        public virtual void RemoveExclusion(string path)
        {
            path = LKUtils.EnsureForwardSlashAtStart(path);
            blacklist.Remove(path);
        }

        public virtual void RemoveExclusionByIndex(int index)
        {
            if (index < 0 || index >= blacklist.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }
            blacklist.RemoveAt(index);
        }
        #endregion

        public virtual void Clear()
        {
            blacklist.Clear();
        }

        public virtual void OnDeserialize()
        {
            for (int i = 0; i < blacklist.Count; i++)
            {
                blacklist[i] = LKUtils.EnsureForwardSlashAtStart(blacklist[i]);
            }
        }
    }
}