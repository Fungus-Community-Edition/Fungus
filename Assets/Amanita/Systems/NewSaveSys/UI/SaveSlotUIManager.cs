using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Amanita.SaveSys.UI
{
    public class SaveSlotUIManager : MonoBehaviour
    {
        [SerializeField] protected SaveSlotViewComposer _viewComposerPrefab;
        [SerializeField] protected Transform _slotHolder;
        [SerializeField] private int initialSlotCount = 10;

        protected virtual void Awake()
        {
            _slotUis = _slotHolder.GetComponentsInChildren<SaveSlotViewComposer>(true).ToList();
            // ^For when you add slots to the holder in the editor directly
            while (_slotUis.Count < initialSlotCount)
            {
                CreateSaveSlot();
            }
        }

        protected virtual void CreateSaveSlot()
        {
            var slot = Instantiate(_viewComposerPrefab, _slotHolder);
            slot.transform.localScale = _viewComposerPrefab.transform.localScale;
            // ^There's a weird Unity bug where instantiated UI prefabs start out with a scale they shouldn't have, 
            // and thus to compensate...
            _slotUis.Add(slot);
        }

        protected IList<SaveSlotViewComposer> _slotUis = new List<SaveSlotViewComposer>();

        protected virtual void OnEnable()
        {
            ToggleSubs(true);
        }

        protected virtual void ToggleSubs(bool on)
        {
            if (on)
            {
                SaveSysSignals.SaveMetasReadOnInit += OnSaveMetasReadOnInit;
            }
            else
            {
                SaveSysSignals.SaveMetasReadOnInit -= OnSaveMetasReadOnInit;
            }
        }

        public virtual void Refresh()
        {
            for (int i = 0; i < _slotUis.Count; i++)
            {
                _slotUis[i].Refresh();
            }
        }

        protected virtual void OnSaveMetasReadOnInit(IList<ISaveMetaData> metas)
        {
            #region Pass the metas to the slot uis
            for (int i = 0; i < _slotUis.Count; i++)
            {
                var slot = _slotUis[i];
                if (i < metas.Count)
                {
                    slot.Meta = metas[i];
                }
                else
                {
                    // A filler meta so the slots at least display their slot numbers correctly
                    SaveMetaData fillerMeta = new SaveMetaData()
                    {
                        SaveName = "Empty Slot",
                        SlotNumber = i + 1,
                        SaveVersion = string.Empty,
                    };
                    slot.Meta = fillerMeta;
                }
            }
            #endregion
        }

        protected virtual void OnDisable()
        {
            ToggleSubs(false);
        }
    }
}