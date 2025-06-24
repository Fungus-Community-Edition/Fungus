using System.Collections.Generic;
using UnityEngine;

namespace Amanita.SaveSys.UI
{ 
    public class SaveSlotUIViewController : MonoBehaviour
    {
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
                ISaveSlotUIView currentView = views[i];
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
            views = new List<ISaveSlotUIView>(GetComponentsInChildren<ISaveSlotUIView>());
        }

        protected IList<ISaveSlotUIView> views;
    }
}