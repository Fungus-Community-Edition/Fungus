using UnityEngine;
using UnityPhysics = UnityEngine.Physics;
using AtMycelia.Amanita.VScripting;

namespace AtMycelia.Amanita.Examples
{
    public class LookingAtDoor : MonoBehaviour
    {
        public Collider doorCol;
        public float gazeTime = 0.2f;
        private float gazeCounter = 0;
        public BlockReference runBlockWhenGazed;
        public Transform eye;

        [ContentTypeConstraint(typeof(bool))]
        public VariableReference fungusBoolHasGazed;


        public void ActivateNow()
        {
            enabled = true;

            if (fungusBoolHasGazed.Variable == null)
            {
                string errorMessage = "LookingAtDoor: No variable set for fungusBoolHasGazed. Please set one in the inspector.";
                Debug.LogError(errorMessage);
            }
        }

        private void Update()
        {
            var curCounter = gazeCounter;
            RaycastHit hit;
            if (UnityPhysics.Raycast(eye.position, eye.forward, out hit))
            {
                if (hit.collider == doorCol)
                {
                    gazeCounter += Time.deltaTime;
                }
                else
                {
                    gazeCounter = 0;
                }
            }
            else
            {
                gazeCounter = 0;
            }

            if (gazeCounter >= gazeTime && curCounter <= gazeTime)
            {
                runBlockWhenGazed.Execute();
                fungusBoolHasGazed.SetValue(true);
            }
        }
    }
}