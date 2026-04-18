#if UNITY_EDITOR
using System;
using UnityEditor;
#endif
using UnityEngine;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Used by the Flowchart window to serialize the currently active Flowchart object
    /// so that the same Flowchart can be displayed while editing & playing.
    /// </summary>
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    public class LegacyFungusState : MonoBehaviour
    {
        [SerializeField] protected Flowchart selectedFlowchart;
        [SerializeField] protected Flowchart lastSelectedFc;

        private void Start()
        {
            Refresh();
        }

        #region Public members

        /// <summary>
        /// The currently selected Flowchart.
        /// </summary>
        public virtual Flowchart SelectedFlowchart
        {
            get { return selectedFlowchart; }
            set { selectedFlowchart = value; }
        }

        public virtual Flowchart LastSelectedFlowchart
        {
            get { return lastSelectedFc; }
            set { lastSelectedFc = value; }
        }

        #endregion

#if UNITY_EDITOR
        protected virtual void OnEnable()
        {
            ToggleSubs(false);
            ToggleSubs(true);
        }

        protected virtual void ToggleSubs(bool on)
        {
            if (on)
            {
                Selection.selectionChanged += Refresh;
            }
            else
            {
                Selection.selectionChanged -= Refresh;
            }
        }

        protected virtual void OnDisable()
        {
            ToggleSubs(false);
        }

        public virtual void Refresh()
        {
            GameObject activeGo = Selection.activeGameObject;
            if (activeGo != null)
            {
                activeGo.TryGetComponent(out Flowchart fcFound);
                if (fcFound != null && fcFound != selectedFlowchart)
                {
                    if (selectedFlowchart != null)
                    {
                        lastSelectedFc = selectedFlowchart;
                    }
                    selectedFlowchart = fcFound;
                    SelectedFlowchartChanged?.Invoke(selectedFlowchart);
                }
            }

        }

        public event Action<Flowchart> SelectedFlowchartChanged = delegate { };
#endif

    }
}