using UnityEditor;
using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Used by the Flowchart window to serialize the currently active Flowchart object
    /// so that the same Flowchart can be displayed while editing & playing.
    /// </summary>
    [AddComponentMenu("")]
    public class AmanitaState : MonoBehaviour
    {
        [SerializeField] protected Flowchart selectedFlowchart;

        #region Public members

        /// <summary>
        /// The currently selected Flowchart.
        /// </summary>
        public virtual Flowchart SelectedFlowchart
        {
            get { return selectedFlowchart; }
            set { selectedFlowchart = value; }
        }

        #endregion

        public virtual void Refresh()
        {
            if (Selection.activeGameObject != null)
            {
                Flowchart fcFound = Selection.activeGameObject.GetComponent<Flowchart>();
                if (fcFound != null || selectedFlowchart == null)
                {
                    selectedFlowchart = fcFound;
                }
            }
        }
    }
}