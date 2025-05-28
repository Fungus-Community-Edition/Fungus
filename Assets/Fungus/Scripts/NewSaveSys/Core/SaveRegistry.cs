using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SavePairDict = System.Collections.Generic.IDictionary<int, Amanita.SaveSys.SaveDataSet>;

namespace Amanita.SaveSys
{
    public class SaveRegistry
    {
        protected readonly SavePairDict _savePairs = new Dictionary<int, SaveDataSet>();

        public virtual void AddSave(SaveDataSet dataSet)
        {
            if (!_savePairs.ContainsKey(dataSet.SlotNumber))
            {
                _savePairs.Add(dataSet.SlotNumber, dataSet);
                SaveSysSignals.SaveAddedToSlot(dataSet);
            }
            else
            {
                _savePairs[dataSet.SlotNumber] = dataSet; // Overwriting
                SaveSysSignals.SaveInSlotOverwritten(dataSet);
            }
        }

        public virtual void RemoveSave(int slotToRemoveFrom)
        {
            if (_savePairs.ContainsKey(slotToRemoveFrom))
            {
                SaveDataSet dataSet = _savePairs[slotToRemoveFrom];
                _savePairs.Remove(slotToRemoveFrom);
                SaveSysSignals.SaveRemovedFromSlot(dataSet);
            }
        }

        public virtual IList<SaveMetaData> GetMultiSaveMetas(IEnumerable<int> multiSlotsToGetFrom)
        {
            IList<SaveMetaData> metasFound = new List<SaveMetaData>();

            foreach (int slot in multiSlotsToGetFrom)
            {
                if (_savePairs.TryGetValue(slot, out SaveDataSet dataSet))
                {
                    // If the pairs have anything linked to the slot number, we know that the
                    // meta there is valid. Safety checks we implemented and such.
                    // No need for a null-check there
                    metasFound.Add(dataSet.Meta);
                }
                else
                {
                    ReportMissingSlotWarning(slot, nameof(SaveMetaData));
                    // ^If this warning gets logged here, something seriously wrong happened
                }
            }

            return metasFound;
        }

        protected virtual void ReportMissingSlotWarning(int slot, string dataType)
        {
            Debug.LogWarning($"Could not find {dataType} assigned to Slot {slot}. Possible causes:\n- The slot was never assigned anything\n- The file the slot corresponds to was deleted.");
        }

        public virtual SaveMetaData GetSaveMeta(int slotToGetFrom)
        {
            bool foundMeta = _savePairs.TryGetValue(slotToGetFrom, out SaveDataSet dataSet);

            if (!foundMeta)
            {
                ReportMissingSlotWarning(slotToGetFrom, nameof(SaveMetaData));
            }

            return dataSet?.Meta;
        }

        public virtual IList<SaveMetaData> GetAllSaveMetas()
        {
            // Thanks to the safety checks, the metas we have registered
            // should all be valid. Thus, no need to check the validity here
            IList<SaveMetaData> result = (from elem in _savePairs.Values
                                          select elem.Meta).ToList();
            return result;
        }

        public virtual IList<SaveData> GetMultiMainSaves(IEnumerable<int> multiSlotsToGetFrom)
        {
            IList<SaveData> savesFound = (from int slotNum in multiSlotsToGetFrom
                                          where GetMainSave(slotNum) != null
                                          select GetMainSave(slotNum)).ToList();

            // Same as with the invalid meta requests
            ReportInvalidMainDataRequests();
            void ReportInvalidMainDataRequests()
            {
                IList<int> invalidSlotNums = (from slotNum in multiSlotsToGetFrom
                                              where !_savePairs.ContainsKey(slotNum)
                                              select slotNum).ToList();

                for (int i = 0; i < invalidSlotNums.Count; i++)
                {
                    int invalidNum = invalidSlotNums[i];
                    ReportMissingSlotWarning(invalidNum, nameof(SaveData));
                }
            }

            return savesFound;

        }

        public virtual SaveData GetMainSave(int slotToGetFrom)
        {
            SaveData result = null;

            if (_savePairs.ContainsKey(slotToGetFrom))
            {
                result = _savePairs[slotToGetFrom].MainData;
            }
            else
            {
                ReportMissingSlotWarning(slotToGetFrom, nameof(SaveData));
            }

            return result;
        }

        public virtual IList<SaveData> GetAllMainSaves()
        {
            IList<SaveData> savesFound = (from elem in _savePairs.Values
                                          where elem.MainData != null
                                          select elem.MainData).ToList();
            return savesFound;
        }

        // Implying there is at least a meta assigned to that slot
        public virtual bool HasSaveInSlot(int slotNumber)
        {
            return _savePairs.ContainsKey(slotNumber);
        }

        public virtual bool HasMainSaveInSlot(int slotNumber)
        {
            return HasSaveInSlot(slotNumber) && 
                _savePairs[slotNumber].MainData != null;
        }

        public virtual bool HasSavesInAll(IEnumerable<int> slotsToConsider)
        {
            bool result = slotsToConsider.All(slot => HasSaveInSlot(slot));
            return result;
        }

        public virtual bool HasSaveInAtLeastOneSlot(IEnumerable<int> slotsToConsider)
        {
            bool result = slotsToConsider.Any(slot => HasSaveInSlot(slot));
            return result;
        }

        /// <summary>
        /// If that save is NOT assigned to a slot, this returns -1
        /// </summary>
        public virtual int GetSlotNumberOf(SaveData mainData)
        {
            int slotNumber = -1;

            foreach (int key in _savePairs.Keys)
            {
                SaveDataSet value = _savePairs[key];
                if (value.MainData == mainData)
                {
                    slotNumber = value.Meta.SlotNumber;
                    break;
                }
            }

            return slotNumber;
        }
        
    }

}