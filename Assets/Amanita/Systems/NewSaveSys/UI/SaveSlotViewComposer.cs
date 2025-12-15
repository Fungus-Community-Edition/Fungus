using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Type = System.Type;

namespace Amanita.SaveSys.UI
{
    /// <summary>
    /// For tying together multiple ISaveSlotUIView components under one controller.
    /// </summary>
    public class SaveSlotViewComposer : MonoBehaviour
    {
        [SerializeField] protected GameObject holdsViews;
        [SerializeField] protected Button button;

        public virtual ISaveMetaData Meta
        {
            get { return meta; }
            set
            {
                meta = value;
                OnMetaUpdated();
            }
        }
        protected ISaveMetaData meta;

        protected virtual void OnMetaUpdated()
        {
            // This method can be overridden to perform additional actions when Meta is set.
            PassMetaToViews();
        }

        protected virtual void PassMetaToViews()
        {
            for (int i = 0; i < views.Count; i++)
            {
                ISaveSlotView currentView = views[i];
                if (currentView != null)
                {
                    // We assume that the views refresh themselves when the meta data is set.
                    currentView.Meta = Meta;
                }
                else
                {
                    Debug.LogWarning($"View at index {i} is null. Cannot pass meta data.");
                }
            }
        }

        protected virtual void Awake()
        {
            if (button == null)
            {
                button = gameObject.GetOrAddComponent<Button>();
            }
            EnsureViews();
        }

        protected virtual void EnsureViews()
        {
            bool alreadyEnsured = views != null && views.Count > 0;
            if (alreadyEnsured)
            {
                return;
            }

            if (holdsViews == null)
            {
                holdsViews = gameObject;
            }

            views = holdsViews.GetComponentsInChildren<ISaveSlotView>();
        }

        protected IList<ISaveSlotView> views;

        public virtual TView GetView<TView>()
            where TView : ISaveSlotView
        {
            return (TView)GetView(typeof(TView));
        }

        public virtual ISaveSlotView GetView(Type viewType)
        {
            bool typeIsValid = saveSlotViewInterfaceType.IsAssignableFrom(viewType);
            if (!typeIsValid)
            {
                Debug.LogError($"Requested view type {viewType} does not implement ISaveSlotView.");
                return null;
            }

            EnsureViews();
            bool failedToEnsure = views == null || views.Count == 0;
            // ^Probably happened because the composer's prefab wasn't set up properly.
            if (failedToEnsure)
            {
                Debug.LogWarning("No views found. Ensure that SaveSlotViewComposer is properly initialized.");
                return null;
            }

            for (int i = 0; i < views.Count; i++)
            {
                bool validView = views[i] != null;
                if (!validView)
                {
                    Debug.LogError($"View at index {i} is null. This may indicate a misconfigured prefab.");
                    continue;
                }

                bool correctType = views[i] != null && viewType.IsAssignableFrom(views[i].GetType());
                if (correctType)
                {
                    return views[i];
                }
            }

            Debug.LogWarning($"No view of type {viewType.FullName} found.");
            return null;
        }

        protected static Type saveSlotViewInterfaceType = typeof(ISaveSlotView);

        protected virtual void OnEnable()
        {
            ToggleSubs(true);
        }

        protected virtual void ToggleSubs(bool on)
        {
            if (on)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
            else
            {
                button.onClick.RemoveListener(OnButtonClicked);
            }
        }

        private void OnButtonClicked()
        {
            SaveSysSignals.SaveSlotSelected?.Invoke(SlotNumber);
        }

        public virtual int SlotNumber
        {
            get
            {
                if (Meta != null)
                {
                    return Meta.SlotNumber;
                }
                else
                {
                    return transform.GetSiblingIndex() + 1; // We don't want 0 to be a valid slot number.
                }
            }
        }

        protected virtual void OnDisable()
        {
            ToggleSubs(false);
        }

        protected virtual void OnValidate()
        {             
            if (holdsViews == null)
            {
                holdsViews = gameObject;
            }
        }

        public virtual void Refresh()
        {
            for (int i = 0; i < views.Count; i++)
            {
                views[i].Refresh();
            }
        }
    }
}