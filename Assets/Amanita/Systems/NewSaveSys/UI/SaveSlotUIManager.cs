using UnityEngine;
using System.Collections.Generic;
using System;

namespace Amanita.SaveSys.UI
{
    public class SaveSlotUIManager : MonoBehaviour
    {
        [SerializeField] protected SaveSlotViewComposer _viewComposerPrefab;
        [SerializeField] protected Transform _slotHolder;
        [SerializeField] private int initialSlotCount = 10;

        protected virtual void Awake()
        {
            for (int i = 0; i < initialSlotCount; i++)
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

        protected virtual void OnSaveMetasReadOnInit(IList<ISaveMetaData> list)
        {
            // Pass the metas to the slots
            for (int i = 0; i < _slotUis.Count; i++)
            {
                var slot = _slotUis[i];
                if (i < list.Count)
                {
                    slot.Meta = list[i];
                }
                else
                {
                    slot.Meta = null;
                }
            }
        }

        protected virtual void OnDisable()
        {
            ToggleSubs(false);
        }
    }
}