using System;
using UnityEngine;

namespace Amanita.SaveSys.UI
{
    /// <summary>
    /// Manages UI aspects of the save menu as a whole.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class SaveMenuManager : MonoBehaviour, IAmanitaManagerSubmodule
    {
        [SerializeField] private int orderIndex = 0;
        [SerializeField] protected CanvasGroup canvasGroup;
        [Tooltip("If true, the menu will start open.")]
        [SerializeField] protected bool startOpen = false;

        public int OrderIndex => orderIndex;

        public void Init()
        {
            if (IsFullyInitted)
            {
                return;
            }
            IsFullyInitted = true;
        }

        public bool IsFullyInitted { get; protected set; } = false;

        protected virtual void Awake()
        {
            _slotUiManager = GetComponentInChildren<SaveSlotUIManager>();
            OpenLogic = DefaultOpenLogic;
            CloseLogic = DefaultCloseLogic;

            if (startOpen)
            {
                Open(null);
            }
            else
            {
                isOpen = true;
                Close(null);
            }
        }

        private SaveSlotUIManager _slotUiManager;

        /// <summary>
        /// Clients (not necessarily subclasses) should override this when they want to decide what this does when
        /// asked to open.
        /// </summary>
        public Action<object> OpenLogic;

        private void DefaultOpenLogic(object args)
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

        /// <summary>
        /// Clients (not necessarily subclasses) should override this when they want to decide what this does when
        /// asked to close.
        /// </summary>
        public Action<object> CloseLogic;

        private void DefaultCloseLogic(object args)
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

        public virtual void Open(object args)
        {
            OpenLogic(args);
        }

        public virtual void Close(object args)
        {
            CloseLogic(args);
        }

        public virtual void Toggle()
        {
            if (isOpen)
            {
                Close(null);
            }
            else
            {
                Open(null);
            }
        }

        public virtual void Open()
        {
            Open(null);
        }

        public virtual void Close()
        {
            Close(null);
        }

        protected virtual void OnValidate()
        {
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            }
        }
    }
}