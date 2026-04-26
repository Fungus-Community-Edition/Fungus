using UnityEngine;
using UnityPhysics = UnityEngine.Physics;
using AtMycelia.Hyphlow;

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
        [SerializeField]
        protected VariableReference _hasGazedBool = new VariableReference();

        [ContentTypeConstraint(typeof(bool))]
        [SerializeField]
        protected VariableReference _isCompleteBool = new VariableReference();

        public void ActivateNow()
        {
            enabled = true;

            if (_hasGazedBool.Variable == null)
            {
                string errorMessage = "LookingAtDoor: No variable set for hasGazedBool. " +
                    "Please set one in the inspector.";
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

            if (gazeCounter >= gazeTime && curCounter <= gazeTime && !_isCompleteBool.GetValue<bool>())
            {
                runBlockWhenGazed.Execute();
                _hasGazedBool.SetValue(true);
            }
        }
    }
}