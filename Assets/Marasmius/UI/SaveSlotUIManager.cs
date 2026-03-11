using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace AtMycelia.SaveSys.UI
{
    public class SaveSlotUIManager : MonoBehaviour
    {
        [SerializeField] private SaveSlotViewComposer _viewComposerPrefab;
        [SerializeField] private Transform _slotHolder;
        [SerializeField] private int initialSlotCount = 10;

        protected virtual void Awake()
        {
            // We're going to avoid using Singletons for this one; users may want separate menus
            // for saving and loading, each with their own slot UI manager.
            _slotUis = _slotHolder.GetComponentsInChildren<SaveSlotViewComposer>(true).ToList();
            // ^For when you add slots to the holder in the editor directly
            while (_slotUis.Count < initialSlotCount)
            {
                CreateSaveSlot();
            }
        }

        protected IList<SaveSlotViewComposer> _slotUis = new List<SaveSlotViewComposer>();

        protected virtual void CreateSaveSlot()
        {
            var slot = Instantiate(_viewComposerPrefab, _slotHolder);
            slot.transform.localScale = _viewComposerPrefab.transform.localScale;
            // ^There's a weird Unity bug where instantiated UI prefabs start out with a scale they shouldn't have, 
            // and thus to compensate...
            _slotUis.Add(slot);
        }

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
                ISaveMetaData metaToAssign;
                DecideMetaToAssign();
                void DecideMetaToAssign()
                {
                    if (i < metas.Count)
                    {
                        metaToAssign = metas[i];
                    }
                    else
                    {
                        ISaveMetaData fillerMeta = new SaveMetaData()
                        {
                            SaveName = "",
                            SlotNumber = i + 1, // +1 because slot numbers are 1-based
                            SaveVersion = string.Empty,
                        };
                        // ^This is so the slots at least display their slot numbers correctly
                        metaToAssign = fillerMeta;
                    }
                }

                var slot = _slotUis[i];
                slot.Meta = metaToAssign;
            }
            #endregion
        }

        protected virtual void OnDisable()
        {
            ToggleSubs(false);
        }

#if UNITY_EDITOR
        // For debug and testing purposes
        public IList<SaveSlotViewComposer> SlotUIs => new List<SaveSlotViewComposer>(_slotUis);

#endif
    }
}