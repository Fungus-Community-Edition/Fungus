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

        protected virtual void OnEnable()
        {

        }

        protected virtual void ToggleSubs(bool on)
        {
        }

        protected virtual void CreateSaveSlot()
        {
            var slot = Instantiate(_viewComposerPrefab, _slotHolder);
            _slotUis.Add(slot);
        }

        protected IList<SaveSlotViewComposer> _slotUis = new List<SaveSlotViewComposer>();
    }
}