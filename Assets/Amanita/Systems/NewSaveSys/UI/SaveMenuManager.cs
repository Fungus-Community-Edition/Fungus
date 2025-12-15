using UnityEngine;

namespace Amanita.SaveSys.UI
{
    /// <summary>
    /// Manages UI aspects of the save menu as a whole.
    /// </summary>
    public class SaveMenuManager : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        [Tooltip("If true, the menu will start open.")]
        [SerializeField] protected bool startOpen = false;

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
            if (startOpen)
            {
                Open();
            }
            else
            {
                Close();
            }
        }

        private SaveSlotUIManager _slotUiManager;

        public virtual void Open()
        {
            if (isOpen)
            {
                return;
            }
            _slotUiManager.Refresh();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            isOpen = true;
            SaveSysSignals.SaveMenuOpened();
        }

        private bool isOpen;

        public virtual void Close()
        {
            if (!isOpen)
            {
                return;
            }
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            isOpen = false;
            SaveSysSignals.SaveMenuClosed();
        }

        public virtual void Toggle()
        {
            if (isOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
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