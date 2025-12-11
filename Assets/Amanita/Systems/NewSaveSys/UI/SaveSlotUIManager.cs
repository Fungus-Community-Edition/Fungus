using UnityEngine;
using System.Collections.Generic;

namespace Amanita.SaveSys.UI
{
    public class SaveSlotUIManager : MonoBehaviour
    {
        [SerializeField] protected SaveSlotViewComposer _viewComposerPrefab;
        [SerializeField] protected Transform _slotHolder;
        [SerializeField] protected int initialSlotCount = 10;

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

        }

        protected virtual void ToggleSubs(bool on)
        {
        }

        
    }
}