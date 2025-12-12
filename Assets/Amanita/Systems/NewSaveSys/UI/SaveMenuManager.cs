using UnityEngine;

namespace Amanita.SaveSys.UI
{
    /// <summary>
    /// Manages UI aspects of the save menu as a whole.
    /// </summary>
    public class SaveMenuManager : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        

        protected virtual void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                bool stillNothing = canvasGroup == null;
                if (stillNothing)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            _slotUiManager = GetComponentInChildren<SaveSlotUIManager>();
        }

        private SaveSlotUIManager _slotUiManager;

        public virtual void Open()
        {
            _slotUiManager.Refresh();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public virtual void Close()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        protected virtual void OnValidate()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }
    }
}