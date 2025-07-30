using UnityEngine;
using Amanita.VScripting;

namespace Amanita.Examples
{
    public class FPDemoPriorityRouter : MonoBehaviour
    {
        public Behaviour[] componentEnabledOutsideFungusPriority;
        public Behaviour[] componentEnabledInsideFungusPriority;

        void OnEnable()
        {
            FungusPrioritySignals.OnFungusPriorityStart += FungusPrioritySignals_OnFungusPriorityStart;
            FungusPrioritySignals.OnFungusPriorityEnd += FungusPrioritySignals_OnFungusPriorityEnd;
        }

        void OnDisable()
        {
            FungusPrioritySignals.OnFungusPriorityStart -= FungusPrioritySignals_OnFungusPriorityStart;
            FungusPrioritySignals.OnFungusPriorityEnd -= FungusPrioritySignals_OnFungusPriorityEnd;
        }

        private void FungusPrioritySignals_OnFungusPriorityEnd()
        {
            foreach (var item in componentEnabledOutsideFungusPriority)
            {
                item.enabled = true;
            }
            foreach (var item in componentEnabledInsideFungusPriority)
            {
                item.enabled = false;
            }
        }

        private void FungusPrioritySignals_OnFungusPriorityStart()
        {
            foreach (var item in componentEnabledOutsideFungusPriority)
            {
                item.enabled = false;
            }
            foreach (var item in componentEnabledInsideFungusPriority)
            {
                item.enabled = true;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}